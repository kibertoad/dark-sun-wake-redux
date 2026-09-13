// Reports bounded instruction uses of explicit positive structure displacements.
// @category DarkSunWakeRedux

import ghidra.app.script.GhidraScript;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.listing.InstructionIterator;

import java.util.HashSet;
import java.util.Locale;
import java.util.Set;

public class ReportStructureOffsets extends GhidraScript {
    private static final int MAX_MATCHES = 256;

    @Override
    protected void run() throws Exception {
        String[] arguments = getScriptArgs();
        if (arguments.length == 0) {
            printerr("Supply one or more decimal or 0x-prefixed structure offsets.");
            return;
        }

        Set<String> requested = new HashSet<>();
        for (String argument : arguments) {
            long offset = Long.decode(argument);
            if (offset < 1) {
                printerr("Offsets must be positive.");
                return;
            }
            requested.add(" + 0x" + Long.toHexString(offset).toLowerCase(Locale.ROOT) + "]");
        }

        int matches = 0;
        InstructionIterator instructions = currentProgram.getListing().getInstructions(true);
        while (instructions.hasNext() && !monitor.isCancelled()) {
            Instruction instruction = instructions.next();
            String rendered = instruction.toString().toLowerCase(Locale.ROOT);
            if (requested.stream().noneMatch(rendered::contains)) continue;
            Function function = currentProgram.getFunctionManager()
                .getFunctionContaining(instruction.getAddress());
            println(instruction.getAddress()
                + (function == null ? "" : " in " + function.getEntryPoint()
                    + " " + function.getName())
                + ": " + instruction);
            if (++matches >= MAX_MATCHES) {
                println("Output capped at " + MAX_MATCHES + " matches.");
                return;
            }
        }
        if (matches == 0) println("No requested structure offsets matched.");
    }
}
