// Reports bounded function entries that intersect one explicitly named memory block.
// @category DarkSunWakeRedux

import ghidra.app.script.GhidraScript;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.FunctionIterator;
import ghidra.program.model.mem.MemoryBlock;

public class ReportBlockFunctions extends GhidraScript {
    private static final int MAX_FUNCTIONS = 256;

    @Override
    protected void run() throws Exception {
        String[] arguments = getScriptArgs();
        if (arguments.length != 1) {
            printerr("Supply one exact memory-block name.");
            return;
        }
        MemoryBlock block = currentProgram.getMemory().getBlock(arguments[0]);
        if (block == null) {
            printerr("Unknown memory block '" + arguments[0] + "'.");
            return;
        }

        int count = 0;
        FunctionIterator functions = currentProgram.getFunctionManager().getFunctions(true);
        while (functions.hasNext() && !monitor.isCancelled()) {
            Function function = functions.next();
            if (!function.getBody().intersects(block.getStart(), block.getEnd())) continue;
            println(function.getEntryPoint() + " " + function.getName()
                + " body=" + function.getBody().getNumAddresses());
            if (++count >= MAX_FUNCTIONS) {
                println("Output capped at " + MAX_FUNCTIONS + " functions.");
                return;
            }
        }
        if (count == 0) println("No functions intersect the requested block.");
    }
}
