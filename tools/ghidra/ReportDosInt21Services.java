// Reports explicit DOS INT 21h instructions with a nearby literal AH service setup.
// @category DarkSunWakeRedux

import ghidra.app.script.GhidraScript;
import ghidra.program.model.listing.Function;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.listing.InstructionIterator;
import ghidra.program.model.scalar.Scalar;

import java.util.HashSet;
import java.util.Locale;
import java.util.Set;

public class ReportDosInt21Services extends GhidraScript {
    private static final int MAX_SERVICES = 16;
    private static final int MAX_RESULTS = 128;
    private static final int LOOKBACK_INSTRUCTIONS = 12;

    @Override
    protected void run() throws Exception {
        Set<Long> requestedServices = parseServices();
        if (requestedServices == null) return;

        int matches = 0;
        InstructionIterator instructions = currentProgram.getListing().getInstructions(true);
        while (instructions.hasNext() && !monitor.isCancelled()) {
            Instruction interrupt = instructions.next();
            if (!interrupt.toString().toLowerCase(Locale.ROOT).startsWith("int 0x21"))
                continue;
            Function function = currentProgram.getFunctionManager()
                .getFunctionContaining(interrupt.getAddress());
            Instruction setup = findImmediateAhSetup(interrupt, function);
            if (setup == null) continue;
            long service = immediateScalar(setup);
            if (!requestedServices.isEmpty() && !requestedServices.contains(service))
                continue;
            println(interrupt.getAddress()
                + (function == null ? "" : " in " + function.getEntryPoint()
                    + " " + function.getName())
                + ": " + setup.getAddress() + " sets AH=" + format(service)
                + " within the prior " + LOOKBACK_INSTRUCTIONS + " instructions");
            if (++matches >= MAX_RESULTS) {
                println("Output capped at " + MAX_RESULTS + " results.");
                return;
            }
        }
        if (matches == 0) println("No explicit INT 21h instruction matched the requested nearby AH setup.");
    }

    private Set<Long> parseServices() {
        String[] arguments = getScriptArgs();
        if (arguments.length > MAX_SERVICES) {
            printerr("Supply no more than " + MAX_SERVICES + " decimal or 0x-prefixed AH values.");
            return null;
        }
        Set<Long> services = new HashSet<>();
        for (String argument : arguments) {
            long service = Long.decode(argument);
            if (service < 0 || service > 0xff) {
                printerr("AH service values must fit one byte.");
                return null;
            }
            services.add(service);
        }
        return services;
    }

    private Instruction findImmediateAhSetup(Instruction interrupt, Function function) {
        Instruction cursor = interrupt;
        for (int count = 0; count < LOOKBACK_INSTRUCTIONS; count++) {
            cursor = cursor.getPrevious();
            if (cursor == null || (function != null && !function.getBody().contains(cursor.getAddress())))
                break;
            if (cursor.toString().toLowerCase(Locale.ROOT).startsWith("mov ah,") &&
                immediateScalar(cursor) >= 0)
                return cursor;
        }
        return null;
    }

    private long immediateScalar(Instruction instruction) {
        for (int operand = 0; operand < instruction.getNumOperands(); operand++)
            for (Object object : instruction.getOpObjects(operand))
                if (object instanceof Scalar scalar) return scalar.getUnsignedValue();
        return -1;
    }

    private String format(long value) {
        return String.format("0x%02x", value);
    }
}
