#!/usr/bin/env node
// Joins an FBOV executable's resident view and its overlay view into one range-aware inventory.
// Resident rows keep segment:offset notation; overlay rows become shipped-file offsets, and every
// overlay body range must lie in the Code ranges row that holds its entry (FND-EXE-520). Run
// ClipBodiesToEntryRegion.java on the overlay snapshot first. Addresses and counts only.
import {readFileSync, writeFileSync, mkdirSync} from 'node:fs';
import {resolve, join, dirname} from 'node:path';
import {fileURLToPath} from 'node:url';
import {createHash} from 'node:crypto';
import {sourceXxh3} from '@scientific-method/executable-reader';
import {union, bytes, validateRegion} from './work-baseline.mjs';
import {inventoryPath} from './inventory.mjs';
const text=p=>readFileSync(p,'utf8').replace(/^﻿/,'');
function rows(s){const lines=s.trimEnd().split(/\r?\n/),columns=lines.shift().split('\t');
 return {columns,rows:lines.map(l=>{const values=l.split('\t');if(values.length!==columns.length)throw Error('Invalid TSV width');return Object.fromEntries(columns.map((c,i)=>[c,values[i]]));})};}
const keyed=s=>Object.fromEntries(rows(s).rows.map(r=>[r.key,r.value]));
const segmented=s=>{if(!/^[0-9a-f]{4}:[0-9a-f]{4}$/i.test(s))throw Error('Expected segment:offset');return [parseInt(s.slice(0,4),16),parseInt(s.slice(5),16)];};
const seg=s=>s.toUpperCase();
const hex8=n=>'0x'+n.toString(16).toUpperCase().padStart(8,'0');
// Ghidra writes a range's exclusive end as a normalized segmented address (4842:063A..4000:8AE5).
// Write it in the start's segment, as the spec writes ranges, when the offset fits.
function sameSegment(range){const [a,b]=range.split('..'),[s,o]=segmented(a),[t,p]=segmented(b),end=(t-s)*16+p;
 return end>o&&end<=0xFFFF?`${seg(a)}..${seg(a.slice(0,5))}${end.toString(16).toUpperCase().padStart(4,'0')}`:`${seg(a)}..${seg(b)}`;}

// headerBytes: MZ header size; imageEnd: file offset after the load image; overlays: [{start,end,mappedStart}]
// with start..end the shipped code range and mappedStart the segment:offset of its first byte in the
// overlay view. expectedWithoutInstruction: entries each view is known to have with no decoded instruction.
export function joinOverlayViews({resident, overlay, headerBytes, imageEnd, overlays, expectedWithoutInstruction={resident:[],overlay:[]}}){
 const fileOf=a=>{const [s,o]=segmented(a);return headerBytes+(s-0x1000)*16+o;};
 for(const o of overlays)if(fileOf(o.mappedStart)!==o.start||!(o.end>o.start)||o.start<imageEnd)throw Error('Overlay mapping does not keep code in place');
 const ownRow=at=>overlays.find(o=>at>=o.start&&at<o.end);
 const out=[],all=[],regionsOut=[];let summed=0;const report={resident:0,overlay:0,withoutInstruction:[]};
 for(const [name,view] of [['resident',resident],['overlay',overlay]]){
  const inv=rows(view.tsv),p=keyed(view.provenance),audit=rows(view.regions).rows;
  if(inv.columns.join('\t')!=='start\tsize\tranges')throw Error('Expected complete body ranges');
  if(Number(p.body_bytes_outside_regions)!==0)throw Error(`The ${name} view has body bytes outside its regions`);
  const expected=expectedWithoutInstruction[name]??[];
  if(Number(p.entries_without_instruction)!==expected.length)throw Error(`The ${name} view's entries without an instruction differ from those expected`);
  audit.forEach(validateRegion);
  if(Number(p.functions)!==inv.rows.length)throw Error(`The ${name} view's function count differs from its provenance`);
  let viewSummed=0;const viewAll=[];
  for(const r of inv.rows){
   const at=fileOf(r.start),size=Number(r.size);
   if(!/^[1-9][0-9]*$/.test(r.size)||!Number.isSafeInteger(size))throw Error('Invalid size');
   const body=r.ranges.split(' ').map(part=>{const pair=part.split('..');if(pair.length!==2)throw Error('Invalid body range');return pair.map(fileOf);});
   if(body.reduce((n,[a,b])=>n+b-a,0)!==size||bytes(body)!==size||!body.some(([a,b])=>at>=a&&at<b))throw Error(`Invalid body at ${r.start}`);
   viewSummed+=size;viewAll.push(...body);
   if(name==='resident'){
    if(body.some(([a,b])=>a<headerBytes||b>imageEnd))throw Error(`Resident body outside the load image at ${r.start}`);
    out.push({at,line:`${seg(r.start)}\t${size}\t${r.ranges.split(' ').map(sameSegment).join(' ')}`});
   }else{
    const own=ownRow(at);
    if(!own)throw Error(`Overlay entry outside every code range at ${r.start}`);
    if(body.some(([a,b])=>a<own.start||b>own.end))throw Error(`Overlay body outside its entry's code range at ${r.start}`);
    out.push({at,line:`${hex8(at)}\t${size}\t${body.map(([a,b])=>hex8(a)+'..'+hex8(b)).join(' ')}`});
   }
   if(expected.includes(seg(r.start)))report.withoutInstruction.push(name==='resident'?seg(r.start):hex8(at));
   report[name]++;
  }
  if(viewSummed!==Number(p.summed_body_bytes)||bytes(viewAll)!==Number(p.unique_body_bytes))throw Error(`The ${name} view's body totals disagree`);
  summed+=viewSummed;all.push(...viewAll);
  for(const r of audit){const line=Object.values(r);
   if(name==='overlay'){const at=fileOf(r.start);if(!overlays.some(o=>o.start===at&&o.end===at+Number(r.size)))throw Error('Overlay region is not a code range');line[0]=hex8(at);}
   else line[0]=seg(r.start);
   regionsOut.push({at:fileOf(r.start),line:line.join('\t'),columns:Object.keys(r)});}
 }
 if(report.withoutInstruction.length!==expectedWithoutInstruction.resident.length+expectedWithoutInstruction.overlay.length)throw Error('An expected entry without an instruction is not in its view');
 out.sort((a,b)=>a.at-b.at);for(let i=1;i<out.length;i++)if(out[i].at===out[i-1].at)throw Error('Both views define one start');
 if(bytes(all)!==summed)throw Error('Bodies of the two views overlap');
 regionsOut.sort((a,b)=>a.at-b.at);
 return {tsv:'start\tsize\tranges\n'+out.map(x=>x.line+'\n').join(''),
  regions:regionsOut[0].columns.join('\t')+'\n'+regionsOut.map(x=>x.line+'\n').join(''),
  functions:out.length,summed,unique:bytes(all),report};
}

