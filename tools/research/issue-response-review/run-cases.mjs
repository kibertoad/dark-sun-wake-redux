import {readFileSync,writeFileSync} from 'node:fs';
import {run} from './reader/node_modules/@scientific-method/executable-reader/dist/src/report.js';
import assert from 'node:assert/strict';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
const local=GAME_DIR+'/analysis/reporter-audit/issue-response-review';
for(const name of ['gap35-register','gap35-consumer-conditional']) {
 const c=JSON.parse(readFileSync(`${GAME_DIR}/analysis/reporter-audit/${name}.json`));
 delete c.sha256;c.xxh3='e296af55ba2ecde7e77f555c90f33d0b';
 const file=`${local}/${name}.json`;writeFileSync(file,JSON.stringify(c));
 try {const r=run(['arguments',file]);writeFileSync(`${local}/${name}.report.json`,JSON.stringify(r));
 console.log(JSON.stringify({name,paths:r.paths?.length,reportBytes:JSON.stringify(r).length,gaps:r.gaps,sites:r.argumentFrameSites,stops:[...new Set(r.paths.map(p=>p.stop))]}));
 } catch(e){console.log(JSON.stringify({name,error:e.message}));}
}
const full=JSON.parse(readFileSync(`${local}/gap35-register.report.json`));
const known=full.paths.flatMap(p=>p.argumentFrames??[]).filter(f=>f.callSite===434930);
assert(known.length>0);assert(known.some(f=>f.groupings.some(g=>g.offset===6&&g.width===4)));
assert(full.argumentFrameSites.find(s=>s.callSite===434930).agreed===false);
const source=GAME_DIR+'/DSUN.EXE';
const base={source,sourceKind:'mz',xxh3:'e296af55ba2ecde7e77f555c90f33d0b',returnBytes:4,instructionLimit:2000,maxSteps:200,maxPaths:16,totalSteps:5000,visitLimit:4,registers:{ds:0x57e0,ss:0x9000,sp:0xf000}};
const consumer={name:'transfer',start:0x154de,end:0x15853,segment:0x1bf3,ip:0x43ae,entries:[0x154de],evidence:'FND-CONFIG-192 complete local body; unknown memory inputs'};
const producer={name:'producer',start:0x1384b,end:0x138d7,segment:0x1bf3,ip:0x271b,entries:[0x1384b],evidence:'FND-CONFIG-193 complete initializer; no producer-consumer call edge inferred'};
for(const [name,entry,regions,steps] of [['transfer',consumer.start,[consumer],200],['producer',producer.start,[producer,consumer],200],['producer-cap',producer.start,[producer,consumer],1]]) {
 const file=`${local}/${name}.json`;writeFileSync(file,JSON.stringify({...base,entry,regions,maxSteps:steps}));
 const r=run(['effects',file]);writeFileSync(`${local}/${name}.report.json`,JSON.stringify(r));
 assert(!r.completeWithinModel);
 if(name.startsWith('producer'))assert(!r.paths.some(p=>p.events.some(e=>e.site>=consumer.start&&e.site<consumer.end)));
 console.log(JSON.stringify({name,reportBytes:JSON.stringify(r).length,paths:r.paths.length,steps:r.stepsUsed,gaps:r.gaps,stops:[...new Set(r.paths.map(p=>p.stop))],returned:r.paths.filter(p=>p.returned).length,hardware:r.hardwareBoundaries.map(b=>({site:b.site,placement:b.placement}))}));
}
