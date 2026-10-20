// Applies the declared flows of tools/ghidra/dsun-declared-flows.json to one view of DSUN.EXE.
// Arguments: the flows file, the view (resident or overlay) and, for the overlay view, the
// ReportFbovOverlayMap.ps1 mapping, which turns each overlay address's shipped-file offset into
// its mapped segment:offset. Resident entries (segment:offset) apply to the resident view and
// overlay entries (file offsets) to the overlay view; `returns` applies to both, since the mapped
// image keeps the resident addresses.
//
// In order: for each `jumps` entry of the view, each computed-jump reference from the site to an
// address the entry does not list is removed (DROPPED), and the bodies of the function holding
// the site and of each function that held such an address are recomputed. Each `decode` range of
// the view is decoded as the walk decoded it: its instructions are found from its start one after
// another, each code unit in the range that is not one of them is cleared (CONFLICT when a
// function held it), each function whose entry lies inside one of them is removed (FALSESTART),
// and each of them the listing lacks is disassembled. For each `jumps` entry, a computed-jump
// reference is added from the site to each target, and each target without an instruction is
// decoded as a one-instruction range. For each `returns` function, its no-return mark is removed,
// and every call to it whose flow override ends the flow at the call has the override removed and
// its fall-through disassembled. For each `fallThroughs` instruction, its fall-through is set to
// the byte after it and disassembled there. The body of each function that holds a site, a call so
// repaired, a conflict or a decode range's `owner` entry, or that held an address a removed
// reference named, is recomputed again, until no body changes. Last, each function whose entry
// holds no instruction is decoded again there, after the code units from its entry on that no
// other function holds are cleared. Run it on a copy of a snapshot, without -readOnly, after
// ApplyDeclaredEntries.java. It prints addresses and counts only.
// @category Restoration
import java.nio.file.*;
import java.util.*;
import com.google.gson.*;
import ghidra.app.cmd.function.CreateFunctionCmd;
import ghidra.app.script.GhidraScript;
import ghidra.app.util.PseudoDisassembler;
import ghidra.app.util.PseudoInstruction;
import ghidra.program.model.address.*;
import ghidra.program.model.listing.*;
import ghidra.program.model.symbol.*;

public class ApplyDeclaredFlows extends GhidraScript {
    private boolean overlay;
    private JsonArray map;
    private final Set<Address> fix = new TreeSet<>();
    private int falseStarts = 0;

    private Address view(String text) {
        boolean resident = text.contains(":");
        if (resident == overlay) return null;
        if (resident) return toAddr(text);
        long off = Long.decode(text);
        for (JsonElement m : map) {
            JsonObject o = m.getAsJsonObject();
            long code = Long.decode(o.get("CodeFileOffset").getAsString());
            if (off >= code && off <= code + o.get("CodeBytes").getAsLong()) return toAddr(o.get("MappedCodeStart").getAsString()).add(off - code);
        }
        throw new IllegalStateException("Overlay address outside every code range: " + text);
    }

    // Decodes the half-open range from low as consecutive instructions, the way the walk did: each
    // code unit in it (or running into it) that is not one of those instructions is cleared, and
    // the function that held it, if any, is printed as CONFLICT and has its body recomputed later;
    // a function whose entry lies inside one of the instructions is removed (FALSESTART). A unit
    // that is one of those instructions stays. Each of the instructions is then disassembled where
    // the listing has none. The range is extended to the end of its last instruction, so a single
    // target is decoded with its whole first instruction.
    private int decode(Address low, Address end) throws Exception {
        FunctionManager fm = currentProgram.getFunctionManager();
        Listing listing = currentProgram.getListing();
        PseudoDisassembler pd = new PseudoDisassembler(currentProgram);
        Map<Address, Integer> lengths = new HashMap<>();
        Address p = low, last = end;
        PseudoInstruction pi = null;
        while (p.compareTo(end) < 0) {
            try { pi = pd.disassemble(p); } catch (Exception x) { pi = null; }
            if (pi == null) { println("NOPSEUDO\t" + p); break; }
            lengths.put(p, pi.getLength());
            p = p.add(pi.getLength());
            if (p.compareTo(last) > 0) last = p;
        }
        int conflicts = 0;
        List<CodeUnit> clear = new ArrayList<>();
        CodeUnit before = listing.getCodeUnitContaining(low);
        if (before != null && before.getMinAddress().compareTo(low) < 0) clear.add(before);
        for (CodeUnit u : listing.getCodeUnits(new AddressSet(low, last.subtract(1)), true)) {
            Integer length = lengths.get(u.getMinAddress());
            if (u instanceof Instruction && length != null && length == u.getLength()) continue;
            clear.add(u);
        }
        for (CodeUnit u : clear) {
            if (u instanceof Data && !((Data) u).isDefined()) continue;
            Function f = fm.getFunctionContaining(u.getMinAddress());
            if (f != null) { conflicts++; fix.add(f.getEntryPoint()); println("CONFLICT\t" + u.getMinAddress() + "\t" + f.getEntryPoint()); }
            // A function whose entry lies inside one of the instructions starts at no instruction.
            for (Address x = u.getMinAddress(); x.compareTo(u.getMaxAddress()) <= 0; x = x.add(1)) {
                if (x.compareTo(low) < 0 || x.compareTo(last) >= 0 || lengths.containsKey(x)) continue;
                Function g = fm.getFunctionAt(x);
                if (g == null) continue;
                fm.removeFunction(x);
                fix.remove(x);
                falseStarts++;
                println("FALSESTART\t" + x);
            }
            clearListing(u.getMinAddress(), u.getMaxAddress());
        }
        List<Address> starts = new ArrayList<>(lengths.keySet());
        Collections.sort(starts);
        for (Address x : starts) if (getInstructionAt(x) == null) disassemble(x);
        if (getInstructionAt(low) == null) println("NOINSTRUCTION\t" + low);
        return conflicts;
    }

