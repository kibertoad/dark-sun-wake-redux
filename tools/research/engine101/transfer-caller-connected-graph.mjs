import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve} from 'node:path';
import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const b=GAME_DIR+'/analysis/reporter-audit';
const d=`${b}/issue5-transfer-caller-graph101`;mkdirSync(d,{recursive:true});
const c=JSON.parse(readFileSync(`${b}/issue5-transfer-boundaries100/wrapper-callees.json`));
const region=(name,start,end,segment,ip,evidence)=>({name,start,end,segment,ip,entries:[start],evidence});
c.entry=139006;delete c.relationalControls;
c.regions.push(
 region('documented-070E-caller',139006,139137,0x2c5f,0x070e,'FND-CONFIG-186 complete caller span; physical field identity remains unknown'),
 region('documented-handle-request',0x1395d,0x139d9,0x1bf3,0x282d,'FND-CONFIG-191 complete request; slot/reference admission remains unknown'),
 region('documented-free-slot-scan',0x13837,0x1384b,0x1bf3,0x2707,'FND-CONFIG-183 complete scan; native flags not supplied'),
 region('documented-cleanup-wrapper',156113,156140,0x2d40,0x3bd1,'FND-CONFIG-184 complete cleanup wrapper'),
 region('documented-release-service',0x139f5,0x13a8f,0x1bf3,0x28c5,'FND-CONFIG-194 complete hardware-bearing release service; no device or alias model'));
for(const [name,q] of [['actual',c],['without-request',{...c,regions:c.regions.filter(r=>r.name!=='documented-handle-request')}],['without-release',{...c,regions:c.regions.filter(r=>r.name!=='documented-release-service')}],['one-instruction',{...c,instructionLimit:1}]]){
 const f=`${d}/${name}.json`;writeFileSync(f,JSON.stringify(q));
 const r=run(['callees',f]);writeFileSync(`${d}/${name}.report.json`,JSON.stringify(r));
 const summary={name,completeWithinDeclaredGraph:r.completeWithinDeclaredGraph,nodes:r.nodes.map(n=>({entry:n.entry,complete:n.body.complete,usable:n.boundaryUsable,hardwareSites:n.body.hardwareBoundaries.map(h=>h.site)})),unresolved:r.edges.filter(e=>e.classification==='unresolved').map(e=>({site:e.site,target:e.target,usable:e.boundaryUsable})),unchecked:r.uncheckedEntries};
 console.log(JSON.stringify(summary));
 if(name==='actual') {assert(r.nodes.length===12&&r.nodes.every(n=>n.body.complete&&n.boundaryUsable));assert(r.edges.every(e=>e.boundaryUsable&&e.classification!=='unresolved'));assert(r.completeWithinDeclaredGraph);}
 else {assert(!r.completeWithinDeclaredGraph);assert(r.edges.some(e=>e.classification==='unresolved')||r.nodes.some(n=>!n.body.complete||!n.boundaryUsable)||r.uncheckedEntries.length);}
}
