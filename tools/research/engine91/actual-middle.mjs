import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve} from 'node:path';
import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const base='C:/GOG Games/Dark Sun 2/analysis/reporter-audit';const dir=`${base}/issue5-middle91`;mkdirSync(dir,{recursive:true});
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
const cfg=JSON.parse(readFileSync(`${base}/issue5-brackets91/actual-bracket-relations.json`));
cfg.regions.push(
 {name:'actual-normalizer',start:0x37225,end:0x372c0,segment:0x409b,ip:0x1675,entries:[0x37225],evidence:'FND-CONFIG-174 complete local body'},
 {name:'actual-service-before',start:0x334a4,end:0x336a3,segment:0x3d72,ip:0x0b84,entries:[0x334a4],evidence:'FND-CONFIG-188/189 complete local body'},
 {name:'actual-service-after',start:0x33262,end:0x3347f,segment:0x3d72,ip:0x0942,entries:[0x33262],evidence:'FND-CONFIG-188/189 complete local body'}
);
const r=query('actual-middle-services',cfg);
const reached=new Set(r.paths.flatMap(p=>p.events.map(e=>e.entry)));
console.log(JSON.stringify({actualServices:[0x37225,0x334a4,0x33262].filter(x=>reached.has(x))}));
assert(![0x37225,0x334a4,0x33262].some(x=>reached.has(x)));
const omit=query('omitted-middle-services',{...cfg,regions:cfg.regions.filter(x=>!x.name.startsWith('actual-service-')&&x.name!=='actual-normalizer')});
assert(!omit.paths.some(p=>p.events.some(e=>[0x37225,0x334a4,0x33262].includes(e.entry))));
const cap=query('one-step',{...cfg,maxSteps:1});
assert(!cap.paths.some(p=>p.events.some(e=>[0x37225,0x334a4,0x33262].includes(e.entry))));
console.log('Actual middle dependency controls passed; whole verdicts retained without invented state');
const narrow=structuredClone(cfg); narrow.entry=0x38fcc; delete narrow.entryFrame; narrow.relationalControls=[];
const nr=query('actual-middle-entry',narrow);
console.log(JSON.stringify({middleEntries:[...new Set(nr.paths.flatMap(p=>p.events.map(e=>e.entry)))]}));
assert(nr.paths.some(p=>p.events.some(e=>[0x37225,0x334a4,0x33262].includes(e.entry))));
const nc=query('middle-entry-one-step',{...narrow,maxSteps:1}); assert(!nc.paths.some(p=>p.events.some(e=>[0x37225,0x334a4,0x33262].includes(e.entry))));
const no=query('middle-entry-omitted-services',{...narrow,regions:narrow.regions.filter(x=>!x.name.startsWith('actual-service-')&&x.name!=='actual-normalizer')});
assert(no.paths.some(p=>p.stopSite===233462));
assert(!no.paths.some(p=>p.events.some(e=>[0x37225,0x334a4,0x33262].includes(e.entry))));
