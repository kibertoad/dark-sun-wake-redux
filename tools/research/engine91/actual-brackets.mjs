import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve} from 'node:path';
import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const base=GAME_DIR+'/analysis/reporter-audit';const dir=`${base}/issue5-brackets91`;mkdirSync(dir,{recursive:true});
const c=JSON.parse(readFileSync(`${base}/guard-order730/reload-stable.json`));
c.callModels=c.callModels.filter(x=>![210618,210632,210665,210679,210652].includes(x.site));
const add=(name,start,end,segment,ip,evidence)=>c.regions.push({name,start,end,segment,ip,entries:[start],evidence});
add('bracket-before',0x2f519,0x2f542,0x39d1,0x0609,'FND-CONFIG-171 complete before bracket');
add('bracket-after',0x2f4f1,0x2f519,0x39d1,0x05e1,'FND-CONFIG-171 complete after bracket');
add('gated-middle',0x38fcc,0x39055,0x4328,0x0b4c,'FND-CONFIG-171 complete local middle helper; transitive callees remain unread');
add('buffer-before',0x38598,0x385c9,0x4328,0x0118,'FND-CONFIG-175 complete buffer/coordinate helper');
add('buffer-after',0x38560,0x38598,0x4328,0x00e0,'FND-CONFIG-175 complete reverse buffer/coordinate helper');
add('copy-to-fixed',0x384da,0x384fc,0x4328,0x005a,'FND-CONFIG-099 complete fixed buffer helper');
add('copy-from-fixed',0x384fc,0x3851e,0x4328,0x007c,'FND-CONFIG-175 complete reverse copy wrapper');
add('coordinate-to-fixed',0x3851e,0x3853c,0x4328,0x009e,'FND-CONFIG-099 complete coordinate helper');
add('coordinate-from-fixed',0x3853c,0x38560,0x4328,0x00bc,'FND-CONFIG-175 complete coordinate output helper');
add('bounded-copy-wrapper',0x375e6,0x37616,0x4237,0x0076,'FND-CONFIG-175 complete pointer-tested copy wrapper');
add('actual-copy',0x5652,0x566e,0x1000,0x0452,'FND-CONFIG-175 complete forward word/byte copy with eight-byte cleanup');
add('shared-cs-setter',0x16a75,0x16a86,0x1bf3,0x5945,'FND-CONFIG-175 complete shared-CS pointer setter');
add('shared-cs-getter',0x18bdb,0x18be5,0x1bf3,0x7aab,'FND-CONFIG-175 complete shared-CS pointer getter');
const query=(name,cfg)=>{const f=`${dir}/${name}.json`;writeFileSync(f,JSON.stringify(cfg));const r=run(['guards',f]);writeFileSync(`${dir}/${name}.report.json`,JSON.stringify(r));console.log(JSON.stringify({name,steps:r.stepsUsed,frame:r.entryFrame,complete:r.completeWithinModel,gaps:r.gaps,paths:r.paths.map(p=>({returned:p.returned,stop:p.stop,site:p.stopSite,callback:p.events.filter(e=>e.kind==='call'&&e.indirectValue).map(e=>({site:e.site,status:e.status,guards:e.guards})),entries:[...new Set(p.events.map(e=>e.entry))]})),controls:r.relationalControls?.controls}));return r;};
const r=query('actual-brackets',c);assert(r.paths.some(p=>p.events.some(e=>e.entry===0x2f519)));
const omit=query('omitted-before-bracket',{...c,regions:c.regions.filter(x=>x.name!=='bracket-before')});assert(omit.paths.some(p=>[210618,210665].includes(p.stopSite)));
const cap=query('one-step',{...c,maxSteps:1});assert(!cap.paths.some(p=>p.events.some(e=>e.kind==='call'&&e.indirectValue)));
const actualGuard=structuredClone(c); actualGuard.callModels=actualGuard.callModels.filter(x=>x.site!==210605); actualGuard.regions.push({name:'actual-stack-guard',start:0x8048,end:0x805a,segment:0x1000,ip:0x2e48,entries:[0x8048],evidence:'FND-CONFIG-163 actual diagnostic/termination guard; INT21 remains unmodeled'}); const guarded=query('actual-brackets-and-guard',actualGuard); assert(!guarded.entryFrame.established); assert(guarded.paths.some(p=>p.events.some(e=>e.entry===0x2f519))); const controlled=structuredClone(actualGuard); controlled.relationalControls=[[210625,210616],[210672,210663]].map(([at,before])=>({name:`actual-target-${at}`,kind:'order',at:{site:at,event:'call'},before:{site:before,event:'branch'},branch:{taken:false},sameValue:{before:'left',at:'indirectValue'}}));
const relations=query('actual-bracket-relations',controlled); assert(relations.relationalControls.controls.every(x=>x.verdict==='undecided')); const second=relations.relationalControls.controls[1]; assert(second.paths.flatMap(p=>p.occurrences).length>0); assert(second.paths.flatMap(p=>p.occurrences).every(o=>o.verdict==='undecided'));
const wrong=structuredClone(controlled); wrong.relationalControls=wrong.relationalControls.slice(1); wrong.relationalControls[0].branch.taken=true; try{query('wrong-null-gate',wrong); throw Error('wrong gate accepted')}catch(e){assert.match(e.message,/violated|earlier branch|branch.*true/); console.log('Wrong callback null-gate rejected');}
const limited=query('relations-one-step',{...controlled,maxSteps:1}); assert(limited.relationalControls.controls.every(x=>x.occurrences===0));
const producers=structuredClone(actualGuard); producers.relationalControls=[92795,92799].map(site=>({name:`shared-cs-input-${site}`,kind:'origin',at:{site,event:'write'},value:{field:'value'},expect:{producers:{include:[193843]}}})); const produced=query('actual-shared-cs-producers',producers); for(const ctl of produced.relationalControls.controls){const occurrences=ctl.paths.flatMap(p=>p.occurrences); assert(occurrences.length>0); assert(occurrences.every(o=>o.verdict==='held')); assert.equal(ctl.verdict,'undecided');}
const getter=structuredClone(actualGuard); getter.relationalControls=[{name:'getter-not-assumed-to-retain-setter',kind:'origin',at:{site:101339,event:'read'},value:{field:'value'},expect:{producers:{include:[92795]}}}]; const lost=query('actual-getter-origin',getter); const go=lost.relationalControls.controls[0].paths.flatMap(p=>p.occurrences); assert(go.length>0); assert(go.every(o=>o.verdict==='undecided'));
console.log('Actual bracket dependencies retained at original query bounds; incomplete routes remain explicit');