    protected void run() throws Exception {
        String[] args = getScriptArgs();
        if (args.length < 2 || !Set.of("resident", "overlay").contains(args[1])) throw new IllegalArgumentException("Supply the flows file and resident or overlay.");
        overlay = args[1].equals("overlay");
        if (overlay) map = JsonParser.parseString(Files.readString(Path.of(args[2]))).getAsJsonObject().getAsJsonArray("Overlays");
        JsonObject flows = JsonParser.parseString(Files.readString(Path.of(args[0]))).getAsJsonObject();
        FunctionManager fm = currentProgram.getFunctionManager();
        ReferenceManager rm = currentProgram.getReferenceManager();
        int decoded = 0, conflicts = 0, jumps = 0, refs = 0, repaired = 0, dropped = 0, fallThroughs = 0;
        // The computed-jump references a site's entry does not list go first, and the bodies that
        // held their targets are recomputed, so that code decoded from a wrong target no longer
        // counts as held when the decode ranges are cleared.
        Set<Address> early = new TreeSet<>();
        for (JsonElement e : flows.getAsJsonArray("jumps")) {
            JsonObject j = e.getAsJsonObject();
            Address site = view(j.get("site").getAsString());
            if (site == null) continue;
            Instruction i = getInstructionAt(site);
            if (i == null) continue;
            Set<Address> declared = new HashSet<>();
            for (JsonElement t : j.getAsJsonArray("targets")) declared.add(view(t.getAsString()));
            for (Reference r : i.getReferencesFrom()) {
                if (r.getReferenceType().isComputed() && r.getReferenceType().isJump() && !declared.contains(r.getToAddress())) {
                    Function h = fm.getFunctionContaining(r.getToAddress());
                    if (h != null && !h.getEntryPoint().equals(r.getToAddress())) early.add(h.getEntryPoint());
                    rm.delete(r);
                    dropped++;
                    println("DROPPED\t" + site + "\t" + r.getToAddress());
                }
            }
            Function f = fm.getFunctionContaining(site);
            if (f != null) early.add(f.getEntryPoint());
        }
        for (Address a : early) {
            Function f = fm.getFunctionAt(a);
            if (f != null) CreateFunctionCmd.fixupFunctionBody(currentProgram, f, monitor);
        }
        fix.addAll(early);
        for (JsonElement e : flows.getAsJsonArray("decode")) {
            JsonObject d = e.getAsJsonObject();
            String[] pair = d.get("range").getAsString().split("\\.\\.");
            Address low = view(pair[0]);
            if (low == null) continue;
            Address end = view(pair[1]);
            conflicts += decode(low, end);
            decoded++;
            Address owner = view(d.get("owner").getAsString());
            if (owner != null) fix.add(owner);
        }
        for (JsonElement e : flows.getAsJsonArray("jumps")) {
            JsonObject j = e.getAsJsonObject();
            Address site = view(j.get("site").getAsString());
            if (site == null) continue;
            Instruction i = getInstructionAt(site);
            if (i == null) { println("NOSITE\t" + site); continue; }
            jumps++;
            Set<Address> declared = new HashSet<>();
            for (JsonElement t : j.getAsJsonArray("targets")) declared.add(view(t.getAsString()));
            for (JsonElement t : j.getAsJsonArray("targets")) {
                Address target = view(t.getAsString());
                if (getInstructionAt(target) == null) conflicts += decode(target, target.add(1));
                rm.addMemoryReference(site, target, RefType.COMPUTED_JUMP, SourceType.USER_DEFINED, 0);
                refs++;
            }
            Function f = fm.getFunctionContaining(site);
            if (f != null) fix.add(f.getEntryPoint()); else println("NOFUNCTION\t" + site);
        }
        for (JsonElement e : flows.getAsJsonArray("returns")) {
            Address a = toAddr(e.getAsJsonObject().get("function").getAsString());
            Function f = fm.getFunctionAt(a);
            if (f == null) { println("NORETURNS\t" + a); continue; }
            f.setNoReturn(false);
            for (Reference r : rm.getReferencesTo(a)) {
                if (!r.getReferenceType().isCall()) continue;
                Instruction call = getInstructionAt(r.getFromAddress());
                if (call == null || call.getFlowOverride() != FlowOverride.CALL_RETURN) continue;
                call.setFlowOverride(FlowOverride.NONE);
                Address next = call.getMaxAddress().add(1);
                if (getInstructionAt(next) == null) disassemble(next);
                repaired++;
                println("REPAIRED\t" + call.getAddress() + "\t" + a);
                Function holder = fm.getFunctionContaining(call.getAddress());
                if (holder != null) fix.add(holder.getEntryPoint());
            }
        }
        for (JsonElement e : flows.getAsJsonArray("fallThroughs")) {
            Address a = view(e.getAsJsonObject().get("at").getAsString());
            if (a == null) continue;
            Instruction i = getInstructionAt(a);
            if (i == null) { println("NOFALLTHROUGH\t" + a); continue; }
            Address next = i.getMaxAddress().add(1);
            i.setFallThrough(next);
            if (getInstructionAt(next) == null) disassemble(next);
            fallThroughs++;
            Function f = fm.getFunctionContaining(a);
            if (f != null) fix.add(f.getEntryPoint());
        }
        int passes = 0;
        boolean changed = true;
        while (changed && passes < 8) {
            changed = false;
            passes++;
            for (Address a : fix) {
                Function f = fm.getFunctionAt(a);
                if (f == null) { println("NOOWNER\t" + a); continue; }
                long before = f.getBody().getNumAddresses();
                CreateFunctionCmd.fixupFunctionBody(currentProgram, f, monitor);
                if (f.getBody().getNumAddresses() != before) changed = true;
            }
        }
        // A function whose entry holds no instruction is decoded again at its entry once the bodies
        // above no longer hold the bytes there.
        int entries = 0;
        List<Function> undecoded = new ArrayList<>();
        for (Function f : fm.getFunctions(true)) if (getInstructionAt(f.getEntryPoint()) == null) undecoded.add(f);
        for (Function f : undecoded) {
            Address a = f.getEntryPoint();
            Function holder = null;
            CodeUnit u = currentProgram.getListing().getCodeUnitContaining(a);
            if (u != null) holder = fm.getFunctionContaining(u.getMinAddress());
            if (holder != null && !holder.equals(f)) { println("ENTRYHELD\t" + a + "\t" + holder.getEntryPoint()); continue; }
            // Clear the code units no function holds from the entry on: within the first 16 bytes
            // (the longest instruction) past undefined bytes, then up to the first undefined byte
            // or held unit. They are a decode that started inside the entry's bytes.
            Address p = u != null ? u.getMinAddress() : a;
            while (p.subtract(a) < 4096) {
                CodeUnit c = currentProgram.getListing().getCodeUnitContaining(p);
                if (c == null) break;
                if (c instanceof Data && !((Data) c).isDefined()) {
                    if (p.subtract(a) >= 16) break;
                    p = p.add(1);
                    continue;
                }
                Function h = fm.getFunctionContaining(c.getMinAddress());
                if (h != null && !h.equals(f)) break;
                clearListing(c.getMinAddress(), c.getMaxAddress());
                p = c.getMaxAddress().add(1);
            }
            disassemble(a);
            if (getInstructionAt(a) == null) { println("ENTRYFAILED\t" + a); continue; }
            CreateFunctionCmd.fixupFunctionBody(currentProgram, f, monitor);
            entries++;
            println("ENTRYDECODED\t" + a + "\t" + f.getBody().getNumAddresses());
        }
        for (Address a : fix) {
            Function f = fm.getFunctionAt(a);
            if (f != null) println("BODY\t" + a + "\t" + f.getBody().getNumAddresses());
        }
        println("FLOWS_COMPLETE\tview=" + args[1] + "\tdecoded=" + decoded + "\tconflicts=" + conflicts + "\tfalse_starts=" + falseStarts + "\tjumps=" + jumps + "\trefs=" + refs + "\trepaired=" + repaired + "\tdropped=" + dropped + "\tfallthroughs=" + fallThroughs + "\tentries=" + entries + "\tfunctions=" + fix.size() + "\tpasses=" + passes);
    }
}
