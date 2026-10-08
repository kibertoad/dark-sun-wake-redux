import {readFileSync,writeFileSync} from 'node:fs';import {resolve} from 'node:path';import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const d='C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-field-incoming101';
const c=JSON.parse(readFileSync(`${d}/documented-start-branch.json`)),s=JSON.parse(readFileSync(`${d}/start-dispatch-source-reading.json`));
const root=c.regions.find(r=>r.name==='caller-528688');root.entries=[root.start];root.ip=0;root.end=0x81457;
c.indirectJumps=[{site:s.jumpSite,exhaustive:true,evidence:'FND-UI-035/CONFIG-028 button input subtraction, unsigned upper gate and word-index shift; private source corroboration. Source table edges only, no native input or table preservation claim.',table:{start:s.tableStart,count:s.count,stride:2,evidence:'Existing documented four contiguous near-word targets, independently checked against source overlay base'}},{site:s.firstTable.jumpSite,exhaustive:true,evidence:'Private complete local loop reading initializes fixed count and selector base, advances by one word, jumps only after equality or takes exhaustion; target field has fixed parallel-array displacement. Source-table assumption, native selector/table state unverified.',table:{start:s.firstTable.tableStart,count:s.firstTable.count,stride:2,evidence:'Private bounded parallel-word-array target read; all source targets lie inside callback code'}}];
for(const [name,q] of [['root-source-tables',c],['without-button-table',{...c,indirectJumps:c.indirectJumps.slice(1)}],['without-selector-table',{...c,indirectJumps:c.indirectJumps.slice(0,1)}],['root-one-instruction',{...c,instructionLimit:1}]]){
 const f=`${d}/${name}.json`;writeFileSync(f,JSON.stringify(q));const r=run(['call-order',f]);writeFileSync(`${d}/${name}.report.json`,JSON.stringify(r));
 console.log(JSON.stringify({name,confirmed:r.incoming.confirmed.map(x=>x.site),callers:r.callers.map(x=>({entry:x.entry,usable:x.orderingUsable,gaps:x.gaps})),declarations:r.indirectJumpDeclarations}));
 if(name==='root-source-tables') {assert(r.incoming.confirmed.some(x=>x.site===0x812e5));assert(r.callers.some(x=>x.entry===0x81130&&x.orderingUsable));}
 else if(name==='root-one-instruction')assert(!r.incoming.confirmed.some(x=>x.site===0x812e5));
 else assert(!r.callers.some(x=>x.entry===0x81130&&x.orderingUsable));
}
