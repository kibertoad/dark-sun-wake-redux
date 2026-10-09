// Adds each FBOV overlay of an analyzed MZ import as a file-backed overlay block at its mapped
// segment, writes its fixup words, creates functions at its trampoline targets, reanalyzes only the
// overlay blocks and clips each overlay function's body to its own block. Run it on a copy of the
// resident snapshot, without -readOnly. Argument: the plan from fbov_overlay_plan.py. It prints
// addresses and counts only.
// @category Restoration
import java.nio.file.*;
import java.util.*;
import com.google.gson.*;
import ghidra.app.plugin.core.analysis.AutoAnalysisManager;
import ghidra.app.script.GhidraScript;
import ghidra.program.database.mem.FileBytes;
import ghidra.program.model.address.*;
import ghidra.program.model.listing.*;
import ghidra.program.model.mem.*;

public class AddFbovOverlayBlocks extends GhidraScript {
    protected void run() throws Exception {
        JsonArray plan = JsonParser.parseString(Files.readString(Path.of(getScriptArgs()[0]))).getAsJsonArray();
        Memory memory = currentProgram.getMemory();
        List<FileBytes> all = memory.getAllFileBytes();
        if (all.size() != 1) throw new IllegalStateException("expected one FileBytes, found " + all.size());
        FileBytes file = all.get(0);
        println("FILEBYTES\t" + file.getFilename() + "\t" + file.getSize());
        List<MemoryBlock> blocks = new ArrayList<>();
        AddressSet overlays = new AddressSet();
        for (JsonElement e : plan) {
            JsonObject o = e.getAsJsonObject();
            MemoryBlock block = memory.createInitializedBlock("fbov" + o.get("descriptor").getAsInt(),
                toAddr(o.get("start").getAsString()), file, o.get("fileOffset").getAsLong(), o.get("length").getAsLong(), true);
            block.setRead(true); block.setWrite(false); block.setExecute(true);
            for (JsonElement w : o.getAsJsonArray("writes")) {
                JsonArray pair = w.getAsJsonArray();
                memory.setShort(block.getStart().add(pair.get(0).getAsLong()), (short) pair.get(1).getAsInt());
            }
            blocks.add(block);
            overlays.add(block.getStart(), block.getEnd());
        }
        int seeded = 0;
        for (int i = 0; i < plan.size(); i++) {
            for (JsonElement t : plan.get(i).getAsJsonObject().getAsJsonArray("targets")) {
                Address a = blocks.get(i).getStart().add(t.getAsLong());
                disassemble(a);
                if (getFunctionAt(a) == null && createFunction(a, null) == null) println("NOFUNCTION\t" + a);
                seeded++;
            }
        }
        println("SEEDED\t" + seeded);
        AutoAnalysisManager mgr = AutoAnalysisManager.getAnalysisManager(currentProgram);
        mgr.reAnalyzeAll(overlays);
        mgr.startAnalysis(monitor);
        long clipped = 0, removed = 0, count = 0;
        for (MemoryBlock block : blocks) {
            AddressSet own = new AddressSet(block.getStart(), block.getEnd());
            for (Function f : currentProgram.getFunctionManager().getFunctions(own, true)) {
                count++;
                AddressSet outside = new AddressSet(f.getBody()).subtract(own);
                if (outside.isEmpty()) continue;
                f.setBody(new AddressSet(f.getBody()).intersect(own));
                println("CLIP\t" + f.getEntryPoint() + "\t" + outside.getNumAddresses() + "\t" + outside);
                clipped++; removed += outside.getNumAddresses();
            }
        }
        println("DONE\tfunctions=" + count + "\tclipped=" + clipped + "\tbytes_removed=" + removed);
    }
}
