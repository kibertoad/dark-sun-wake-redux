import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve} from 'node:path';
import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
import {readMz,incomingCalls} from '../../../node_modules/@scientific-method/executable-reader/dist/src/legacy-image.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const d=resolve(GAME_DIR+'/analysis/reporter-audit/release-prefix101');mkdirSync(d,{recursive:true});
const bytes=readFileSync(GAME_DIR+'/DSUN.EXE'),image=readMz(bytes);
const incoming=incomingCalls(image,91364,{limit:256});writeFileSync(`${d}/corrected-entry-incoming.json`,JSON.stringify(incoming));console.log(JSON.stringify({incoming}));
const base=image.header+(0x1bf3-0x1000)*16,near=[];
for(let site=base;site+3<=Math.min(base+0x10000,image.end);site++)if(bytes[site]===0xe8||bytes[site]===0xe9){const target=base+((site-base+3+bytes.readInt16LE(site+1))&0xffff);if(target===91364)near.push({site,kind:bytes[site]===0xe8?'call':'jump'});}
writeFileSync(`${d}/corrected-entry-near-candidates.json`,JSON.stringify({near,negativeUsable:false}));console.log(JSON.stringify({near}));
const c=JSON.parse(readFileSync(GAME_DIR+'/analysis/reporter-audit/release-connected-ds101/connected.json'));
c.entry=91364;c.regions=c.regions.map(r=>r.start===91236?{...r,name:'prologue-bound-release-caller',start:91364,ip:91364-base,entries:[91364],evidence:'Bounded source prologue excludes preceding table-like bytes; incoming admission remains conditional'}:r);
for(const [name,command,q] of [['prologue-graph','callees',c],['prologue-trace','trace',c],['prologue-one-step','trace',{...c,maxSteps:1}]]){
 const f=`${d}/${name}.json`;writeFileSync(f,JSON.stringify(q));const query=command==='callees'?{...q,relationalControls:undefined}:q;writeFileSync(f,JSON.stringify(query));const r=run([command,f]);writeFileSync(`${d}/${name}.report.json`,JSON.stringify(r));
 if(command==='callees'){assert(r.completeWithinDeclaredGraph);console.log(JSON.stringify({name,complete:r.completeWithinDeclaredGraph,nodes:r.nodes.map(n=>n.entry)}));}
 else {const controls=r.relationalControls.controls; console.log(JSON.stringify({name,complete:r.completeWithinModel,controls:controls.map(x=>({name:x.name,whole:x.verdict,occurrences:x.occurrences,local:x.paths.flatMap(p=>p.occurrences.map(o=>o.verdict))})),stops:r.paths.map(p=>({site:p.stopSite,reason:p.stop}))}));if(name==='prologue-one-step')assert(controls.every(x=>x.occurrences===0));}
}
