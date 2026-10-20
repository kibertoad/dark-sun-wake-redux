// Corrects the targets Ghidra 12.1.3 gives near relative transfers in 16-bit segmented code whose
// segment spans a linear 64K boundary. A near call, jump, conditional jump, loop or jcxz adds its
// signed displacement to IP and keeps CS, so its target is CS:((IP after it + displacement) &
// FFFF). CS is the segment whose FBOV segment-table span holds the instruction (FMT-EXE-002,
// FND-EXE-571: the descriptor's segment word as loaded, which carries an MZ relocation, from its
// start offset to its end offset), or, outside every span (overlay code), the segment its memory
// block starts at. Ghidra wraps the sum at the 64K linear page of the instruction instead, which
// moves the target 64K up or down wherever the instruction and its target lie on two sides of a
// linear 0x?0000 boundary inside one segment.
//
// Arguments: report or apply, the segment table's load address (segment:offset), its number of
// descriptors and, for the overlay view, the ReportFbovOverlayMap.ps1 mapping and the shipped
// DSUN.EXE, whose trampolines (FMT-EXE-004) name routine entries to keep. Both modes print one
// WRONG line per transfer whose flows lack the CS target, with Ghidra's flows and the CS target,
// and one OUTSIDE line per computed-jump reference from a near indirect jump (FF /4) to an address
// outside CS:0000..CS:FFFF. Then they print FALSEFUNC for each function at a wrong target, with the
// number of references to it from other instructions, and NEWCALLEE for each CS call target with
// no function.
//
// apply also: removes the wrong flow references and the OUTSIDE references; adds the CS target with
// the transfer's reference type; clears code units no function body holds where the target has no
// instruction and disassembles it; removes a call's end-of-flow override when the CS callee is not
// marked no-return and disassembles its fall-through; creates a function at each CS call target
// that has an instruction and no function; removes each function at a wrong target that no other
// reference names and that is no trampoline target (printed as KEPT otherwise), clears the code
// units of its body that no other function holds, and disassembles again each cleared address, or
// address just before cleared bytes, that a remaining instruction flows or falls into; and
// recomputes the body of each function that holds a corrected transfer, an OUTSIDE site or such a
// flow, or that ends where a wrong target began. Last, it disassembles each fall-through or flow
// target of an instruction in a function body that lies on undefined bytes and recomputes that
// body, until none is left. Run apply on a copy of a snapshot, without -readOnly; run it again
// after ApplyDeclaredFlows.java, whose decoding gives the transfers it decodes Ghidra's targets
// again. It prints addresses and counts only.
// @category Restoration
import java.nio.file.*;
import java.util.*;
import com.google.gson.*;
import ghidra.app.cmd.function.CreateFunctionCmd;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.*;
import ghidra.program.model.listing.*;
import ghidra.program.model.mem.*;
import ghidra.program.model.symbol.*;

public class CorrectNearTransfers extends GhidraScript {
    private static final Set<Integer> PREFIXES = Set.of(0x26, 0x2E, 0x36, 0x3E, 0x64, 0x65, 0x66, 0x67, 0xF0, 0xF2, 0xF3);
    private Address table;
    private final List<long[]> spans = new ArrayList<>();

    private int cs(SegmentedAddress at) {
        int cs = -1;
        if (at.getAddressSpace().equals(table.getAddressSpace())) {
            for (long[] span : spans) if (at.getOffset() >= span[1] && at.getOffset() < span[2]) cs = (int) span[0];
        }
        if (cs < 0) cs = ((SegmentedAddress) currentProgram.getMemory().getBlock(at).getStart()).getSegment();
        return cs;
    }

