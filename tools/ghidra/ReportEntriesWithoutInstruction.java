// Lists function entries with no decoded instruction, the anomaly ExportResearchBaseline.java counts as
// entries_without_instruction. Prints addresses and body sizes only. Run with -readOnly.
// @category Restoration
import ghidra.app.script.GhidraScript;
import ghidra.program.model.listing.*;

public class ReportEntriesWithoutInstruction extends GhidraScript {
    protected void run() throws Exception {
        long count=0;
        FunctionIterator fs=currentProgram.getFunctionManager().getFunctions(true);
        while(fs.hasNext()) {
            monitor.checkCancelled(); Function f=fs.next(); if(f.isExternal()) continue;
            if(currentProgram.getListing().getInstructionAt(f.getEntryPoint())==null) {
                println("NO_INSTRUCTION\t"+f.getEntryPoint()+"\t"+f.getBody().getNumAddresses()); count++;
            }
        }
        println("NO_INSTRUCTION_COMPLETE\t"+count);
    }
}
