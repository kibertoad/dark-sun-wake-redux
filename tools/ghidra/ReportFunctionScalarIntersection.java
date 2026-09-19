// Reports bounded functions containing every explicitly supplied scalar.
// @category DarkSunWakeRedux

import ghidra.app.script.GhidraScript;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.FunctionIterator;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.listing.InstructionIterator;
import ghidra.program.model.scalar.Scalar;

import java.util.LinkedHashMap;
import java.util.LinkedHashSet;
import java.util.Map;
import java.util.Set;

public class ReportFunctionScalarIntersection extends GhidraScript {
    private static final int MAX_SCALARS = 16;
    private static final int MAX_FUNCTIONS = 64;

    @Override
    protected void run() throws Exception {
        String[] arguments = getScriptArgs();
        if (arguments.length == 0 || arguments.length > MAX_SCALARS) {
            printerr("Supply between 1 and " + MAX_SCALARS + " decimal or 0x-prefixed scalars.");
            return;
        }

        Set<Long> requested = new LinkedHashSet<>();
        for (String argument : arguments) requested.add(Long.decode(argument));
        int matches = 0;
        FunctionIterator functions = currentProgram.getFunctionManager().getFunctions(true);
        while (functions.hasNext() && !monitor.isCancelled()) {
            Function function = functions.next();
            Map<Long, Instruction> found = new LinkedHashMap<>();
            InstructionIterator instructions = currentProgram.getListing()
                .getInstructions(function.getBody(), true);
            while (instructions.hasNext()) {
                Instruction instruction = instructions.next();
                for (int operand = 0; operand < instruction.getNumOperands(); operand++) {
                    for (Object object : instruction.getOpObjects(operand)) {
                        if (object instanceof Scalar scalar &&
                            requested.contains(scalar.getUnsignedValue()))
                            found.putIfAbsent(scalar.getUnsignedValue(), instruction);
                    }
                }
            }
            if (!found.keySet().containsAll(requested)) continue;
            println(function.getEntryPoint() + " " + function.getName());
            for (long scalar : requested)
                println("  " + scalar + " at " + found.get(scalar).getAddress());
            if (++matches >= MAX_FUNCTIONS) {
                println("Output capped at " + MAX_FUNCTIONS + " functions.");
                return;
            }
        }
        if (matches == 0) println("No function contained every requested scalar.");
    }
}
