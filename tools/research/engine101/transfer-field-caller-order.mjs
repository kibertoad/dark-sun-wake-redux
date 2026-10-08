import {readFileSync,writeFileSync} from 'node:fs';import {resolve} from 'node:path';
import assert from 'node:assert/strict';import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const b='C:/GOG Games/Dark Sun 2/analysis/reporter-audit',d=`${b}/issue5-transfer-field-incoming101`;
const base=JSON.parse(readFileSync(`${b}/issue5-transfer-mask100/destination-field-432567-actual.json`));delete base.query;delete base.entry;
const windows=JSON.parse(readFileSync(`${d}/caller-window-contexts.json`));
for(const [name,target,end,sites] of [['allocator-field-writer',432567,433525,[515890,529125]],['cleanup-field-writer',433525,433703,[118024]],['transfer-field-consumer',139006,139137,[164801,165711,180391]]]){
 const callers=windows.filter(w=>sites.includes(w.site)).map((w,i)=>({name:`caller-${w.start}`,start:w.start,end:w.end,segment:w.start<318000?0x1000+Math.floor((w.start-0x5200)/16):0x9000+i*0x1000,ip:w.start<318000?(w.start-0x5200)%16:0,entries:[w.start],evidence:'inventory-adjacent candidate window; resident header mapping or explicit overlay analysis view, not native DS'}));
 const targetRegion={name:`target-${target}`,start:target,end,segment:target===139006?0x2c5f:0xd000,ip:target===139006?0x070e:0,entries:[target],evidence:'previously verified target span; no native DS or producer-consumer identity claim'};
 const c={...base,target,searchRegions:callers.map(r=>r.name),regions:[...callers,targetRegion]};
 for(const [suffix,q] of [['actual',c],['one-instruction',{...c,instructionLimit:1}]]) {
  const f=`${d}/${name}-${suffix}.json`;writeFileSync(f,JSON.stringify(q));const r=run(['call-order',f]);writeFileSync(`${d}/${name}-${suffix}.report.json`,JSON.stringify(r));
  console.log(JSON.stringify({name,suffix,confirmed:r.incoming.confirmed.map(x=>x.site),incoming:r.incoming,callers:r.callers.map(x=>({entry:x.entry,usable:x.orderingUsable,gaps:x.gaps})),unchecked:r.uncheckedEntries}));
  if(suffix==='one-instruction')assert(!r.incoming.confirmed.some(x=>sites.includes(x.site)));
  else if(name!=='allocator-field-writer')assert(sites.every(site=>r.incoming.confirmed.some(x=>x.site===site)));
  else assert(sites.every(site=>r.incoming.candidates.some(x=>x.site===site)));
 }
}
