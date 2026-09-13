// Prints a bounded byte range at one explicitly supplied virtual address.
// @category DarkSunWakeRedux

import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;

public class ReportDataBytes extends GhidraScript {
    private static final int MAX_BYTES = 256;

    @Override
    protected void run() throws Exception {
        String[] arguments = getScriptArgs();
        if (arguments.length != 2) {
            printerr("Supply a virtual address and a byte count.");
            return;
        }

        Address address = toAddr(arguments[0]);
        int count = Integer.decode(arguments[1]);
        if (count < 1 || count > MAX_BYTES) {
            printerr("Byte count must be between 1 and " + MAX_BYTES + ".");
            return;
        }

        byte[] bytes = new byte[count];
        currentProgram.getMemory().getBytes(address, bytes);
        println("===== " + address + " (" + count + " bytes) =====");
        for (int offset = 0; offset < bytes.length; offset += 16) {
            StringBuilder line = new StringBuilder(address.add(offset) + ":");
            int end = Math.min(offset + 16, bytes.length);
            for (int index = offset; index < end; index++)
                line.append(String.format(" %02x", bytes[index] & 0xff));
            println(line.toString());
        }
    }
}
