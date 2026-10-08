import {readFileSync,writeFileSync} from 'node:fs';import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';import assert from 'node:assert/strict';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
const root=GAME_DIR+'/analysis/reporter-audit/guard-order730';
for(const stable of [false,true]){
 const c=JSON.parse(readFileSync(`${root}/reload-${stable?'stable':'fresh'}.json`));c.relationalControls=[[210625,210616],[210672,210663]].map(([at,before])=>({name:`target-${at}`,kind:'order',at:{site:at,event:'call'},before:{site:before,event:'branch'},branch:{taken:false},sameValue:{before:'left',at:'indirectValue'}}));const name=`reload-${stable?'stable':'fresh'}-relations`;const f=`${root}/${name}.json`;writeFileSync(f,JSON.stringify(c));const r=run(['guards',f]);writeFileSync(`${root}/${name}.report.json`,JSON.stringify(r));
 for(const control of r.relationalControls.controls){const occurrences=control.paths.flatMap(p=>p.occurrences);assert(occurrences.length>0);assert(occurrences.every(o=>o.verdict===(stable?'held':'undecided')));assert(occurrences.every(o=>o.interveningCalls.length>0));}
 console.log(JSON.stringify({name,complete:r.completeWithinModel,allHeld:r.relationalControls.allHeld,controls:r.relationalControls.controls.map(c=>({name:c.name,verdict:c.verdict,occurrences:c.paths.flatMap(p=>p.occurrences).map(o=>o.verdict)}))}));
}
