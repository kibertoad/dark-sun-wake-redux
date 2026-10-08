// Synthetic-only controls, never run against a licensed program.
// @category DarkSunWakeRedux
import ghidra.app.cmd.disassemble.DisassembleCommand;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.AddressSet;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.pcode.PcodeOp;
import java.util.Arrays;
import java.util.Set;

public class TestSegmentWrites extends GhidraScript {
    @Override
    protected void run() throws Exception {
        if (!currentProgram.getName().equals("segment-effects-synthetic.bin"))
            throw new IllegalArgumentException("Requires the named synthetic fixture");
        byte[] expected = { (byte)0x8c, (byte)0xd8, (byte)0x8e, (byte)0xd8,
            (byte)0x8e, (byte)0xd0, (byte)0x1f, (byte)0x0f, (byte)0xb2, 0,
            (byte)0xc5, 0, (byte)0xcc, (byte)0x90, (byte)0x9b, (byte)0xc3 };
        byte[] actual = new byte[expected.length];
        currentProgram.getMemory().getBytes(toAddr("1000"), actual);
        if (!Arrays.equals(actual, expected)) throw new IllegalArgumentException("Wrong synthetic bytes");
        if (!new DisassembleCommand(toAddr("1000"), new AddressSet(toAddr("1000"), toAddr("100f")), true)
                .applyTo(currentProgram, monitor)) throw new IllegalStateException("Synthetic decode failed");
        String[] names = { "DS", "SS" };
        String[] sites = { "1000", "1002", "1004", "1006", "1007", "100a", "100c", "100d", "100e", "100f" };
        String[] expectedWrites = { "", "DS", "SS", "DS", "SS", "DS", "", "", "", "" };
        for (int i = 0; i < sites.length; i++) {
            new DisassembleCommand(toAddr(sites[i]), null, false).applyTo(currentProgram, monitor);
            Instruction instruction = currentProgram.getListing().getInstructionAt(toAddr(sites[i]));
            if (instruction == null) throw new IllegalStateException("Missing synthetic instruction: " + sites[i]);
            Set<String> result = ReportSegmentWrites.writes(currentProgram, instruction, names);
            if (!String.join(",", result).equals(expectedWrites[i]))
                throw new IllegalStateException("Wrong segment effects at " + sites[i] + ": " + result);
        }
        boolean opaque = false;
        for (PcodeOp op : currentProgram.getListing().getInstructionAt(toAddr("100c")).getPcode())
            if (op.getOpcode() == PcodeOp.CALLOTHER) opaque = true;
        if (!opaque) throw new IllegalStateException("Opaque interrupt control was not recognized");
        if (currentProgram.getListing().getInstructionAt(toAddr("100d")).getPcode().length != 0)
            throw new IllegalStateException("NOP empty-effect control was not recognized");
        if (currentProgram.getListing().getInstructionAt(toAddr("100e")).getPcode().length != 0)
            throw new IllegalStateException("WAIT empty-effect control was not recognized");
        println("Synthetic segment-output controls passed.");
    }
}
