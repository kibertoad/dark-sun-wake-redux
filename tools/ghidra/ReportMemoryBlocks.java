// Reports the program's bounded memory-block map.
// @category DarkSunWakeRedux

import ghidra.app.script.GhidraScript;
import ghidra.program.model.mem.MemoryBlock;

public class ReportMemoryBlocks extends GhidraScript {
    private static final int MAX_BLOCKS = 512;

    @Override
    protected void run() throws Exception {
        if (getScriptArgs().length != 0) {
            printerr("This report takes no arguments.");
            return;
        }
        MemoryBlock[] blocks = currentProgram.getMemory().getBlocks();
        if (blocks.length > MAX_BLOCKS) {
            printerr("Program has " + blocks.length + " blocks; maximum is " + MAX_BLOCKS + ".");
            return;
        }
        for (MemoryBlock block : blocks)
            println(block.getName() + " " + block.getStart() + "-" + block.getEnd()
                + " size=" + block.getSize());
    }
}
