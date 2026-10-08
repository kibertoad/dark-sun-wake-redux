import {readFileSync,writeFileSync} from 'node:fs';import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';import assert from 'node:assert/strict';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
const root=GAME_DIR+'/analysis/reporter-audit/guard-order730';
for(const stable of [false,true]){
 const c=JSON.parse(readFileSync(`${root}/reload-${stable?'stable':'fresh'}.json`));c.callModels=c.callModels.map(m=>({...m,cases:[{registers:{ax:[210625,210672].includes(m.site)?1:0}}]}));const name=`reload-${stable?'stable':'fresh'}-nonzero`;const f=`${root}/${name}.json`;writeFileSync(f,JSON.stringify(c));const r=run(['guards',f]);writeFileSync(`${root}/${name}.report.json`,JSON.stringify(r));assert(r.entryFrame.established);assert.equal(r.gaps.length,0);assert(r.paths.every(p=>p.returned));assert(r.paths.some(p=>p.registers.ax.value===0xffff));assert(r.paths.some(p=>p.registers.ax.value===0));assert.equal(r.nativeReachability,'unconfirmed');console.log(JSON.stringify({name,paths:r.paths.length,results:r.paths.map(p=>p.registers.ax.value),complete:r.completeWithinModel}));
}
