// Exports addresses/counts only. The caller supplies verified identity and snapshot provenance.
// @category Restoration
import java.nio.file.*;
import java.nio.charset.StandardCharsets;
import java.util.*;
import ghidra.app.script.GhidraScript;
import ghidra.framework.Application;
import ghidra.program.model.address.*;
import ghidra.program.model.listing.*;
import ghidra.program.model.mem.MemoryBlock;

public class ExportResearchBaseline extends GhidraScript {
    private String end(Address a) throws AddressOverflowException { return a.addNoWrap(1).toString(); }
    private long count(AddressSetView a, AddressSetView b) { return a.intersect(b).getNumAddresses(); }
    protected void run() throws Exception {
        String[] args=getScriptArgs();
        if(args.length<6) throw new IllegalArgumentException("Supply output stem, verified xxh3, snapshot ID, exporter revision, mode (executable or initialized), verified source MD5, then optional start..end regions.");
        Path stem=Path.of(args[0]).toAbsolutePath().normalize();
        if(!args[1].matches("[0-9a-f]{32}") || !Set.of("executable","initialized").contains(args[4])) throw new IllegalArgumentException("Invalid hash or region mode.");
        for(int i=1;i<4;i++) if(args[i].matches("(?s).*\\r.*") || args[i].contains("\n") || args[i].contains("\t")) throw new IllegalArgumentException("Invalid provenance text.");
        if(!args[5].matches("[0-9a-f]{32}") || !args[5].equalsIgnoreCase(currentProgram.getExecutableMD5())) throw new IllegalArgumentException("Snapshot/source MD5 mismatch.");
        AddressSet bodies=new AddressSet(), instructions=new AddressSet(), data=new AddressSet(), regions=new AddressSet();
        List<AddressSet> selected=new ArrayList<>();
        if(args.length>6) {
            for(int i=6;i<args.length;i++) {
                String[] pair=args[i].split("\\.\\.");
                if(pair.length!=2) throw new IllegalArgumentException("Region must be start..exclusive-end.");
                Address start=toAddr(pair[0]), stop=toAddr(pair[1]);
                if(start==null || stop==null || start.compareTo(stop)>=0) throw new IllegalArgumentException("Invalid region.");
                AddressSet r=new AddressSet(start,stop.subtract(1));
                if(!regions.intersect(r).isEmpty() || !currentProgram.getMemory().contains(r)) throw new IllegalArgumentException("Overlapping or unmapped region.");
                selected.add(r); regions.add(r);
            }
        } else for(MemoryBlock block:currentProgram.getMemory().getBlocks()) {
            if(block.isInitialized() && block.isLoaded() && (args[4].equals("initialized") || block.isExecute())) {
                AddressSet r=new AddressSet(block.getStart(),block.getEnd()); selected.add(r); regions.add(r);
            }
        }
        if(regions.isEmpty()) throw new IllegalStateException("No measured regions.");
        StringBuilder inventory=new StringBuilder("start\tsize\tranges\n");
        long functions=0, missingInstructions=0, summedBodies=0;
        FunctionIterator fs=currentProgram.getFunctionManager().getFunctions(true);
        while(fs.hasNext()) {
            monitor.checkCancelled(); Function f=fs.next(); if(f.isExternal() || !regions.contains(f.getEntryPoint())) continue;
            AddressSetView body=f.getBody();
            if(body.isEmpty() || !body.contains(f.getEntryPoint())) throw new IllegalStateException("Invalid function body at "+f.getEntryPoint());
            if(currentProgram.getListing().getInstructionAt(f.getEntryPoint())==null) missingInstructions++;
            inventory.append(f.getEntryPoint()).append('\t').append(body.getNumAddresses()).append('\t');
            AddressRangeIterator ranges=body.getAddressRanges(); boolean first=true;
            while(ranges.hasNext()) { AddressRange r=ranges.next(); if(!first)inventory.append(' ');first=false;
                inventory.append(r.getMinAddress()).append("..").append(end(r.getMaxAddress())); }
            inventory.append('\n'); functions++; summedBodies+=body.getNumAddresses(); bodies.add(body);
        }
        InstructionIterator ins=currentProgram.getListing().getInstructions(true);
        while(ins.hasNext()) {monitor.checkCancelled(); Instruction i=ins.next(); instructions.add(i.getMinAddress(),i.getMaxAddress());}
        DataIterator ds=currentProgram.getListing().getDefinedData(true);
        while(ds.hasNext()) {monitor.checkCancelled(); Data d=ds.next(); data.add(d.getMinAddress(),d.getMaxAddress());}
        instructions=instructions.intersect(regions);data=data.intersect(regions);
        if(!instructions.intersect(data).isEmpty())throw new IllegalStateException("Instruction/data classes overlap.");
        AddressSet undefined=regions.subtract(instructions.union(data));
        StringBuilder audit=new StringBuilder("start\tsize\tbody_bytes\tinstructions\tinstructions_outside\tdata\tdata_outside\tundefined\tundefined_outside\n");
        for(AddressSet r:selected) {
            long size=r.getNumAddresses(),i=count(instructions,r),d=count(data,r),u=count(undefined,r);
            if(i+d+u!=size)throw new IllegalStateException("Partition does not sum to region size.");
            audit.append(r.getMinAddress()).append('\t').append(size).append('\t').append(count(bodies,r)).append('\t')
                .append(i).append('\t').append(count(instructions.subtract(bodies),r)).append('\t')
                .append(d).append('\t').append(count(data.subtract(bodies),r)).append('\t')
                .append(u).append('\t').append(count(undefined.subtract(bodies),r)).append('\n');
        }
        String provenance="key\tvalue\nxxh3\t"+args[1]+"\nsource_md5\t"+args[5]+"\nanalysis_tool\tGhidra\nanalysis_version\t"+Application.getApplicationVersion()+
            "\nsnapshot\t"+args[2]+"\nexporter_revision\t"+args[3]+"\nregion_mode\t"+args[4]+
            "\nfunctions\t"+functions+"\nsummed_body_bytes\t"+summedBodies+"\nunique_body_bytes\t"+bodies.getNumAddresses()+
            "\nbody_bytes_outside_regions\t"+bodies.subtract(regions).getNumAddresses()+"\nentries_without_instruction\t"+missingInstructions+"\n";
        Path[] paths={Path.of(stem+".tsv"),Path.of(stem+".provenance.tsv"),Path.of(stem+".regions.tsv")};
        for(Path p:paths)if(Files.exists(p))throw new IllegalArgumentException("Output exists: "+p);
        Files.createDirectories(stem.getParent());
        String[] texts={inventory.toString(),provenance,audit.toString()};
        List<Path> temporary=new ArrayList<>();
        try {for(int i=0;i<3;i++){Path tmp=Files.createTempFile(stem.getParent(),"baseline-",".partial");temporary.add(tmp);Files.writeString(tmp,texts[i],StandardCharsets.UTF_8);}
            for(int i=0;i<3;i++)Files.move(temporary.get(i),paths[i]);}
        finally {for(Path tmp:temporary)Files.deleteIfExists(tmp);}
        println("BASELINE_COMPLETE functions="+functions+" regions="+selected.size()+" bytes="+regions.getNumAddresses());
    }
}
