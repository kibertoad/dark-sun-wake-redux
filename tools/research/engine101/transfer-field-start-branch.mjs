import {readFileSync,writeFileSync} from 'node:fs';import {resolve} from 'node:path';import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const d='C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-field-incoming101';
const c=JSON.parse(readFileSync(`${d}/allocator-field-writer-actual.json`));
c.regions=c.regions.filter(r=>r.name!=='caller-515384');c.searchRegions=['caller-528688'];
c.regions.find(r=>r.name==='caller-528688').entries=[0x8126e];
c.regions.find(r=>r.name==='caller-528688').ip=0x128;
for(const [name,q] of [['documented-start-branch',c],['start-branch-one-instruction',{...c,instructionLimit:1}]]) {
 const f=`${d}/${name}.json`;writeFileSync(f,JSON.stringify(q));const r=run(['call-order',f]);writeFileSync(`${d}/${name}.report.json`,JSON.stringify(r));
 console.log(JSON.stringify({name,confirmed:r.incoming.confirmed.map(x=>x.site),candidates:r.incoming.candidates.map(x=>x.site),callers:r.callers.map(x=>({entry:x.entry,usable:x.orderingUsable,gaps:x.gaps})),gaps:r.incoming.gaps}));
 if(name==='documented-start-branch')assert(r.incoming.confirmed.some(x=>x.site===0x812e5));else assert(!r.incoming.confirmed.some(x=>x.site===0x812e5));
}
