import {readFileSync,writeFileSync} from 'node:fs';import {resolve} from 'node:path';import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const d='C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-release-callers101';
const c=JSON.parse(readFileSync(`${d}/connected-producer-graph.json`));
c.regions.push({name:'referenced-adjacent-helper',start:88147,end:88618,segment:0x1bf3,ip:0x4723,entries:[88147],evidence:'FND-CONFIG-185 identifies this service; inventory-adjacent window, source body and effects require published boundary checks; native inputs remain unknown'});
for(const [name,q] of [['producer-helper-connected',c],['without-adjacent-helper',{...c,regions:c.regions.filter(r=>r.name!=='referenced-adjacent-helper')}],['without-request',{...c,regions:c.regions.filter(r=>r.name!=='documented-handle-request')}],['connected-one-instruction',{...c,instructionLimit:1}]]){
 const f=`${d}/${name}.json`;writeFileSync(f,JSON.stringify(q));const r=run(['callees',f]);writeFileSync(`${d}/${name}.report.json`,JSON.stringify(r));
 console.log(JSON.stringify({name,complete:r.completeWithinDeclaredGraph,nodes:r.nodes.map(n=>({entry:n.entry,complete:n.body.complete,usable:n.boundaryUsable,span:n.body.span,holes:n.body.holes,ports:n.body.hardwareBoundaries.map(h=>h.site)})),unresolved:r.edges.filter(e=>e.classification==='unresolved').map(e=>({site:e.site,target:e.target})),unchecked:r.uncheckedEntries}));
 if(name==='producer-helper-connected')assert(r.completeWithinDeclaredGraph&&r.edges.every(e=>e.boundaryUsable&&e.classification!=='unresolved'));else assert(!r.completeWithinDeclaredGraph);
}
