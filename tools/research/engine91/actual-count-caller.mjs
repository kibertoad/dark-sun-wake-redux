import {readFileSync,writeFileSync,mkdirSync} from 'node:fs'; import {resolve} from 'node:path'; import assert from 'node:assert/strict'; import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe'); const base=GAME_DIR+'/analysis/reporter-audit';const dir=`${base}/issue5-count91`;mkdirSync(dir,{recursive:true});
const c=JSON.parse(readFileSync(`${base}/result-origin730/pair-connected.json`));c.callModels=[];c.entry=0x2fae3;
const add=(name,start,end,segment,ip,evidence)=>c.regions.push({name,start,end,segment,ip,entries:[start],evidence});
add('actual-list-caller',0x2fae3,0x2fd93,0x3a8e,3,'FND-CONFIG-173 complete list caller; counts/graph/aliases remain unknown');
add('one-record-setup',0x37597,0x375e6,0x4237,0x27,'FND-CONFIG-176 actual count-one writer, real fill/copy/normalizer');
add('split-wrapper',0x382af,0x3835c,0x4237,0xd3f,'FND-CONFIG-177 complete wrapper; geometry admission unknown');
add('append-wrapper',0x383ec,0x38480,0x4237,0xe7c,'FND-CONFIG-177 complete append wrapper');
add('sentinel-walk',0x38125,0x38168,0x4237,0xbb5,'FND-CONFIG-177 complete sentinel walker');
add('sentinel',0x35b3e,0x35b73,0x4072,0x21e,'FND-CONFIG-177 complete four-word sentinel check');
add('split-body',0x376f0,0x38125,0x4237,0x180,'FND-CONFIG-178 bounded complete code region; detailed geometry interpretation and native admission remain open');
add('self-copy',0x3835c,0x383ec,0x4237,0xdec,'FND-CONFIG-178 complete full-region self-copy');
add('rectangle-equality',0x359b8,0x35a0d,0x4072,0x98,'FND-CONFIG-178 complete four-word equality');
add('coordinate-offset',0x39262,0x3927f,0x4400,0x62,'FND-UI-011 complete coordinate-offset helper');
add('position',0x3851e,0x3853c,0x4328,0x9e,'FND-CONFIG-099 complete position writer');
add('fixed-buffer-copy',0x384da,0x384fc,0x4328,0x5a,'FND-CONFIG-099 complete fixed-buffer wrapper');
add('actual-stack-guard',0x8048,0x805a,0x1000,0x2e48,'FND-CONFIG-163 actual guard; DOS remains unmodeled');
const rc=JSON.parse(readFileSync(`${base}/issue5-recursion91/documented-recursive-dependencies.json`));c.regions.push(...rc.regions.filter(x=>x.name!=='actual-runtime-guard'));
const query=(name,cfg)=>{const f=`${dir}/${name}.json`;writeFileSync(f,JSON.stringify(cfg));const r=run(['trace',f]);writeFileSync(`${dir}/${name}.report.json`,JSON.stringify(r));console.log(JSON.stringify({name,complete:r.completeWithinModel,steps:r.stepsUsed,gaps:r.gaps,paths:r.paths.map(p=>({returned:p.returned,stop:p.stop,site:p.stopSite,entries:[...new Set(p.events.map(e=>e.entry))]})),controls:r.relationalControls?.controls.map(x=>({name:x.name,verdict:x.verdict,occurrences:x.occurrences,reasons:x.reasons}))}));return r;};
const r=query('actual-list-count-producers',c);assert(r.paths.some(p=>p.events.some(e=>e.kind==='string-operation'&&e.operation==='stos')));
const omit=query('caller-without-fill',{...c,regions:c.regions.filter(x=>x.name!=='fill')});assert(!omit.paths.some(p=>p.events.some(e=>e.kind==='string-operation'&&e.operation==='stos')));const cap=query('caller-one-step',{...c,maxSteps:1});assert(!cap.paths.some(p=>p.events.some(e=>e.kind==='string-operation')));
const one=query('actual-one-record-root',{...c,entry:0x37597});
const controls=structuredClone(c);controls.relationalControls=[{name:'pair-count-from-actual-fill',kind:'origin',at:{site:230021,event:'read'},value:{field:'value'},expect:{producers:{include:[37274]}}},{name:'pair-count-last-written-by-fill',kind:'lastWriter',at:{site:230021,event:'read'},writers:[37274]}];const proved=query('actual-pair-count-controls',controls);for(const ctl of proved.relationalControls.controls){const occurrences=ctl.paths.flatMap(p=>p.occurrences);assert(occurrences.length>0);assert(occurrences.every(o=>o.verdict==='held'));assert.equal(ctl.verdict,'undecided');}
const wrong=structuredClone(controls);wrong.relationalControls=wrong.relationalControls.slice(1);wrong.relationalControls[0].writers=[37259];try{query('wrong-fill-preparation-writer',wrong);throw Error('wrong writer accepted')}catch(e){assert.match(e.message,/violated|writer/);console.log('Wrong fill-preparation writer rejected');}
const unread=query('count-controls-without-fill',{...controls,regions:controls.regions.filter(x=>x.name!=='fill')});assert(unread.relationalControls.controls.every(x=>x.occurrences===0));const limited=query('count-controls-one-step',{...controls,maxSteps:1});assert(limited.relationalControls.controls.every(x=>x.occurrences===0));
console.log('Actual fixed-region caller and count-one setup traversed without seeded memory or modeled guards');


