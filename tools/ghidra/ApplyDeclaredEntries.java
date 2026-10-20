// Applies the declared function entries of tools/ghidra/dsun-declared-entries.json to one view of
// DSUN.EXE. Arguments: the entries file, the view (resident or overlay) and, for the overlay view,
// the ReportFbovOverlayMap.ps1 mapping, which turns each overlay entry's shipped-file offset into
// its mapped segment:offset. In the resident view it first removes each `remove` entry's functions,
// clears the listing of its `clear` range and, where it names an `owner` function, disassembles
// every flow target inside that range from the owner's instructions and recomputes the owner's
// body; a cleared range with no owner is decoded again only from the entries. Then, for each entry of
// the view, it clears defined data at the entry, disassembles there and creates a function; a
// function whose body held the entry is recomputed so that it stops at the new one. Run it on a copy
// of a snapshot, without -readOnly, before exporting. It prints addresses and counts only.
// @category Restoration
import java.nio.file.*;
import java.util.*;
import com.google.gson.*;
import ghidra.app.cmd.function.CreateFunctionCmd;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.*;
import ghidra.program.model.listing.*;

public class ApplyDeclaredEntries extends GhidraScript {
    private Address range(String text, boolean end) {
        String[] pair = text.split("\\.\\.");
        return toAddr(pair[end ? 1 : 0]);
    }

    protected void run() throws Exception {
        String[] args = getScriptArgs();
        if (args.length < 2 || !Set.of("resident", "overlay").contains(args[1])) throw new IllegalArgumentException("Supply the entries file and resident or overlay.");
        JsonObject entries = JsonParser.parseString(Files.readString(Path.of(args[0]))).getAsJsonObject();
        FunctionManager fm = currentProgram.getFunctionManager();
        Listing listing = currentProgram.getListing();
        List<Address> starts = new ArrayList<>();
        if (args[1].equals("resident")) {
            for (JsonElement e : entries.getAsJsonArray("remove")) {
                JsonObject r = e.getAsJsonObject();
                Function owner = r.has("owner") ? fm.getFunctionAt(toAddr(r.get("owner").getAsString())) : null;
                if (r.has("owner") && owner == null) throw new IllegalStateException("No owner function " + r.get("owner").getAsString());
                Address low = range(r.get("clear").getAsString(), false), high = range(r.get("clear").getAsString(), true).subtract(1);
                AddressSet clear = new AddressSet(low, high);
                for (JsonElement s : r.getAsJsonArray("starts")) {
                    Address start = toAddr(s.getAsString());
                    if (fm.getFunctionAt(start) != null) { fm.removeFunction(start); println("REMOVED\t" + start); }
                    else println("ABSENT\t" + start);
                }
                clearListing(clear);
                if (owner == null) { println("CLEARED\t" + clear); continue; }
                boolean added = true;
                while (added) {
                    added = false;
                    for (Instruction i : listing.getInstructions(new AddressSet(owner.getEntryPoint(), high), true)) {
                        List<Address> targets = new ArrayList<>(Arrays.asList(i.getFlows()));
                        if (i.getFallThrough() != null) targets.add(i.getFallThrough());
                        for (Address t : targets) if (clear.contains(t) && getInstructionAt(t) == null) { disassemble(t); added = true; println("DISASSEMBLED\t" + t); }
                        if (added) break;
                    }
                }
                CreateFunctionCmd.fixupFunctionBody(currentProgram, owner, monitor);
                println("OWNER\t" + owner.getEntryPoint() + "\t" + owner.getBody().getNumAddresses() + "\t" + owner.getBody());
            }
            for (JsonElement e : entries.getAsJsonArray("resident")) starts.add(toAddr(e.getAsString()));
        } else {
            JsonObject map = JsonParser.parseString(Files.readString(Path.of(args[2]))).getAsJsonObject();
            for (JsonElement e : entries.getAsJsonArray("overlay")) {
                long at = Long.decode(e.getAsString());
                Address a = null;
                for (JsonElement m : map.getAsJsonArray("Overlays")) {
                    JsonObject o = m.getAsJsonObject();
                    long code = Long.decode(o.get("CodeFileOffset").getAsString());
                    if (at >= code && at < code + o.get("CodeBytes").getAsLong()) a = toAddr(o.get("MappedCodeStart").getAsString()).add(at - code);
                }
                if (a == null) throw new IllegalStateException("Overlay entry outside every code range: " + e.getAsString());
                starts.add(a);
            }
        }
        int created = 0, existed = 0, failed = 0;
        for (Address a : starts) {
            if (fm.getFunctionAt(a) != null) { existed++; println("EXISTS\t" + a); continue; }
            if (getInstructionAt(a) == null) {
                Data d = getDataContaining(a);
                if (d != null && d.isDefined()) clearListing(d.getMinAddress(), d.getMaxAddress());
                disassemble(a);
            }
            if (getInstructionAt(a) == null) { failed++; println("NOINSTRUCTION\t" + a); continue; }
            Function holder = fm.getFunctionContaining(a);
            if (holder != null) holder.setBody(new AddressSet(holder.getEntryPoint(), holder.getEntryPoint()));
            Function f = createFunction(a, null);
            if (holder != null) CreateFunctionCmd.fixupFunctionBody(currentProgram, holder, monitor);
            if (f == null) { failed++; println("NOFUNCTION\t" + a); continue; }
            created++;
            println("CREATED\t" + a + "\t" + f.getBody().getNumAddresses() + (holder != null ? "\tsplit " + holder.getEntryPoint() + " " + holder.getBody().getNumAddresses() : ""));
        }
        println("ENTRIES_COMPLETE\tview=" + args[1] + "\tcreated=" + created + "\texisted=" + existed + "\tfailed=" + failed);
    }
}
