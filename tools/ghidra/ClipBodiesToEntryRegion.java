// Removes from each function body the addresses outside the region that holds its entry.
// For an FBOV mapped snapshot the regions are the overlays' mapped code ranges: FND-EXE-520 shows an
// overlay's segment holds only its own code and fixups at offset 0, so near flow from an entry in one
// overlay cannot reach another overlay's code, padding or the resident image. Run it on a copy of a
// snapshot, without -readOnly, before exporting. It prints addresses and counts only.
// @category Restoration
import java.util.*;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.*;
import ghidra.program.model.listing.*;

public class ClipBodiesToEntryRegion extends GhidraScript {
    protected void run() throws Exception {
        String[] args=getScriptArgs();
        if(args.length<1) throw new IllegalArgumentException("Supply start..exclusive-end regions.");
        List<AddressSet> selected=new ArrayList<>(); AddressSet all=new AddressSet();
        for(String arg:args) {
            String[] pair=arg.split("\\.\\.");
            if(pair.length!=2) throw new IllegalArgumentException("Region must be start..exclusive-end.");
            Address start=toAddr(pair[0]), stop=toAddr(pair[1]);
            if(start==null || stop==null || start.compareTo(stop)>=0) throw new IllegalArgumentException("Invalid region.");
            AddressSet r=new AddressSet(start,stop.subtract(1));
            if(!all.intersect(r).isEmpty() || !currentProgram.getMemory().contains(r)) throw new IllegalArgumentException("Overlapping or unmapped region.");
            selected.add(r); all.add(r);
        }
        long clipped=0, removedBytes=0, examined=0;
        FunctionIterator fs=currentProgram.getFunctionManager().getFunctions(true);
        List<Function> functions=new ArrayList<>();
        while(fs.hasNext()) { Function f=fs.next(); if(!f.isExternal() && all.contains(f.getEntryPoint())) functions.add(f); }
        for(Function f:functions) {
            monitor.checkCancelled(); examined++;
            AddressSet region=null;
            for(AddressSet r:selected) if(r.contains(f.getEntryPoint())) { region=r; break; }
            AddressSetView body=f.getBody();
            if(!body.contains(f.getEntryPoint())) throw new IllegalStateException("Body lacks its entry at "+f.getEntryPoint());
            AddressSet removed=new AddressSet(body).subtract(region);
            if(removed.isEmpty()) continue;
            f.setBody(new AddressSet(body).intersect(region));
            StringBuilder line=new StringBuilder("CLIP\t").append(f.getEntryPoint()).append('\t').append(removed.getNumAddresses()).append('\t');
            AddressRangeIterator ranges=removed.getAddressRanges(); boolean first=true;
            while(ranges.hasNext()) { AddressRange r=ranges.next(); if(!first) line.append(' '); first=false;
                line.append(r.getMinAddress()).append("..").append(r.getMaxAddress().addNoWrap(1)); }
            println(line.toString()); clipped++; removedBytes+=removed.getNumAddresses();
        }
        println("CLIP_COMPLETE\tfunctions_examined="+examined+"\tfunctions_clipped="+clipped+"\tbytes_removed="+removedBytes);
    }
}