function main(){
 const [config,output]=process.argv.slice(2);if(!config||!output)throw Error('Supply the join configuration and a staging directory');
 const root=resolve(import.meta.dirname,'../..'),c=JSON.parse(text(config));
 const sha=p=>createHash('sha256').update(readFileSync(join(root,p))).digest('hex');
 const exporter=sha('tools/ghidra/ExportResearchBaseline.java'),clip=sha('tools/ghidra/ClipBodiesToEntryRegion.java'),joiner=sha('tools/evidence/join-overlay-views.mjs');
 const raw=readFileSync(c.source);
 if(sourceXxh3(raw)!==c.xxh3||createHash('md5').update(raw).digest('hex')!==c.md5)throw Error('Shipped source identity differs');
 const views={};
 for(const name of ['resident','overlay']){const v=c[name];
  for(const suffix of ['.tsv','.provenance.tsv','.regions.tsv'])if(text(v.stem+suffix)!==text(v.stem.replace(/-a$/,'-b')+suffix))throw Error('Repeated exports differ');
  const p=keyed(text(v.stem+'.provenance.tsv')),src=readFileSync(v.source);
  if(sourceXxh3(src)!==p.xxh3||createHash('md5').update(src).digest('hex')!==p.source_md5||p.exporter_revision!==exporter)throw Error(`The ${name} view's source or exporter identity differs`);
  if(name==='resident'&&p.xxh3!==c.xxh3)throw Error('The resident view did not read the shipped file');
  views[name]={tsv:text(v.stem+'.tsv'),provenance:text(v.stem+'.provenance.tsv'),regions:text(v.stem+'.regions.tsv'),p};
 }
 const m=JSON.parse(text(c.mapping));
 const overlays=m.Overlays.map(o=>({start:parseInt(o.CodeFileOffset,16),end:parseInt(o.CodeFileOffset,16)+o.CodeBytes,mappedStart:o.MappedCodeStart}));
 const joined=joinOverlayViews({resident:views.resident,overlay:views.overlay,headerBytes:m.HeaderBytes,imageEnd:m.HeaderBytes+m.ResidentImageBytes,overlays,expectedWithoutInstruction:c.expectedWithoutInstruction});
 const pr=views.resident.p,po=views.overlay.p;
 if(pr.analysis_version!==po.analysis_version)throw Error('The views come from different analysis versions');
 const provenance=['key\tvalue',`xxh3\t${c.xxh3}`,`source_md5\t${c.md5}`,'analysis_tool\tGhidra',`analysis_version\t${pr.analysis_version}`,
  `snapshot\t${pr.snapshot}+${po.snapshot}`,`exporter_revision\t${exporter}`,`overlay_view_xxh3\t${po.xxh3}`,`overlay_body_clip_revision\t${clip}`,
  `join_revision\t${joiner}`,'region_mode\tinitialized',`functions\t${joined.functions}`,`summed_body_bytes\t${joined.summed}`,
  `unique_body_bytes\t${joined.unique}`,'body_bytes_outside_regions\t0',`entries_without_instruction\t${joined.report.withoutInstruction.length}`,''].join('\n');
 const path=inventoryPath(c.build,c.manifest),dst=join(resolve(output),path);
 mkdirSync(dirname(dst),{recursive:true});
 // Exclusive writes never replace a file, so a staging directory holds one run.
 writeFileSync(dst,joined.tsv,{flag:'wx'});writeFileSync(dst.slice(0,-4)+'.provenance.tsv',provenance,{flag:'wx'});writeFileSync(dst.slice(0,-4)+'.regions.tsv',joined.regions,{flag:'wx'});
 console.log(JSON.stringify({staged:path,...joined.report,functions:joined.functions,summed_body_bytes:joined.summed}));
}
if(process.argv[1]&&resolve(process.argv[1])===fileURLToPath(import.meta.url))main();
