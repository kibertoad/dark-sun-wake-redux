// Reports bounded occurrences of one explicit hexadecimal byte pattern.
// @category DarkSunWakeRedux

import ghidra.app.script.GhidraScript;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.mem.Memory;
import ghidra.program.model.mem.MemoryBlock;
import ghidra.program.model.symbol.Reference;
import ghidra.program.model.symbol.ReferenceIterator;

public class ReportBytePattern extends GhidraScript {
    private static final int MAX_MATCHES = 100;
    private static final int MAX_REFERENCES_PER_MATCH = 20;
    private static final int MAX_PATTERN_BYTES = 64;

    @Override
    protected void run() throws Exception {
        String[] arguments = getScriptArgs();
        if (arguments.length != 1) {
            printerr("Supply one even-length hexadecimal byte pattern.");
            return;
        }

        byte[] pattern = parsePattern(arguments[0]);
        Memory memory = currentProgram.getMemory();
        int matches = 0;
        for (MemoryBlock block : memory.getBlocks()) {
            var cursor = block.getStart();
            while (cursor.compareTo(block.getEnd()) <= 0 &&
                   matches < MAX_MATCHES && !monitor.isCancelled()) {
                var found = memory.findBytes(cursor, pattern, null, true, monitor);
                if (found == null || !block.contains(found)) break;
                reportMatch(block, found);
                matches++;
                cursor = found.add(1);
            }
            if (matches >= MAX_MATCHES) break;
        }

        if (matches == 0) println("No byte-pattern matches found.");
        else if (matches == MAX_MATCHES)
            println("Output capped at " + MAX_MATCHES + " matches.");
    }

    private void reportMatch(MemoryBlock block, ghidra.program.model.address.Address found) {
        Instruction instruction = currentProgram.getListing().getInstructionContaining(found);
        Function function = currentProgram.getFunctionManager().getFunctionContaining(found);
        println(found + " in " + block.getName()
            + (function == null ? "" : " in " + function.getEntryPoint() + " " + function.getName())
            + (instruction == null ? "" : ": " + instruction));

        ReferenceIterator references = currentProgram.getReferenceManager().getReferencesTo(found);
        int emitted = 0;
        while (references.hasNext() && emitted < MAX_REFERENCES_PER_MATCH) {
            Reference reference = references.next();
            println("  referenced from " + reference.getFromAddress() + " " + reference.getReferenceType());
            emitted++;
        }
        if (references.hasNext())
            println("  references capped at " + MAX_REFERENCES_PER_MATCH);
    }

    private byte[] parsePattern(String text) {
        String compact = text.replace(" ", "").replace("_", "");
        if (compact.length() == 0 || compact.length() > MAX_PATTERN_BYTES * 2 ||
            (compact.length() & 1) != 0 ||
            !compact.matches("[0-9a-fA-F]+"))
            throw new IllegalArgumentException(
                "Pattern must contain between 1 and " + MAX_PATTERN_BYTES + " complete hexadecimal bytes.");
        byte[] pattern = new byte[compact.length() / 2];
        for (int index = 0; index < pattern.length; index++)
            pattern[index] = (byte) Integer.parseInt(compact.substring(index * 2, index * 2 + 2), 16);
        return pattern;
    }
}
