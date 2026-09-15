// Reports bounded instructions whose rendered operands contain explicit tokens.
// @category DarkSunWakeRedux

import ghidra.app.script.GhidraScript;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.listing.InstructionIterator;

import java.util.Arrays;
import java.util.Locale;

public class ReportInstructionText extends GhidraScript {
    private static final int MAX_TOKENS = 16;
    private static final int MAX_TOKEN_LENGTH = 64;
    private static final int MAX_MATCHES = 256;

    @Override
    protected void run() throws Exception {
        String[] arguments = getScriptArgs();
        if (arguments.length == 0 || arguments.length > MAX_TOKENS) {
            printerr("Supply between 1 and " + MAX_TOKENS + " instruction-text tokens.");
            return;
        }
        String[] tokens = Arrays.stream(arguments)
            .map(value -> value.toLowerCase(Locale.ROOT))
            .toArray(String[]::new);
        if (Arrays.stream(tokens).anyMatch(value -> value.isBlank()
            || value.length() > MAX_TOKEN_LENGTH)) {
            printerr("Tokens must be nonblank and at most " + MAX_TOKEN_LENGTH + " characters.");
            return;
        }

        int matches = 0;
        InstructionIterator instructions = currentProgram.getListing().getInstructions(true);
        while (instructions.hasNext() && !monitor.isCancelled()) {
            Instruction instruction = instructions.next();
            String rendered = instruction.toString().toLowerCase(Locale.ROOT);
            if (Arrays.stream(tokens).noneMatch(rendered::contains)) continue;
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
        if (matches == 0) println("No requested instruction text matched.");
    }
}
