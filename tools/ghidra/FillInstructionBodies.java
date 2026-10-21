// Adds to each function body the bytes of its own instructions that the body leaves out.
// Ghidra 12.1.3 leaves a call's target out of a body even where the target lies inside an
// instruction of that body: a near call into the second byte of an instruction (the one-byte iret
// routines of FND-EXE-594), or a reference Ghidra made from a computed call or from bytes it decoded
// outside every function. The instruction's other bytes stay in the body, so the body holds part of
// an instruction (FND-EXE-601). For each function and each instruction that starts in its body,
// every byte of the instruction the body lacks is added, unless another function's body holds it
// (printed as HELD). Each function changed is printed as FILLED with the bytes it gained. Run it
// on a copy of a snapshot, without -readOnly, as the last step that changes bodies before
// ClipBodiesToEntryRegion.java or the export. It prints addresses and counts only.
// @category Restoration
import java.util.*;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.*;
import ghidra.program.model.listing.*;

public class FillInstructionBodies extends GhidraScript {
    protected void run() throws Exception {
        FunctionManager fm = currentProgram.getFunctionManager();
        Listing listing = currentProgram.getListing();
        List<Function> functions = new ArrayList<>();
        for (Function f : fm.getFunctions(true)) if (!f.isExternal()) functions.add(f);
        int filled = 0, held = 0;
        long bytes = 0;
        for (Function f : functions) {
            monitor.checkCancelled();
            AddressSetView body = f.getBody();
            AddressSet missing = new AddressSet();
            for (Instruction i : listing.getInstructions(body, true)) {
                if (!body.contains(i.getAddress())) continue;
                AddressSet whole = new AddressSet(i.getMinAddress(), i.getMaxAddress());
                missing.add(whole.subtract(body));
            }
            if (missing.isEmpty()) continue;
            AddressSet add = new AddressSet();
            for (AddressRange r : missing) {
                for (Address a = r.getMinAddress(); a.compareTo(r.getMaxAddress()) <= 0; a = a.add(1)) {
                    Function other = fm.getFunctionContaining(a);
                    if (other != null && !other.equals(f)) { held++; println("HELD\t" + f.getEntryPoint() + "\t" + a + "\t" + other.getEntryPoint()); continue; }
                    add.add(a);
                }
            }
            if (add.isEmpty()) continue;
            f.setBody(new AddressSet(body).union(add));
            filled++;
            bytes += add.getNumAddresses();
            StringBuilder line = new StringBuilder("FILLED\t").append(f.getEntryPoint()).append('\t').append(add.getNumAddresses()).append('\t');
            boolean first = true;
            for (AddressRange r : add) {
                if (!first) line.append(' ');
                first = false;
                line.append(r.getMinAddress()).append("..").append(r.getMaxAddress().add(1));
            }
            println(line.toString());
        }
        println("FILL_COMPLETE\tfunctions_examined=" + functions.size() + "\tfunctions_filled=" + filled + "\tbytes_added=" + bytes + "\theld=" + held);
    }
}
