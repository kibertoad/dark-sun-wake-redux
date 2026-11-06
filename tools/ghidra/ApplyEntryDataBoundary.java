// Bounded, source-bound Ghidra metadata repair. Never changes program bytes.
// Arguments: JSON plan. See RUNTIME-ERROR-INVENTORY.md. Run on an isolated copy.
// @category Restoration
import java.nio.file.*;
import java.util.*;
import com.google.gson.*;
import ghidra.app.cmd.disassemble.DisassembleCommand;
import ghidra.app.script.GhidraScript;
import ghidra.program.flatapi.FlatProgramAPI;
import ghidra.program.model.address.*;
import ghidra.program.model.data.*;
import ghidra.program.model.listing.*;
import ghidra.util.task.TaskMonitor;

public class ApplyEntryDataBoundary extends GhidraScript {
    private record Part(Address low, Address high, String kind) {
        AddressSet set() { return new AddressSet(low, high); }
        int size() { return (int)set().getNumAddresses(); }
    }
    private static Part part(FlatProgramAPI api, String range, String kind) {
        String[] pair=range.split("\\.\\.");
        if(pair.length!=2) throw new IllegalArgumentException("Expected half-open range");
        Address low=api.toAddr(pair[0]), stop=api.toAddr(pair[1]);
        if(low==null || stop==null || low.compareTo(stop)>=0 || stop.subtract(low)>4096)
            throw new IllegalArgumentException("Invalid bounded range");
        return new Part(low,stop.subtract(1),kind);
    }
    private static void keys(JsonObject o, String... allowed) {
        if(!o.keySet().equals(Set.of(allowed))) throw new IllegalArgumentException("Unexpected or missing plan keys");
    }
    private static DataType type(Part p) {
        if(p.kind.equals("word")) return new WordDataType();
        return new ArrayDataType(new ByteDataType(),p.size(),1);
    }
    private static Map<String,String> bodies(Program p, Set<Address> except) {
        Map<String,String> result=new TreeMap<>();
        for(Function f:p.getFunctionManager().getFunctions(true))
            if(!except.contains(f.getEntryPoint())) result.put(f.getEntryPoint().toString(),f.getBody().toString());
        return result;
    }
    private static boolean finalState(Program p, List<Part> code, List<Part> data) {
        Listing listing=p.getListing();
        AddressSet scope=new AddressSet(); Set<Address> entries=new HashSet<>();
        for(Part x:code) { scope.add(x.set()); entries.add(x.low); }
        for(Part x:data) scope.add(x.set());
        for(Function f:p.getFunctionManager().getFunctions(true))
            if(!entries.contains(f.getEntryPoint()) && !f.getBody().intersect(scope).isEmpty())return false;
        for(Part x:code) {
            Function f=p.getFunctionManager().getFunctionAt(x.low);
            if(f==null || !x.set().equals(f.getBody())) return false;
            AddressSet instructions=new AddressSet();
            for(Instruction i:listing.getInstructions(x.set(),true)) instructions.add(i.getMinAddress(),i.getMaxAddress());
            if(!instructions.equals(x.set())) return false;
        }
        for(Part x:data) {
            Data d=listing.getDataAt(x.low);
            if(d==null || d.getLength()!=x.size() || !d.getDataType().isEquivalent(type(x))) return false;
            if(listing.getInstructions(x.set(),true).hasNext()) return false;
        }
        return true;
    }
    public static String apply(Program program, JsonObject plan, TaskMonitor monitor) throws Exception {
        if(program.getCurrentTransactionInfo()!=null)
            throw new IllegalStateException("Boundary repair requires no enclosing transaction");
        keys(plan,"version","sourceMd5","oldEntry","oldBody","functions","data");
        if(plan.get("version").getAsInt()!=1) throw new IllegalArgumentException("Unsupported boundary plan version");
        String md5=plan.get("sourceMd5").getAsString();
        if(!md5.matches("[0-9a-f]{32}") || !md5.equalsIgnoreCase(program.getExecutableMD5()))
            throw new IllegalArgumentException("Boundary source MD5 mismatch");
        FlatProgramAPI api=new FlatProgramAPI(program,monitor);
        Part whole=part(api,plan.get("oldBody").getAsString(),"old");
        Address oldEntry=api.toAddr(plan.get("oldEntry").getAsString());
        if(!whole.low.equals(oldEntry) || !program.getMemory().contains(whole.set()))
            throw new IllegalArgumentException("Old entry/range is not mapped");
        List<Part> code=new ArrayList<>(), data=new ArrayList<>();
        if(plan.getAsJsonArray("functions").size()!=2 || plan.getAsJsonArray("data").size()>8)
            throw new IllegalArgumentException("Require two code parts and at most eight data parts");
        for(JsonElement e:plan.getAsJsonArray("functions")) {
            JsonObject o=e.getAsJsonObject(); keys(o,"entry","range");
            Part x=part(api,o.get("range").getAsString(),"code");
            if(!x.low.equals(api.toAddr(o.get("entry").getAsString())))
                throw new IllegalArgumentException("Function entry is not its range start");
            code.add(x);
        }
        if(code.stream().noneMatch(x->x.low.equals(oldEntry))) throw new IllegalArgumentException("Old entry is not retained");
        for(JsonElement e:plan.getAsJsonArray("data")) {
            JsonObject o=e.getAsJsonObject(); keys(o,"range","kind");
            String kind=o.get("kind").getAsString();
            Part x=part(api,o.get("range").getAsString(),kind);
            if(!Set.of("word","bytes").contains(kind) || (kind.equals("word") && x.size()!=2))
                throw new IllegalArgumentException("Invalid data kind/extent");
            data.add(x);
        }
        AddressSet partition=new AddressSet();
        List<Part> parts=new ArrayList<>(code); parts.addAll(data);
        for(Part x:parts) {
            if(!whole.set().contains(x.set()) || !partition.intersect(x.set()).isEmpty())
                throw new IllegalArgumentException("Overlapping or out-of-range partition");
            partition.add(x.set());
        }
        if(!partition.equals(whole.set())) throw new IllegalArgumentException("Incomplete boundary partition");
        if(finalState(program,code,data)) return "ALREADY_REPAIRED";
        FunctionManager fm=program.getFunctionManager();
        Function old=fm.getFunctionAt(oldEntry);
        if(old==null || !old.getBody().equals(whole.set())) throw new IllegalArgumentException("Old function body mismatch");
        for(Function f:fm.getFunctions(true))
            if(!f.getEntryPoint().equals(oldEntry) && !f.getBody().intersect(whole.set()).isEmpty())
                throw new IllegalArgumentException("Another function intersects boundary");
        Map<String,String> untouched=bodies(program,Set.of(oldEntry));
        byte[] bytes=new byte[whole.size()];
        if(program.getMemory().getBytes(whole.low,bytes)!=bytes.length) throw new IllegalStateException("Short memory read");
        int transaction=program.startTransaction("Bounded entry/data metadata repair");
        boolean commit=false;
        try {
            // Retain the old entry's name/signature and change only its body.
            old.setBody(code.stream().filter(x->x.low.equals(oldEntry)).findFirst().orElseThrow().set());
            api.clearListing(whole.low,whole.high);
            for(Part x:data) api.createData(x.low,type(x));
            for(Part x:code) {
                DisassembleCommand cmd=new DisassembleCommand(x.low,x.set(),true);
                cmd.enableCodeAnalysis(false);
                if(!cmd.applyTo(program,monitor)) throw new IllegalStateException("Bounded disassembly failed");
                Function f=fm.getFunctionAt(x.low);
                if(f==null)f=api.createFunction(x.low,null);
                if(f==null) throw new IllegalStateException("Boundary function creation failed");
                f.setBody(x.set());
            }
            if(!finalState(program,code,data)) throw new IllegalStateException("Instruction/data boundary postcondition failed");
            Set<Address> changed=new HashSet<>(); for(Part x:code)changed.add(x.low);
            if(!untouched.equals(bodies(program,changed))) throw new IllegalStateException("Unrelated function body changed");
            byte[] after=new byte[bytes.length]; program.getMemory().getBytes(whole.low,after);
            if(!Arrays.equals(bytes,after)) throw new IllegalStateException("Program bytes changed");
            commit=true;
        } finally {
            // FlatProgramAPI starts its own nested transaction for mutating helpers.
            // Close that owner before ending our outer rollback boundary.
            api.end(commit);
            program.endTransaction(transaction,commit);
        }
        return "REPAIRED codeBytes="+code.stream().mapToInt(Part::size).sum()+" dataBytes="+data.stream().mapToInt(Part::size).sum()+" untouched="+untouched.size();
    }
    protected void run() throws Exception {
        String[] args=getScriptArgs();
        if(args.length!=1)throw new IllegalArgumentException("Supply one bounded entry/data plan");
        if(!isRunningHeadless())throw new IllegalArgumentException("Use an isolated headless project");
        Path input=Path.of(args[0]);
        if(Files.size(input)>65536)throw new IllegalArgumentException("Boundary plan exceeds 64 KiB");
        JsonObject plan=JsonParser.parseString(Files.readString(input)).getAsJsonObject();
        end(true);
        println("ENTRY_DATA_COMPLETE\t"+apply(currentProgram,plan,monitor));
    }
}
