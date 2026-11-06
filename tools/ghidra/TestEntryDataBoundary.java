// Invented binary fixture only; exercises guard rejection, rollback and metadata repair.
// @category Restoration
import java.util.*;
import com.google.gson.*;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.*;
import ghidra.program.model.listing.*;

public class TestEntryDataBoundary extends GhidraScript {
    private JsonObject plan() {
        JsonObject p=JsonParser.parseString("{\"version\":1,\"sourceMd5\":\"PLACEHOLDER\",\"oldEntry\":\"00001000\",\"oldBody\":\"00001000..00001020\",\"functions\":[{\"entry\":\"00001000\",\"range\":\"00001000..00001008\"},{\"entry\":\"00001010\",\"range\":\"00001010..00001020\"}],\"data\":[{\"range\":\"00001008..0000100A\",\"kind\":\"word\"},{\"range\":\"0000100A..00001010\",\"kind\":\"bytes\"}]}").getAsJsonObject();
        p.addProperty("sourceMd5",currentProgram.getExecutableMD5()); return p;
    }
    private String state() throws Exception {
        StringBuilder s=new StringBuilder();
        for(Function f:currentProgram.getFunctionManager().getFunctions(true))s.append(f.getEntryPoint()).append(f.getBody());
        for(Instruction i:currentProgram.getListing().getInstructions(true))s.append("I").append(i.getAddress()).append(':').append(i.getLength());
        for(Data d:currentProgram.getListing().getDefinedData(true))s.append("D").append(d.getAddress()).append(':').append(d.getLength());
        byte[] b=new byte[64];currentProgram.getMemory().getBytes(toAddr(0x1000),b);return s+Arrays.toString(b);
    }
    private void reject(JsonObject p,String name) throws Exception {
        String before=state(); boolean rejected=false;
        try { ApplyEntryDataBoundary.apply(currentProgram,p,monitor); }
        catch(Exception expected) { rejected=true; println("EXPECTED_REJECTION\t"+name+"\t"+expected.getMessage()); }
        if(!rejected || !before.equals(state()))throw new IllegalStateException("Rejection/rollback failed: "+name+" rejected="+rejected+" before="+before+" after="+state());
        println("BOUNDARY_REJECTED\t"+name);
    }
    protected void run() throws Exception {
        disassemble(toAddr(0x1000)); disassemble(toAddr(0x1020));
        Function old=createFunction(toAddr(0x1000),"synthetic_combined");
        old.setBody(new AddressSet(toAddr(0x1000),toAddr(0x101f)));
        createFunction(toAddr(0x1020),"synthetic_untouched");
        end(true); // Commit fixture setup; the repair owns its independent rollback boundary.
        int enclosing=currentProgram.startTransaction("synthetic enclosing owner");
        try { reject(plan(),"enclosing-transaction"); }
        finally { currentProgram.endTransaction(enclosing,true); }
        JsonObject p=plan();p.addProperty("sourceMd5","00000000000000000000000000000000");reject(p,"identity");
        p=plan();p.addProperty("extra",1);reject(p,"unknown-key");
        p=plan();p.getAsJsonArray("data").get(0).getAsJsonObject().addProperty("range","00001007..0000100A");reject(p,"overlap");
        p=plan();p.getAsJsonArray("data").remove(1);reject(p,"incomplete");
        p=plan();p.getAsJsonArray("data").get(0).getAsJsonObject().addProperty("kind","string");reject(p,"data-kind");
        p=plan();p.addProperty("oldBody","00001000..00001021");reject(p,"old-extent");
        start(); old.setBody(new AddressSet(toAddr(0x1000),toAddr(0x101e))); end(true);
        reject(plan(),"old-body");
        start(); old.setBody(new AddressSet(toAddr(0x1000),toAddr(0x101f))); end(true);
        p=plan();p.getAsJsonArray("functions").get(0).getAsJsonObject().addProperty("range","00001000..00001002");
        p.getAsJsonArray("data").get(0).getAsJsonObject().addProperty("range","00001002..00001004");
        p.getAsJsonArray("data").get(1).getAsJsonObject().addProperty("range","00001004..00001010");reject(p,"instruction-cut");
        String result=ApplyEntryDataBoundary.apply(currentProgram,plan(),monitor);
        if(!result.startsWith("REPAIRED codeBytes=24 dataBytes=8 untouched=1"))throw new IllegalStateException(result);
        if(!currentProgram.getFunctionManager().getFunctionAt(toAddr(0x1000)).getName().equals("synthetic_combined"))
            throw new IllegalStateException("Old entry metadata was not retained");
        String after=state();
        if(!ApplyEntryDataBoundary.apply(currentProgram,plan(),monitor).equals("ALREADY_REPAIRED") || !after.equals(state()))
            throw new IllegalStateException("Idempotence failed");
        println("ENTRY_DATA_SYNTHETIC_COMPLETE\tguards=9 rollback=true bytesUnchanged=true untouched=true idempotent=true");
    }
}
