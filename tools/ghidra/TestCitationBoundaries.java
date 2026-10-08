// Synthetic-only integration controls; never run against an original program.
// @category DarkSunWakeRedux

import ghidra.app.cmd.disassemble.DisassembleCommand;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.address.AddressSet;

public class TestCitationBoundaries extends GhidraScript {
    private void check(String start, String end, boolean requireReturn,
            String expectedStart, String expectedEnd, String expectedReturn) throws Exception {
        String result = ReportCitationBoundaries.inspect(currentProgram, toAddr(start), toAddr(end), requireReturn);
        if (!result.contains("startState=" + expectedStart + " ")
                || !result.contains("endState=" + expectedEnd + " ")
                || !result.endsWith("returnState=" + expectedReturn))
            throw new IllegalStateException("Synthetic endpoint control failed: " + result);
    }

    @Override
    protected void run() throws Exception {
        if (!currentProgram.getName().equals("citation-boundaries-synthetic.bin"))
            throw new IllegalArgumentException("Requires the named synthetic fixture, never an original program");
        Address start = toAddr("1000");
        byte[] expected = { (byte) 0x90, 0x0f, (byte) 0x85, 0, 0, 0, 0, (byte) 0xc3 };
        byte[] actual = new byte[expected.length];
        currentProgram.getMemory().getBytes(start, actual);
        if (!java.util.Arrays.equals(expected, actual))
            throw new IllegalArgumentException("Synthetic fixture bytes do not match");
        DisassembleCommand command = new DisassembleCommand(start, new AddressSet(start, start.add(7)), true);
        if (!command.applyTo(currentProgram, monitor))
            throw new IllegalStateException("Could not decode synthetic controls");
        check("1000", "1008", true, "aligned", "aligned", "present");
        check("1000", "1007", false, "aligned", "aligned", "not-requested");
        check("1000", "1007", true, "aligned", "aligned", "missing");
        check("1000", "1006", true, "aligned", "interior", "unknown");
        check("1002", "1008", false, "interior", "aligned", "not-requested");
        check("1008", "100c", true, "undecoded", "undecoded", "unknown");
        check("2000", "2004", true, "unmapped", "unmapped", "unknown");
        try {
            ReportCitationBoundaries.inspect(currentProgram, start.add(1), start, false);
            throw new IllegalStateException("Reversed range was accepted");
        } catch (IllegalArgumentException expectedFailure) {
            // Required rejection, rather than an empty successful audit.
        }
        println("Synthetic citation endpoint controls passed.");
    }
}
