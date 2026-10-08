import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve} from 'node:path';
import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const root='C:/GOG Games/Dark Sun 2/analysis/reporter-audit';const dir=`${root}/issue5-initializer91`;mkdirSync(dir,{recursive:true});
const source=JSON.parse(readFileSync(`${root}/issue5-root91/documented-runtime-dependencies.json`));
const c={source:source.source,sourceKind:'mz',xxh3:source.xxh3,returnBytes:4,entry:0x1384b,registers:{ss:0x9000,ds:0x57e0},maxSteps:2048,totalSteps:4096,maxPaths:2,visitLimit:255,regions:[{name:'graphics-pool-initializer',start:0x1384b,end:0x138d8,segment:0x1bf3,ip:0x271b,entries:[0x1384b],evidence:'FND-CONFIG-193 complete call-free body; fixed 254-iteration loop, source-bounded visit count; SS/DS are explicit conditional input registers, not native admission'}]};
const query=(name,config)=>{const f=`${dir}/${name}.json`;writeFileSync(f,JSON.stringify(config));const r=run(['effects',f]);writeFileSync(`${dir}/${name}.report.json`,JSON.stringify(r));console.log(JSON.stringify({name,steps:r.stepsUsed,paths:r.paths.length,gaps:r.gaps,complete:r.completeWithinModel,stops:r.paths.map(p=>p.stop),controls:r.relationalControls?.controls.map(x=>({name:x.name,verdict:x.verdict,occurrences:x.occurrences,reasons:x.reasons}))}));return r;};
const initial=query('actual-initializer',c);assert.equal(initial.completeWithinModel,true);assert(initial.paths.every(p=>p.returned));
const events=initial.paths[0].events;const writes=events.filter(e=>e.kind==='write');const at=events.find(e=>e.kind==='return').site;
const fields=[['root-zero',4,0xa000],['root-one',6,0xa400],['next-paragraph',0xe4e,0xa7e8],['upper-paragraph',0xe4c,0xaffb]];
const controls=fields.map(([name,offset,value])=>{const write=writes.find(e=>e.offset?.value===offset&&e.width===2);assert(write,name);assert.equal(write.value.value,value);return {name,kind:'lastWriter',at:{site:at,event:'checkpoint'},address:{segment:'cs',displacement:offset,width:2},writers:[write.site]};});
const flagWrites=writes.filter(e=>e.width===1&&e.offset?.value>=0xc08&&e.offset?.value<=0xe02);assert.equal(flagWrites.length,254);assert(flagWrites.every(e=>e.value.value===0x80));
const flagSite=flagWrites[0].site;assert(flagWrites.every(e=>e.site===flagSite));
for(const offset of [0xc08,0xe02])controls.push({name:`flag-low-written-high-unwritten-${offset}`,kind:'lastWriter',at:{site:at,event:'checkpoint'},address:{segment:'cs',displacement:offset,width:2},byteWriters:[[flagSite],['entryState']]});
const controlled={...c,relationalControls:controls};const result=query('known-producer-checkpoints',controlled);assert(result.relationalControls.controls.every(x=>x.verdict==='held'));
const capped=query('one-step',{...controlled,maxSteps:1});assert(capped.relationalControls.controls.every(x=>x.occurrences===0));
const short=query('four-visits',{...controlled,visitLimit:4});assert.equal(short.completeWithinModel,false);assert(short.relationalControls.controls.every(x=>x.occurrences===0));
const wrong=structuredClone(controlled);wrong.relationalControls=wrong.relationalControls.slice(0,1);wrong.relationalControls[0].writers=controls[1].writers;
try{query('wrong-root-writer',wrong);throw Error('wrong writer accepted')}catch(e){assert.match(e.message,/violated|writer/);console.log('Wrong root writer rejected');}
console.log('Actual initializer known-answer roots/pool and low/high byte provenance controls passed; connected transfer remains separate');

