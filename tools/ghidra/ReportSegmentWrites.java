// Reports named segment-register outputs in decoded p-code, never rendered operands.
// @category DarkSunWakeRedux

import ghidra.app.script.GhidraScript;
import ghidra.program.model.lang.Register;
import ghidra.program.model.listing.Instruction;
import ghidra.program.model.listing.InstructionIterator;
import ghidra.program.model.listing.Program;
import ghidra.program.model.pcode.PcodeOp;
import ghidra.program.model.pcode.Varnode;
import java.util.ArrayList;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.Map;
import java.util.TreeMap;

public class ReportSegmentWrites extends GhidraScript {
    public static Set<String> writes(Program program, Instruction instruction, String[] names) {
        Set<String> result = new LinkedHashSet<>();
        for (String name : names) {
            Register register = program.getRegister(name);
            if (register == null) throw new IllegalArgumentException("Unknown segment register: " + name);
            long start = register.getAddress().getOffset();
            long end = start + register.getMinimumByteSize();
            for (PcodeOp op : instruction.getPcode()) {
                Varnode output = op.getOutput();
                if (output != null && output.getAddress().getAddressSpace().equals(
                        register.getAddress().getAddressSpace())
                        && output.getOffset() < end && output.getOffset() + output.getSize() > start)
                    result.add(name);
            }
        }
        return result;
    }

    @Override
    protected void run() throws Exception {
        String[] args = getScriptArgs();
        if (args.length != 2)
            throw new IllegalArgumentException("Supply segment names and output limit (1..10000)");
        if (!currentProgram.getLanguage().getProcessor().toString().equalsIgnoreCase("x86"))
            throw new IllegalArgumentException("Requires an x86 program");
        String[] names = args[0].toUpperCase(Locale.ROOT).split("\\+", -1);
        Set<String> unique = new LinkedHashSet<>();
        for (String name : names) {
            if (!Set.of("CS", "DS", "ES", "SS", "FS", "GS").contains(name)
                    || !unique.add(name) || currentProgram.getRegister(name) == null)
                throw new IllegalArgumentException("Supply unique supported segment names");
        }
        int limit = Integer.parseInt(args[1]);
        if (limit < 1 || limit > 10000)
            throw new IllegalArgumentException("Output limit must be 1..10000");
        long scanned = 0, matched = 0, opaque = 0, missing = 0;
        List<String> results = new ArrayList<>();
        List<String> opaqueSites = new ArrayList<>();
        Map<String, Long> emptyKinds = new TreeMap<>();
        Map<String, String> emptyFirst = new TreeMap<>();
        InstructionIterator instructions = currentProgram.getListing().getInstructions(true);
        while (instructions.hasNext()) {
            monitor.checkCancelled();
            Instruction instruction = instructions.next();
            scanned++;
            PcodeOp[] effects = instruction.getPcode();
            if (effects.length == 0) {
                missing++;
                String kind = instruction.getMnemonicString().toUpperCase(Locale.ROOT);
                emptyKinds.merge(kind, 1L, Long::sum);
                emptyFirst.putIfAbsent(kind, instruction.getAddress().toString());
            }
            boolean hasOpaque = false;
            for (PcodeOp op : effects) if (op.getOpcode() == PcodeOp.CALLOTHER) hasOpaque = true;
            if (hasOpaque) {
                opaque++;
                if (opaqueSites.size() < limit) opaqueSites.add("opaqueAddress=" + instruction.getAddress());
            }
            Set<String> written = writes(currentProgram, instruction, names);
            if (!written.isEmpty()) {
                matched++;
                if (results.size() < limit)
                    results.add("address=" + instruction.getAddress() + " writes=" + String.join(",", written));
            }
        }
        for (String result : results) println(result);
        for (String site : opaqueSites) println(site);
        int emptyKindsEmitted = 0;
        for (Map.Entry<String, Long> kind : emptyKinds.entrySet()) {
            if (emptyKindsEmitted >= limit) break;
            println("emptyMnemonic=" + kind.getKey() + " count=" + kind.getValue()
                + " firstAddress=" + emptyFirst.get(kind.getKey()));
            emptyKindsEmitted++;
        }
        println("Completed decoded segment-output audit: scanned=" + scanned + " matches=" + matched
            + " emitted=" + results.size() + " truncated=" + (matched > limit)
            + " opaqueInstructions=" + opaque + " noPcodeInstructions=" + missing
            + " opaqueSitesEmitted=" + opaqueSites.size() + " opaqueSitesTruncated=" + (opaque > limit)
            + " emptyClasses=" + emptyKinds.size() + " emptyClassesEmitted=" + emptyKindsEmitted
            + " emptyClassesTruncated=" + (emptyKinds.size() > limit));
        println("Domain: current decoded listing only; opaque effects, undecoded bytes, external code,"
            + " runtime-generated instructions and initial descriptor bases are not established.");
    }
}
