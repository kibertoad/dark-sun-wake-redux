import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';import {resolve} from 'node:path';import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const b=GAME_DIR+'/analysis/reporter-audit',d=`${b}/issue5-transfer-consumer-producer101`;mkdirSync(d,{recursive:true});
const c=JSON.parse(readFileSync(`${b}/issue5-transfer-caller-graph101/actual.json`));
c.entry=180389;c.regions.push({name:'documented-owned-literal-caller',start:179076,end:180627,segment:0x1000+Math.floor((179076-0x5200)/16),ip:(179076-0x5200)%16,entries:[179076,180389,180391],evidence:'Published resident consumer incoming ownership and source literal immediately preceding it; narrow entry is not proof of ancestor reachability'});
c.relationalControls=[{name:'consumer-request-literal-value',kind:'relation',at:{site:139027,event:'checkpoint'},left:{field:'registers.di'},op:'eq',right:1},{name:'consumer-request-literal-origin',kind:'origin',at:{site:139027,event:'checkpoint'},value:{field:'registers.di'},expect:{producers:{include:[180389]}}}];
c.relationalControls.push({name:'request-parameter-literal-value',kind:'relation',at:{site:80239,event:'checkpoint'},left:{field:'registers.di'},op:'eq',right:1},{name:'request-parameter-literal-origin',kind:'origin',at:{site:80239,event:'checkpoint'},value:{field:'registers.di'},expect:{producers:{include:[180389]}}});
for(const [name,q] of [['actual-literal-prefix',c],['without-literal-push',{...c,entry:180391}],['wrong-literal-value',{...c,relationalControls:[{...c.relationalControls[0],right:0}]}],['literal-one-step',{...c,maxSteps:1}]]){
 const f=`${d}/${name}.json`;writeFileSync(f,JSON.stringify(q));let r;
 try {r=run(['trace',f]);} catch(e) {if(name==='without-literal-push')assert.match(e.message,/consumer-request-literal-origin violated.*producer 180389 not among/s);else if(name==='wrong-literal-value')assert.match(e.message,/consumer-request-literal-value violated/s);else throw e;writeFileSync(`${d}/${name}.rejection.txt`,e.message);console.log(JSON.stringify({name,rejected:true}));continue;}
 if(name==='without-literal-push'||name==='wrong-literal-value')throw new Error('Expected rejected negative');
 writeFileSync(`${d}/${name}.report.json`,JSON.stringify(r));
 const controls=r.relationalControls.controls;
 console.log(JSON.stringify({name,complete:r.completeWithinModel,controls:controls.map(x=>({name:x.name,whole:x.verdict,occurrences:x.occurrences,local:x.paths.flatMap(p=>p.occurrences.map(o=>o.verdict))})),stops:r.paths.map(p=>({site:p.stopSite,reason:p.stop}))}));
 if(name==='actual-literal-prefix')assert(controls.every(x=>x.paths.some(p=>p.occurrences.some(o=>o.verdict==='held'))));
 else if(name==='without-literal-push')assert(controls.every(x=>!x.paths.some(p=>p.occurrences.some(o=>o.verdict==='held'))));
 else assert(controls.every(x=>x.occurrences===0));
}
