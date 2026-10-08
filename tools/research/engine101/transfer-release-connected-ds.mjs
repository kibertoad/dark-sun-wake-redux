import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve} from 'node:path';
import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const b=GAME_DIR+'/analysis/reporter-audit/issue5-transfer-release-callers101';
const d=resolve(GAME_DIR+'/analysis/reporter-audit/release-connected-ds101');mkdirSync(d,{recursive:true});
const c=JSON.parse(readFileSync(`${b}/producer-helper-connected.json`));
const reading=JSON.parse(readFileSync(`${b}/direct-release-producer-reading.json`));
const sites=[...reading.ownFieldWriters.map(x=>x.site),...reading.argumentFields.map(x=>x.pushSite)];
c.relationalControls=sites.map(site=>({name:`current-ds-${site}`,kind:'relation',at:{site,event:'checkpoint'},left:{field:'registers.ds'},op:'eq',right:0x1bf3}));
for(const [name,q] of [['connected',c],['without-request',{...c,regions:c.regions.filter(r=>r.name!=='documented-handle-request')}],['one-step',{...c,maxSteps:1}]]){
 const f=`${d}/${name}.json`;writeFileSync(f,JSON.stringify(q));
 const r=run(['trace',f]);writeFileSync(`${d}/${name}.report.json`,JSON.stringify(r));
 const controls=r.relationalControls.controls;
 const summary={name,complete:r.completeWithinModel,controls:controls.map(x=>({name:x.name,whole:x.verdict,occurrences:x.occurrences,local:x.paths.flatMap(p=>p.occurrences.map(o=>o.verdict))})),stops:r.paths.map(p=>({site:p.stopSite,reason:p.stop}))};
 writeFileSync(`${d}/${name}.summary.json`,JSON.stringify(summary,null,2));console.log(JSON.stringify(summary));
 assert(!r.completeWithinModel);
 if(name==='one-step')assert(controls.every(x=>x.occurrences===0));
}
