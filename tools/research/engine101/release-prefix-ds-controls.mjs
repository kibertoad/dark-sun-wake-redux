import {readFileSync,writeFileSync} from 'node:fs';import {resolve} from 'node:path';import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const d=resolve(GAME_DIR+'/analysis/reporter-audit/release-prefix101');const c=JSON.parse(readFileSync(`${d}/prologue-trace.json`));
c.relationalControls.unshift({name:'prologue-ds-selection',kind:'relation',at:{site:91372,event:'checkpoint'},left:{field:'registers.ds'},op:'eq',right:0x1bf3});
for(const [name,q] of [['prefix-ds',c],['prefix-wrong-ds',{...c,relationalControls:[{...c.relationalControls[0],right:0x57e0}]}],['prefix-one-step',{...c,maxSteps:1}]]){
 const f=`${d}/${name}.json`;writeFileSync(f,JSON.stringify(q));let r;
 try{r=run(['trace',f]);}catch(e){if(name!=='prefix-wrong-ds')throw e;assert.match(e.message,/prologue-ds-selection violated/);writeFileSync(`${d}/${name}.rejection.txt`,e.message);console.log(JSON.stringify({name,rejected:true}));continue;}
 assert(name!=='prefix-wrong-ds');writeFileSync(`${d}/${name}.report.json`,JSON.stringify(r));const ctl=r.relationalControls.controls;
 if(name==='prefix-ds')assert(ctl[0].paths.some(p=>p.occurrences.some(o=>o.verdict==='held')));else assert(ctl.every(x=>x.occurrences===0));
 console.log(JSON.stringify({name,complete:r.completeWithinModel,prefix:{whole:ctl[0].verdict,occurrences:ctl[0].occurrences,local:ctl[0].paths.flatMap(p=>p.occurrences.map(o=>o.verdict))},laterOccurrences:ctl.slice(1).map(x=>x.occurrences),paths:r.paths.map(p=>({returned:p.returned,stop:p.stop,site:p.stopSite}))}));
}
const order={...c,target:91364,searchRegions:['source-local-incoming-window'],regions:[...c.regions,{name:'source-local-incoming-window',start:93710,end:94018,segment:0x1bf3,ip:93710-0x11130,entries:[93710],evidence:'Bounded local source scope only; native parent entry and argument admission remain unverified'}]};delete order.relationalControls;delete order.entry;
for(const [name,q] of [['prologue-incoming-order',order],['prologue-incoming-one-instruction',{...order,instructionLimit:1}]]){const f=`${d}/${name}.json`;writeFileSync(f,JSON.stringify(q));const r=run(['call-order',f]);writeFileSync(`${d}/${name}.report.json`,JSON.stringify(r));console.log(JSON.stringify({name,confirmed:r.incoming.confirmed.map(x=>x.site),candidates:r.incoming.candidates.map(x=>x.site),callers:r.callers.map(x=>({entry:x.entry,usable:x.orderingUsable,gaps:x.gaps}))}));if(name==='prologue-incoming-order')assert(r.incoming.confirmed.some(x=>x.site===93834));else assert(!r.incoming.confirmed.some(x=>x.site===93834));}
