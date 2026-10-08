import {readFileSync,writeFileSync} from 'node:fs';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
import assert from 'node:assert/strict';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
const root=GAME_DIR+'/analysis/reporter-audit/runtime-mode-controls';
const read=name=>JSON.parse(readFileSync(`${root}/${name}.json`));
function q(name,c){const f=`${root}/${name}.json`;writeFileSync(f,JSON.stringify(c));const r=run(['effects',f]);writeFileSync(`${root}/${name}.report.json`,JSON.stringify(r));return r;}
const one=read('two-services');
const metadata=read('call-boundaries');
for(const m of one.callModels)assert(metadata.some(e=>e.site===m.site&&e.kind==='call'));
const bypass=[21921,...metadata.filter(e=>e.memoryDisplacements.some(d=>[0x3676,0x367a,0x367e].includes(d))).map(e=>e.site)];
assert.equal(bypass.length,4);
const r=q('mode-one-accepted',one);assert.equal(r.paths.length,1);assert.equal(r.gaps.length,0);assert(!r.completeWithinModel);assert.equal(r.nativeReachability,'unconfirmed');
const p=r.paths[0],summary=r.effectOrdering.paths[0];assert(!p.returned);assert(p.stop.includes('interrupt handler'));
for(const site of bypass)assert(!p.events.some(e=>e.kind==='call'&&e.site===site));
for(const [site,taken,value] of [[21905,true,1],[21955,false,0],[21959,true,1]])assert(summary.guards.some(g=>g.site===site&&g.taken===taken&&g.left.value===value));
for(const site of [21943,21948]){const c=summary.calls.find(c=>c.site===site);assert(c&&c.status==='modeled-return'&&c.unknownEffects);assert(Number.isInteger(c.conditionalModel));assert(p.conditionalModels[c.conditionalModel].preservedMemoryScopes.length>0);}
const termination=p.events.find(e=>e.kind==='hardware-boundary'&&e.site===0x53a5);assert(termination&&termination.boundary==='interrupt'&&termination.vector===0x21);assert.equal(p.registers.ah.value,0x4c);assert.equal(p.registers.al.value,null);
assert(r.hardwareBoundaries.some(b=>b.site===termination.site&&b.placement==='everyTracedPath'));
assert(summary.transactionality.startsWith('not established'));
const zero=q('mode-zero-control',read('mode-zero'));assert(!zero.completeWithinModel);assert(zero.paths.every(p=>p.guards.some(g=>g.site===21905&&g.taken===false&&g.left.value===0)));assert(zero.paths.some(p=>p.events.some(e=>e.kind==='call'&&e.site===21921)));assert(zero.paths.some(p=>p.events.some(e=>e.kind==='call'&&e.site===21934)));assert(!zero.paths.some(p=>p.returned));
const setter=q('setter-accepted',read('setter-guard'));assert.equal(setter.paths.length,2);assert(!setter.completeWithinModel);
for(const path of setter.paths){const gate=path.events.find(e=>e.kind==='branch'&&e.site===193327);assert.equal(gate.predicate,'jb');const store=path.events.find(e=>e.kind==='write'&&e.site===193338);const guard=path.events.find(e=>e.kind==='call'&&e.site===193329);
 if(gate.taken){assert(store&&store.width===4&&store.offset.value===0xa0f1&&gate.order<store.order);assert(!guard);assert(path.returned);}
 else{assert(guard&&gate.order<guard.order);assert(!store);assert(path.stop.includes('interrupt handler'));assert.equal(path.registers.ah.value,9);assert(!path.returned);}
}
assert(setter.hardwareBoundaries.some(b=>b.placement==='conditional'));
for(const [name,c] of [['one-step',{...one,maxSteps:1}],['one-unread',{...one,callModels:[]}],['one-unscoped',{...one,callModels:one.callModels.map(({preservesMemory,...m})=>m)}]]){
 const negative=q(name,c);assert(!negative.completeWithinModel);assert(!negative.paths.some(p=>p.returned));
 if(name==='one-unscoped'){assert(negative.paths.some(p=>p.stop.includes('return target')));assert(negative.hardwareBoundaries.some(b=>b.placement==='unresolved'));}
 else assert(!negative.hardwareBoundaries.some(b=>b.site===0x53a5));
}
console.log('Runtime mode and pre-store guard acceptance passes: mode-one callback/indirect bypasses, retained mode arguments through scoped unknown services, mode-zero callback dependency, conditional setter store, separate diagnostic and termination requests, cap/unread/unscoped controls; native outcomes stay unconfirmed');
