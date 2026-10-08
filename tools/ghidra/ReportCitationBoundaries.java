// Checks explicit half-open citation endpoints; does not infer interior coverage.
// @category DarkSunWakeRedux

import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.listing.Program;

public class ReportCitationBoundaries extends GhidraScript {
    private static final int MAX_QUERIES = 32;

    public static String inspect(Program program, Address start, Address end,
            boolean requireReturn) throws Exception {
        if (!start.getAddressSpace().equals(end.getAddressSpace())
                || end.compareTo(start) <= 0)
            throw new IllegalArgumentException("Range must be nonempty, increasing and in one address space");
        Address last = end.subtract(1);
        Instruction firstInstruction = program.getListing().getInstructionContaining(start);
        Instruction lastInstruction = program.getListing().getInstructionContaining(last);
        String startState = firstInstruction == null
            ? (program.getMemory().contains(start) ? "undecoded" : "unmapped")
            : (firstInstruction.getAddress().equals(start) ? "aligned" : "interior");
        String endState = lastInstruction == null
            ? (program.getMemory().contains(last) ? "undecoded" : "unmapped")
            : (lastInstruction.getMaxAddress().equals(last) ? "aligned" : "interior");
        String returnState = "not-requested";
        if (requireReturn) {
            returnState = "unknown";
            if (endState.equals("aligned")) {
                String mnemonic = lastInstruction.getMnemonicString();
                returnState = mnemonic.equalsIgnoreCase("RET") || mnemonic.equalsIgnoreCase("RETF")
                    ? "present" : "missing";
            }
        }
        return "start=" + start + " end=" + end + " startState=" + startState
            + " endState=" + endState + " returnState=" + returnState;
    }

    @Override
    protected void run() throws Exception {
        String[] args = getScriptArgs();
        if (args.length == 0 || args.length > MAX_QUERIES)
            throw new IllegalArgumentException("Supply 1..32 start..end[:return] queries");
        // Validate all queries before emitting results; malformed later input must not
        // leave a partial result that looks like a completed audit.
        String[] results = new String[args.length];
        for (int i = 0; i < args.length; i++) {
            String query = args[i];
            boolean requireReturn = query.endsWith(":return");
            if (requireReturn) query = query.substring(0, query.length() - 7);
            String[] endpoints = query.split("\\.\\.", -1);
            if (endpoints.length != 2 || endpoints[0].isBlank() || endpoints[1].isBlank())
                throw new IllegalArgumentException("Expected start..end[:return]");
            Address start = currentProgram.getAddressFactory().getAddress(endpoints[0]);
            Address end = currentProgram.getAddressFactory().getAddress(endpoints[1]);
            if (start == null || end == null)
                throw new IllegalArgumentException("Endpoint is not a representable program address");
            results[i] = inspect(currentProgram, start, end, requireReturn);
        }
        for (String result : results) println(result);
        println("Completed " + results.length + " endpoint queries; interior bytes, source identity,"
            + " reachability, callers, aliases and runtime effects are not validated.");
    }
}
