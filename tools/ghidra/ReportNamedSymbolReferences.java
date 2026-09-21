// Finds bounded program symbols by an explicit name fragment and reports their
// direct references without retaining source bytes or decompiler output.
// @category DarkSunWakeRedux

import ghidra.app.script.GhidraScript;
import ghidra.app.decompiler.DecompInterface;
import ghidra.app.decompiler.DecompileResults;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.pcode.HighSymbol;
import ghidra.program.model.symbol.Reference;
import ghidra.program.model.symbol.ReferenceIterator;
import ghidra.program.model.symbol.Symbol;
import ghidra.program.model.symbol.SymbolIterator;
import ghidra.program.model.listing.VariableStorage;

public class ReportNamedSymbolReferences extends GhidraScript {
    private static final int MAX_SYMBOLS = 32;
    private static final int MAX_REFERENCES_PER_SYMBOL = 64;

    @Override
    protected void run() throws Exception {
        String[] arguments = getScriptArgs();
        if ((arguments.length != 1 && arguments.length != 2) || arguments[0].isBlank()
            || arguments[0].length() > 128) {
            printerr("Supply a nonblank symbol-name fragment and optional function address.");
            return;
        }

        String fragment = arguments[0];
        int matches = 0;
        SymbolIterator symbols = currentProgram.getSymbolTable().getAllSymbols(true);
        while (symbols.hasNext() && !monitor.isCancelled()) {
            Symbol symbol = symbols.next();
            if (!symbol.getName().contains(fragment)) continue;
            println(symbol.getName() + " at " + symbol.getAddress() + " "
                + symbol.getSymbolType() + " " + symbol.getSource());
            reportReferences(symbol.getAddress());
            if (++matches >= MAX_SYMBOLS) {
                println("Output capped at " + MAX_SYMBOLS + " symbols.");
                return;
            }
        }
        if (matches != 0 || arguments.length == 1) {
            if (matches == 0) println("No symbol names contained the requested fragment.");
            return;
        }

        Address address = toAddr(arguments[1]);
        Function function = currentProgram.getFunctionManager().getFunctionContaining(address);
        if (function == null) {
            printerr(arguments[1] + ": no containing function");
            return;
        }
        DecompInterface decompiler = new DecompInterface();
        decompiler.openProgram(currentProgram);
        try {
            DecompileResults result = decompiler.decompileFunction(function, 60, monitor);
            if (!result.decompileCompleted()) {
                printerr("Decompiler failed: " + result.getErrorMessage());
                return;
            }
            var globals = result.getHighFunction().getGlobalSymbolMap().getSymbols();
            while (globals.hasNext() && !monitor.isCancelled()) {
                HighSymbol symbol = globals.next();
                if (!symbol.getName().contains(fragment)) continue;
                VariableStorage storage = symbol.getStorage();
                Address storageAddress = storage.getMinAddress();
                println("decompiler global " + symbol.getName() + " at " + storageAddress);
                reportReferences(storageAddress);
                matches++;
            }
        }
        finally {
            decompiler.dispose();
        }
        if (matches == 0) println("No program or decompiler-global symbol names contained the fragment.");
    }

    private void reportReferences(Address address) {
        ReferenceIterator references = currentProgram.getReferenceManager().getReferencesTo(address);
        int emitted = 0;
        while (references.hasNext() && emitted < MAX_REFERENCES_PER_SYMBOL) {
            Reference reference = references.next();
            Instruction instruction = currentProgram.getListing()
                .getInstructionAt(reference.getFromAddress());
            Function function = currentProgram.getFunctionManager()
                .getFunctionContaining(reference.getFromAddress());
            println("  " + reference.getFromAddress()
                + (function == null ? "" : " in " + function.getEntryPoint()
                    + " " + function.getName())
                + " " + reference.getReferenceType()
                + (instruction == null ? "" : ": " + instruction));
            emitted++;
        }
        if (references.hasNext())
            println("  references capped at " + MAX_REFERENCES_PER_SYMBOL);
    }
}
