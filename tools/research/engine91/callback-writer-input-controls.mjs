import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve} from 'node:path';
import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const base='C:/GOG Games/Dark Sun 2/analysis/reporter-audit',dir=`${base}/issue5-writer-callers91`;mkdirSync(dir,{recursive:true});
const owner=JSON.parse(readFileSync(`${base}/issue5-writers91/field-41241-batch-6.json`));
const writer=owner.regions.find(r=>r.start===209409);assert(writer);
const cfg={source:owner.source,sourceKind:owner.sourceKind,xxh3:owner.xxh3,regions:[writer,{name:'actual-stack-guard',start:0x8048,end:0x805a,segment:0x1000,ip:0x2e48,entries:[0x8048],evidence:'FND-CONFIG-163 actual guard'}],entry:209409,registers:{ds:0x57e0,ss:0x9000},maxSteps:64,maxPaths:4,totalSteps:256,visitLimit:4,returnBytes:4};
const query=(name,c,command='guards')=>{const f=`${dir}/${name}.json`;writeFileSync(f,JSON.stringify(c));const r=run([command,f]);writeFileSync(`${dir}/${name}.report.json`,JSON.stringify(r));console.log(JSON.stringify({name,steps:r.stepsUsed,complete:r.completeWithinModel,paths:r.paths?.map(p=>({returned:p.returned,site:p.stopSite,stop:p.stop,fields:p.events.filter(e=>e.kind==='write'&&e.site===209427).map(e=>({site:e.site,width:e.width,offset:e.offset?.value,producers:e.value?.producers}))})),counts:r.counts,confirmed:r.confirmed,partial:r.partialSearch,negativeUsable:r.negativeUsable,controls:r.relationalControls?.controls}));return r;};
const controlled=structuredClone(cfg);controlled.relationalControls=[{name:'actual-stacked-callback-input',kind:'origin',at:{site:209427,event:'write'},value:{field:'value'},expect:{producers:{include:[209423]}}}];
const pr=query('actual-input-producer',controlled);const ctl=pr.relationalControls.controls[0];assert.equal(ctl.verdict,'undecided');assert(ctl.paths.flatMap(p=>p.occurrences).length>0);assert(ctl.paths.flatMap(p=>p.occurrences).every(o=>o.verdict==='held'));
const wrong=structuredClone(controlled);wrong.relationalControls[0].expect.producers.include=[209409];try{query('wrong-prologue-producer',wrong);throw Error('wrong producer accepted')}catch(e){assert.match(e.message,/producer|violated/);console.log('Wrong prologue producer rejected');}
const cap=query('input-one-step',{...controlled,maxSteps:1});assert.equal(cap.relationalControls.controls[0].occurrences,0);
const pointerCfg={source:cfg.source,sourceKind:'mz',xxh3:cfg.xxh3,target:209409,query:{segment:writer.segment,offset:writer.ip}};const pointers=query('writer-relocated-pairs',pointerCfg,'pointers');console.log(JSON.stringify({pointerKeys:Object.keys(pointers),exact:pointers.exactPair,aliases:pointers.aliasedTarget}));
