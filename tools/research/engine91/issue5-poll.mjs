import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve} from 'node:path';
import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence91/Scripts/python.exe');
const root='C:/GOG Games/Dark Sun 2/analysis/reporter-audit';
const dir=`${root}/issue5-poll91`;mkdirSync(dir,{recursive:true});
const c=JSON.parse(readFileSync(`${root}/engine811/poll-modeled.json`));
const sites=JSON.parse(readFileSync(`${root}/poll-alias-controls/boundary-selectors.json`));
const prior=JSON.parse(readFileSync(`${root}/engine811/poll-bx-0.report.json`));
const predicate=prior.paths[0].events.find(e=>e.kind==='branch');
const query=(name,config)=>{const f=`${dir}/${name}.json`;writeFileSync(f,JSON.stringify(config));const r=run(['effects',f]);writeFileSync(`${dir}/${name}.report.json`,JSON.stringify(r));console.log(JSON.stringify({name,frame:r.entryFrame,complete:r.completeWithinModel,gaps:r.gaps,stops:r.paths.map(p=>p.stop),controls:r.relationalControls?.controls}));return r;};
const origin={name:'predicate-from-interrupt-bx',kind:'origin',at:{site:predicate.site,event:'branch'},value:{field:'left'},expect:{inputs:{include:[{modeledCall:sites.interrupt,register:'bx'}]}}};
let q=structuredClone(c);q.callModels[0].cases=[{registers:{}}];q.relationalControls=[origin];
const r=query('unknown-bx-origin',q);
assert(r.relationalControls.controls[0].paths.flatMap(p=>p.occurrences).some(o=>o.verdict==='held'));
const cap=query('origin-cap',{...q,maxSteps:1});assert.equal(cap.relationalControls.controls[0].occurrences,0);
const unread=query('origin-unread',{...q,callModels:[]});assert.equal(unread.relationalControls.controls[0].occurrences,0);
query('origin-unscoped',{...q,callModels:q.callModels.map(({preservesMemory,...m})=>m)});
for(const bx of [0,1]){const s=structuredClone(q);s.callModels[0].cases=[{registers:{bx}}];query(`supplied-bx-${bx}`,s);}
const frame=structuredClone(q);delete frame.registers.sp;delete frame.registers.bp;frame.entryFrame={from:frame.regions[0].start};
const formed=query('actual-root-frame',frame);assert.equal(formed.entryFrame.established,false);
frame.regions.push({name:'first-local-callee',start:0x884c1,end:0x88521,segment:0x576c,ip:0x0bc1,entries:[0x884c1],evidence:'FND-CONFIG-162 complete first local callee; no preservation or field hypotheses'});
query('root-with-first-callee',frame);
const wrong=structuredClone(q);wrong.relationalControls[0].expect.inputs.include[0].register='cx';
const wr=query('wrong-origin-cx',wrong);
assert(!wr.relationalControls.controls[0].paths.flatMap(p=>p.occurrences).some(o=>o.verdict==='held'));
assert.equal(wr.relationalControls.controls[0].verdict,'undecided');
console.log('Wrong CX origin stays undecided and has no held occurrence');
console.log('Issue 5 poll origin/frame controls passed with whole acceptance still open');

const probe=structuredClone(q);probe.relationalControls.push({name:'scratch-last-dx',kind:'lastWriter',at:{site:predicate.site,event:'checkpoint'},address:{segment:'ss',base:'bp',displacement:-2,width:2},writers:[sites.storeDX]});
const pr=query('checkpoint-last-dx',probe);assert(pr.relationalControls.controls[1].paths.flatMap(p=>p.occurrences).some(o=>o.verdict==='held'));
const wp=structuredClone(probe);wp.relationalControls=wp.relationalControls.slice(1);wp.relationalControls[0].writers=[sites.storeCX];
try{query('checkpoint-wrong-cx',wp);throw Error('wrong writer accepted')}catch(e){assert.match(e.message,/violated|not among|writer/);console.log('Wrong CX last writer rejected');}
query('checkpoint-unscoped',{...probe,callModels:probe.callModels.map(({preservesMemory,...m})=>m)});
query('checkpoint-unread',{...probe,callModels:[]});query('checkpoint-cap',{...probe,maxSteps:1});
console.log('9.1.0 source checkpoint probes passed; whole caller frame and repeating route stay open');
