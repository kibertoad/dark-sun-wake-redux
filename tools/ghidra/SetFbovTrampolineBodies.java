// Gives each FBOV trampoline of an MZ import a function of its own five bytes (FMT-EXE-004). The
// overlay manager returns from INT 3Fh to the trampoline's first byte, rewritten as a far jump, so no
// flow runs from the INT into the bytes after it (FND-EXE-593). For each overlay header the plan from
// fbov_overlay_plan.py names, the script removes every function that starts in, or whose body
// reaches, the header's bytes up to the paragraph after its last trampoline, clears that listing,
// disassembles each trampoline's INT 3Fh alone with no fall-through, defines its target word and
// unk_04 byte as data and creates a function whose body is the five bytes. A trampoline already in
// that state is left as it is. Run it on a copy of the resident snapshot, without -readOnly, before
// and after AddFbovOverlayBlocks.java. It prints addresses and counts only.
// @category Restoration
import java.nio.file.*;
import java.util.*;
import com.google.gson.*;
import ghidra.app.cmd.disassemble.DisassembleCommand;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.*;
import ghidra.program.model.data.*;
import ghidra.program.model.listing.*;
import ghidra.program.model.symbol.*;

public class SetFbovTrampolineBodies extends GhidraScript {
    private boolean ready(Address a) throws Exception {
        Function f = getFunctionAt(a);
        Instruction i = getInstructionAt(a);
        return f != null && f.getBody().equals(new AddressSet(a, a.add(4))) && i != null && i.getLength() == 2
            && i.getFallFrom() == null && i.getFallThrough() == null && getDataAt(a.add(2)) != null && getDataAt(a.add(4)) != null;
    }

    protected void run() throws Exception {
        JsonArray plan = JsonParser.parseString(Files.readString(Path.of(getScriptArgs()[0]))).getAsJsonArray();
        FunctionManager fm = currentProgram.getFunctionManager();
        Listing listing = currentProgram.getListing();
        long headers = 0, trampolines = 0, changedHeaders = 0, removed = 0;
        Set<Address> referenced = new TreeSet<>();
        for (JsonElement e : plan) {
            JsonObject o = e.getAsJsonObject();
            Address base = toAddr(o.get("header").getAsString() + ":0000");
            int count = o.get("trampolines").getAsInt();
            long length = (0x20 + 5L * count + 15) / 16 * 16;
            AddressSet header = new AddressSet(base, base.add(length - 1));
            headers++; trampolines += count;
            List<Address> stubs = new ArrayList<>();
            for (int i = 0; i < count; i++) stubs.add(base.add(0x20 + 5L * i));
            boolean clean = true;
            for (Address a : stubs) if (!ready(a)) clean = false;
            Iterator<Function> overlapping = fm.getFunctionsOverlapping(header);
            List<Function> drop = new ArrayList<>();
            while (overlapping.hasNext()) { Function f = overlapping.next(); if (!stubs.contains(f.getEntryPoint())) clean = false; drop.add(f); }
            if (clean) continue;
            changedHeaders++;
            for (Function f : drop) {
                AddressSet outside = new AddressSet(f.getBody()).subtract(header);
                for (Address from : f.getBody().getAddresses(true)) {
                    Instruction i = listing.getInstructionAt(from);
                    if (i == null || !header.contains(from)) continue;
                    for (Reference r : i.getReferencesFrom()) if (r.getReferenceType().isFlow() && !header.contains(r.getToAddress())) referenced.add(r.getToAddress());
                }
                println("REMOVE\t" + f.getEntryPoint() + "\t" + f.getBody().getNumAddresses() + "\toutside=" + outside.getNumAddresses()
                    + (outside.isEmpty() ? "" : "\t" + outside));
                fm.removeFunction(f.getEntryPoint());
                removed++;
            }
            clearListing(header);
            for (Address a : stubs) {
                byte[] trap = getBytes(a, 2);
                if (trap[0] != (byte) 0xCD || trap[1] != (byte) 0x3F) throw new IllegalStateException("No INT 3Fh at " + a);
                new DisassembleCommand(a, new AddressSet(a, a.add(1)), false).applyTo(currentProgram, monitor);
                Instruction i = getInstructionAt(a);
                if (i == null || i.getLength() != 2) throw new IllegalStateException("INT 3Fh not decoded at " + a);
                i.setFallThrough(null);
                createData(a.add(2), new WordDataType());
                createData(a.add(4), new ByteDataType());
                Function f = createFunction(a, null);
                if (f == null) throw new IllegalStateException("No function at " + a);
                f.setBody(new AddressSet(a, a.add(4)));
            }
        }
        // Flow targets of the removed decodes that lie outside the headers: an analyzer start that
        // only those decodes reached has no remaining flow reference.
        for (Address to : referenced) {
            boolean still = false;
            for (Reference r : getReferencesTo(to)) if (r.getReferenceType().isFlow()) still = true;
            Function f = getFunctionAt(to);
            println("TARGET\t" + to + "\t" + (f != null ? "function" : getInstructionAt(to) != null ? "instruction" : "none")
                + "\t" + (still ? "referenced" : "unreferenced"));
        }
        int ok = 0;
        for (JsonElement e : plan) {
            JsonObject o = e.getAsJsonObject();
            Address base = toAddr(o.get("header").getAsString() + ":0000");
            for (int i = 0; i < o.get("trampolines").getAsInt(); i++) if (ready(base.add(0x20 + 5L * i))) ok++;
        }
        if (ok != trampolines) throw new IllegalStateException("Trampolines not set: " + (trampolines - ok));
        println("TRAMPOLINES_COMPLETE\theaders=" + headers + "\ttrampolines=" + trampolines + "\tchanged_headers=" + changedHeaders
            + "\tfunctions_removed=" + removed);
    }
}