    protected void run() throws Exception {
        String[] args = getScriptArgs();
        if (args.length != 3 && args.length != 5 || !Set.of("report", "apply").contains(args[0])) throw new IllegalArgumentException("Supply report or apply, the segment table's address, its descriptor count and, for the overlay view, the mapping and the shipped file.");
        boolean apply = args[0].equals("apply");
        FunctionManager fm = currentProgram.getFunctionManager();
        ReferenceManager rm = currentProgram.getReferenceManager();
        Memory mem = currentProgram.getMemory();
        table = toAddr(args[1]);
        int count = Integer.parseInt(args[2]);
        // In the overlay view, each trampoline's target (FMT-EXE-004: CD 3F at 0x20 + 5 * k of the
        // header in the shipped file, then the target's offset in the overlay's code) is a routine
        // entry whatever references it. The mapped image does not keep the headers.
        Set<Address> entries = new HashSet<>();
        if (args.length == 5) {
            byte[] shipped = Files.readAllBytes(Path.of(args[4]));
            for (JsonElement m : JsonParser.parseString(Files.readString(Path.of(args[3]))).getAsJsonObject().getAsJsonArray("Overlays")) {
                JsonObject o = m.getAsJsonObject();
                int header = Integer.decode(o.get("HeaderFileOffset").getAsString()) + 0x20;
                Address code = toAddr(o.get("MappedCodeStart").getAsString());
                for (int k = 0; k < o.get("TrampolineCount").getAsInt(); k++) {
                    int t = header + 5 * k;
                    if ((shipped[t] & 0xFF) != 0xCD || (shipped[t + 1] & 0xFF) != 0x3F) throw new IllegalStateException("No INT 3Fh at file offset " + Integer.toHexString(t));
                    entries.add(code.add((shipped[t + 2] & 0xFF) | (shipped[t + 3] & 0xFF) << 8));
                }
            }
            println("TRAMPOLINE_TARGETS\t" + entries.size());
        }
        for (int d = 0; d < count; d++) {
            Address row = table.add(8L * d);
            int segment = mem.getShort(row) & 0xFFFF, end = mem.getShort(row.add(2)) & 0xFFFF, begin = mem.getShort(row.add(6)) & 0xFFFF;
            if (end == 0 || end == 0xFFFF || begin >= end) continue;
            long base = segment * 16L;
            spans.add(new long[] {segment, base + begin, base + end});
        }
        println("SPANS\t" + spans.size());
        int scanned = 0, wrong = 0;
        List<Object[]> fixes = new ArrayList<>();
        Map<Address, Integer> wrongTargets = new TreeMap<>();
        for (Instruction i : currentProgram.getListing().getInstructions(true)) {
            byte[] b = i.getBytes();
            int k = 0;
            while (k < b.length && PREFIXES.contains(b[k] & 0xFF)) k++;
            if (k >= b.length) continue;
            int op = b[k] & 0xFF, rest = b.length - k;
            Integer rel = null;
            RefType type = null;
            if ((op == 0xE8 || op == 0xE9) && rest == 3) { rel = (int) (short) ((b[k + 1] & 0xFF) | (b[k + 2] << 8)); type = op == 0xE8 ? RefType.UNCONDITIONAL_CALL : RefType.UNCONDITIONAL_JUMP; }
            else if ((op == 0xEB || (op >= 0x70 && op <= 0x7F) || (op >= 0xE0 && op <= 0xE3)) && rest == 2) { rel = (int) b[k + 1]; type = op == 0xEB ? RefType.UNCONDITIONAL_JUMP : RefType.CONDITIONAL_JUMP; }
            else if (op == 0x0F && rest == 4 && (b[k + 1] & 0xFF) >= 0x80 && (b[k + 1] & 0xFF) <= 0x8F) { rel = (int) (short) ((b[k + 2] & 0xFF) | (b[k + 3] << 8)); type = RefType.CONDITIONAL_JUMP; }
            if (rel == null) continue;
            if (!(i.getAddress() instanceof SegmentedAddress)) continue;
            scanned++;
            SegmentedAddress at = (SegmentedAddress) i.getAddress();
            int cs = cs(at);
            long ipNext = at.getOffset() + i.getLength() - (long) cs * 16;
            long ip = (ipNext + rel) & 0xFFFF;
            Address right = ((SegmentedAddressSpace) at.getAddressSpace()).getAddress(cs, (int) ip);
            Address[] flows = i.getFlows();
            boolean ok = false;
            for (Address f : flows) if (f.equals(right)) ok = true;
            if (ok) continue;
            wrong++;
            StringBuilder s = new StringBuilder("WRONG\t" + at + "\t" + i.getMnemonicString() + "\tcs=" + right + "\tghidra=");
            for (Address f : flows) { s.append(f).append(' '); wrongTargets.merge(f, 1, Integer::sum); }
            println(s.toString().trim());
            fixes.add(new Object[] {i, right, type, flows});
        }
        // A near indirect jump (FF /4) keeps CS, so a computed-jump reference from it to an address
        // outside CS:0000..CS:FFFF is wrong.
        Set<Address> wrongSites = new HashSet<>();
        for (Object[] x : fixes) wrongSites.add(((Instruction) x[0]).getAddress());
        List<Reference> outside = new ArrayList<>();
        for (Instruction i : currentProgram.getListing().getInstructions(true)) {
            if (!(i.getAddress() instanceof SegmentedAddress)) continue;
            byte[] b = i.getBytes();
            int k = 0;
            while (k < b.length && PREFIXES.contains(b[k] & 0xFF)) k++;
            if (k + 1 >= b.length || (b[k] & 0xFF) != 0xFF || ((b[k + 1] >> 3) & 7) != 4) continue;
            long base = (long) cs((SegmentedAddress) i.getAddress()) * 16;
            for (Reference r : i.getReferencesFrom()) {
                if (!r.getReferenceType().isComputed() || !r.getReferenceType().isJump()) continue;
                Address to = r.getToAddress();
                if (to.getAddressSpace().equals(i.getAddress().getAddressSpace()) && to.getOffset() >= base && to.getOffset() < base + 0x10000) continue;
                outside.add(r);
                wrongSites.add(i.getAddress());
                wrongTargets.merge(to, 1, Integer::sum);
                println("OUTSIDE\t" + i.getAddress() + "\t" + to);
            }
        }
        for (Map.Entry<Address, Integer> e : wrongTargets.entrySet()) {
            Function f = fm.getFunctionAt(e.getKey());
            if (f == null) continue;
            int others = 0;
            for (Reference r : rm.getReferencesTo(e.getKey())) if (!wrongSites.contains(r.getFromAddress())) others++;
            println("FALSEFUNC\t" + e.getKey() + "\twrong=" + e.getValue() + "\tother_refs=" + others + "\tbody=" + f.getBody().getNumAddresses());
        }
        Set<Address> newCallees = new TreeSet<>();
        for (Object[] x : fixes) if (((RefType) x[2]).isCall() && fm.getFunctionAt((Address) x[1]) == null) newCallees.add((Address) x[1]);
        for (Address a : newCallees) println("NEWCALLEE\t" + a + "\tinstruction=" + (getInstructionAt(a) != null));
        int created = 0, cleared = 0, held = 0, removedFalse = 0, redecoded = 0, filled = 0;
        Set<Address> fix = new TreeSet<>();
        if (apply) {
            for (Object[] x : fixes) {
                Instruction i = (Instruction) x[0];
                Address right = (Address) x[1];
                RefType type = (RefType) x[2];
                for (Reference r : i.getReferencesFrom()) if (r.getReferenceType().isFlow()) rm.delete(r);
                rm.addMemoryReference(i.getAddress(), right, type, SourceType.USER_DEFINED, 0);
                if (getInstructionAt(right) == null) {
                    CodeUnit u = currentProgram.getListing().getCodeUnitContaining(right);
                    if (u != null && !(u instanceof Data && !((Data) u).isDefined())) {
                        if (fm.getFunctionContaining(u.getMinAddress()) != null) { held++; println("HELD\t" + right + "\t" + u.getMinAddress()); }
                        else { clearListing(u.getMinAddress(), u.getMaxAddress()); cleared++; }
                    }
                    disassemble(right);
                }
                if (type.isCall() && i.getFlowOverride() == FlowOverride.CALL_RETURN) {
                    Function callee = fm.getFunctionAt(right);
                    if (callee == null || !callee.hasNoReturn()) {
                        i.setFlowOverride(FlowOverride.NONE);
                        Address next = i.getMaxAddress().add(1);
                        if (getInstructionAt(next) == null) disassemble(next);
                    }
                }
                Function holder = fm.getFunctionContaining(i.getAddress());
                if (holder != null) fix.add(holder.getEntryPoint());
            }
            // A body that ends where a wrong target began (Ghidra stops a body at a call target) is
            // recomputed too.
            for (Address t : wrongTargets.keySet()) {
                Instruction before;
                try { before = getInstructionContaining(t.subtract(1)); } catch (AddressOutOfBoundsException x) { continue; }
                Function holder = before != null ? fm.getFunctionContaining(before.getAddress()) : null;
                if (holder != null) fix.add(holder.getEntryPoint());
            }
            for (Reference r : outside) {
                rm.delete(r);
                Function holder = fm.getFunctionContaining(r.getFromAddress());
                if (holder != null) fix.add(holder.getEntryPoint());
            }
            for (Address a : newCallees) {
                if (getInstructionAt(a) == null || fm.getFunctionAt(a) != null) continue;
                Function holder = fm.getFunctionContaining(a);
                if (holder != null) holder.setBody(new AddressSet(holder.getEntryPoint(), holder.getEntryPoint()));
                Function f = createFunction(a, null);
                if (holder != null) { CreateFunctionCmd.fixupFunctionBody(currentProgram, holder, monitor); fix.add(holder.getEntryPoint()); }
                if (f != null) { created++; println("CREATED\t" + a + "\t" + f.getBody().getNumAddresses()); }
            }
            // A function at a wrong target that nothing else references was made from the wrong
            // transfer alone: remove it, clear the code units of its body that no other function
            // holds, and decode again only what flows from the remaining code reach.
            AddressSet cleared2 = new AddressSet();
            for (Map.Entry<Address, Integer> e : wrongTargets.entrySet()) {
                Function f = fm.getFunctionAt(e.getKey());
                if (f == null) continue;
                boolean others = false;
                for (Reference r : rm.getReferencesTo(e.getKey())) if (!wrongSites.contains(r.getFromAddress())) others = true;
                if (others) continue;
                if (entries.contains(e.getKey())) { println("KEPT\t" + e.getKey()); continue; }
                AddressSetView body = f.getBody();
                fm.removeFunction(e.getKey());
                removedFalse++;
                println("REMOVED\t" + e.getKey() + "\t" + body.getNumAddresses());
                for (AddressRange r : body) {
                    for (CodeUnit u : currentProgram.getListing().getCodeUnits(new AddressSet(r), true)) {
                        if (fm.getFunctionContaining(u.getMinAddress()) != null) continue;
                        clearListing(u.getMinAddress(), u.getMaxAddress());
                        cleared2.add(u.getMinAddress(), u.getMaxAddress());
                    }
                }
            }
            // One pass suffices: the disassembler follows flows inside the cleared bytes from each
            // address it starts at, and a target it could not decode is not tried again.
            Set<Address> tried = new HashSet<>();
            if (!cleared2.isEmpty()) {
                List<Address[]> into = new ArrayList<>();
                for (Instruction i : currentProgram.getListing().getInstructions(true)) {
                    List<Address> targets = new ArrayList<>(Arrays.asList(i.getFlows()));
                    if (i.getFallThrough() != null) targets.add(i.getFallThrough());
                    // A target just before the cleared bytes failed to decode while they held code.
                    for (Address t : targets) if (cleared2.contains(t) || getInstructionAt(t) == null && cleared2.intersects(t, t.add(15))) into.add(new Address[] {i.getAddress(), t});
                }
                for (Address[] x : into) {
                    if (getInstructionAt(x[1]) != null || !tried.add(x[1])) continue;
                    disassemble(x[1]);
                    redecoded++;
                    Function holder = fm.getFunctionContaining(x[0]);
                    if (holder != null) fix.add(holder.getEntryPoint());
                }
            }
            for (int pass = 0; pass < 3; pass++) for (Address a : fix) {
                Function f = fm.getFunctionAt(a);
                if (f != null) CreateFunctionCmd.fixupFunctionBody(currentProgram, f, monitor);
            }
            // Then each fall-through or flow target of an instruction in a function body that lies
            // on undefined bytes is disassembled and the body recomputed, until none is left.
            for (int pass = 0; pass < 8; pass++) {
                Map<Address, Address> open = new TreeMap<>();
                for (Instruction i : currentProgram.getListing().getInstructions(true)) {
                    Function holder = fm.getFunctionContaining(i.getAddress());
                    if (holder == null) continue;
                    List<Address> targets = new ArrayList<>(Arrays.asList(i.getFlows()));
                    if (i.getFallThrough() != null) targets.add(i.getFallThrough());
                    for (Address t : targets) {
                        if (!mem.contains(t) || getInstructionAt(t) != null || tried.contains(t)) continue;
                        CodeUnit u = currentProgram.getListing().getCodeUnitAt(t);
                        if (u instanceof Data && !((Data) u).isDefined()) open.put(t, holder.getEntryPoint());
                    }
                }
                if (open.isEmpty()) break;
                Set<Address> holders = new TreeSet<>();
                for (Map.Entry<Address, Address> e : open.entrySet()) {
                    tried.add(e.getKey());
                    if (getInstructionAt(e.getKey()) == null) disassemble(e.getKey());
                    if (getInstructionAt(e.getKey()) != null) { filled++; holders.add(e.getValue()); }
                }
                for (Address a : holders) {
                    Function f = fm.getFunctionAt(a);
                    if (f != null) CreateFunctionCmd.fixupFunctionBody(currentProgram, f, monitor);
                }
            }
        }
        println("NEAR_COMPLETE\tmode=" + args[0] + "\tscanned=" + scanned + "\twrong=" + wrong + "\toutside=" + outside.size() + "\tfalse_candidates=" + wrongTargets.size() + "\tnew_callees=" + newCallees.size() + "\tcreated=" + created + "\tcleared=" + cleared + "\theld=" + held + "\tremoved=" + removedFalse + "\tredecoded=" + redecoded + "\tfilled=" + filled);
    }
}
