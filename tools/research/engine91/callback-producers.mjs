import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import assert from 'node:assert/strict';
import {resolve} from 'node:path';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const base=GAME_DIR+'/analysis/reporter-audit';const dir=`${base}/issue5-producers91`;mkdirSync(dir,{recursive:true});
const c=JSON.parse(readFileSync(`${base}/issue5-root91/documented-runtime-dependencies.json`));
delete c.entryFrame;c.entry=c.regions.find(x=>x.name==='caller').start;c.relationalControls=[];
c.callModels[0].cases=[{registers:{bx:0}}];
const query=(name,config)=>{const f=`${dir}/${name}.json`;writeFileSync(f,JSON.stringify(config));const r=run(['effects',f]);writeFileSync(`${dir}/${name}.report.json`,JSON.stringify(r));console.log(JSON.stringify({name,steps:r.stepsUsed,paths:r.paths.length,gaps:r.gaps,complete:r.completeWithinModel,stops:r.paths.map(p=>({stop:p.stop,site:p.stopSite})),writes:r.paths.flatMap((p,i)=>p.events.filter(e=>e.kind==='write'&&[0x61a2,0x61a4,0xa0f1,0xa0f3].includes(e.offset?.value)).map(e=>({path:i,site:e.site,offset:e.offset?.value,value:e.value,interval:e.interval,guards:e.guards}))),controls:r.relationalControls?.controls}));return r;};
const initial=query('root-bx-zero',c);
const localRegion=c.regions.find(x=>x.name==='first-local-callee');
const writes=initial.paths.flatMap(p=>p.events).filter(e=>e.kind==='write'&&[0x61a2,0xa0f1].includes(e.offset?.value));
const unique=[...new Map(writes.map(e=>[e.site,e])).values()];assert.equal(unique.length,2);
const controlled=structuredClone(c);controlled.relationalControls=unique.map(e=>({name:e.offset.value===0x61a2?'overlay-registration-from-caller-literal':'resident-registration-from-caller-literal',kind:'origin',at:{site:e.site,event:'write'},value:{field:'value'},expect:{producers:{include:e.value.producers.filter(site=>site>=localRegion.start&&site<localRegion.end)}}}));
const result=query('caller-literal-origins',controlled);
for(const ctl of result.relationalControls.controls){const occ=ctl.paths.flatMap(p=>p.occurrences);assert(occ.length>0);assert(occ.every(o=>o.verdict==='held'));assert.equal(ctl.verdict,'undecided');}
const wrong=structuredClone(controlled);wrong.relationalControls=wrong.relationalControls.slice(0,1);wrong.relationalControls[0].expect.producers.include=[JSON.parse(readFileSync(`${base}/poll-alias-controls/boundary-selectors.json`)).copyBXToAX];
try{query('wrong-poll-producer',wrong);throw Error('wrong producer accepted')}catch(e){assert.match(e.message,/violated|not among|producer/);console.log('Wrong poll-result producer rejected');}
const capped=query('producer-one-step',{...controlled,maxSteps:1});assert(capped.relationalControls.controls.every(x=>x.occurrences===0));
const omitted=query('producer-without-overlay-setter',{...controlled,regions:controlled.regions.filter(x=>x.name!=='fallback-overlay-setter')});assert(omitted.relationalControls.controls.every(x=>x.occurrences===0));
console.log('Actual caller literal reaches both real registration stores; whole coverage stays undecided');
