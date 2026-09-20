// Reports bounded DX-addressed port I/O following MOV DX, immediate setup.
// @category DarkSunWakeRedux

import ghidra.app.script.GhidraScript;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.listing.InstructionIterator;
import ghidra.program.model.scalar.Scalar;

import java.util.HashSet;
import java.util.Locale;
import java.util.Set;

public class ReportImmediatePortIo extends GhidraScript {
    private static final int MAX_PORTS = 16;
    private static final int MAX_MATCHES = 128;
    private static final int LOOKAHEAD_INSTRUCTIONS = 16;

    @Override
    protected void run() throws Exception {
        String[] arguments = getScriptArgs();
        if (arguments.length == 0 || arguments.length > MAX_PORTS) {
            printerr("Supply between 1 and " + MAX_PORTS + " port values.");
            return;
        }

        Set<Long> ports = new HashSet<>();
        for (String argument : arguments) {
            long port = Long.decode(argument);
            if (port < 0 || port > 0xffff) {
                printerr("Port values must fit an unsigned 16-bit value.");
                return;
            }
            ports.add(port);
        }

        int matches = 0;
        InstructionIterator instructions = currentProgram.getListing().getInstructions(true);
        while (instructions.hasNext() && !monitor.isCancelled()) {
            Instruction setup = instructions.next();
            if (!isDxImmediateSetup(setup, ports)) continue;
            Function function = currentProgram.getFunctionManager()
                .getFunctionContaining(setup.getAddress());
            Instruction cursor = setup;
            for (int offset = 0; offset < LOOKAHEAD_INSTRUCTIONS; offset++) {
                cursor = cursor.getNext();
                if (cursor == null || (function != null &&
                    !function.getBody().contains(cursor.getAddress()))) break;
                if (!isDxPortIo(cursor)) continue;
                println(setup.getAddress()
                    + (function == null ? "" : " in " + function.getEntryPoint()
                        + " " + function.getName())
                    + ": " + setup + " -> " + cursor.getAddress() + ": " + cursor);
                if (++matches >= MAX_MATCHES) {
                    println("Output capped at " + MAX_MATCHES + " matches.");
                    return;
                }
            }
        }
        if (matches == 0) println("No immediate-DX port I/O sequences matched.");
    }

    private static boolean isDxImmediateSetup(Instruction instruction, Set<Long> ports) {
        String text = instruction.toString().toLowerCase(Locale.ROOT);
        if (!text.startsWith("mov dx,")) return false;
        for (int operand = 0; operand < instruction.getNumOperands(); operand++)
            for (Object object : instruction.getOpObjects(operand))
                if (object instanceof Scalar scalar && ports.contains(scalar.getUnsignedValue()))
                    return true;
        return false;
    }

    private static boolean isDxPortIo(Instruction instruction) {
        String text = instruction.toString().toLowerCase(Locale.ROOT);
        return text.startsWith("in ") && text.contains(",dx") ||
            text.startsWith("out dx,");
    }
}
