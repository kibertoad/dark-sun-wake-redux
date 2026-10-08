import {readFileSync,writeFileSync} from 'node:fs';import {resolve} from 'node:path';import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const b=GAME_DIR+'/analysis/reporter-audit',d=`${b}/issue5-transfer-release-callers101`;
const c=JSON.parse(readFileSync(`${b}/issue5-transfer-caller-graph101/actual.json`));delete c.entry;delete c.relationalControls;
const windows=JSON.parse(readFileSync(`${d}/near-caller-windows.json`));
const owners=[...new Map(windows.map(w=>[w.start,w])).values()];
c.regions=[...owners.map(w=>({name:`direct-release-caller-${w.start}`,start:w.start,end:w.end,segment:0x1bf3,ip:w.start-0x11130,entries:[w.start],evidence:'Inventory-adjacent source window inside the documented graphics resident segment; original handle producers remain unknown'})),c.regions.find(r=>r.name==='documented-release-service')];
c.target=80373;c.searchRegions=owners.map(w=>`direct-release-caller-${w.start}`);
for(const [name,q] of [['actual-near-callers',c],['near-callers-one-instruction',{...c,instructionLimit:1}]]){
 const f=`${d}/${name}.json`;writeFileSync(f,JSON.stringify(q));const r=run(['call-order',f]);writeFileSync(`${d}/${name}.report.json`,JSON.stringify(r));
 const sites=r.incoming.confirmed.map(x=>x.site);console.log(JSON.stringify({name,confirmed:sites,candidates:r.incoming.candidates.map(x=>x.site),callers:r.callers.map(x=>({entry:x.entry,usable:x.orderingUsable,gaps:x.gaps,calls:x.calls.map(y=>({site:y.site,guards:y.necessaryGuards}))}))}));
 if(name==='actual-near-callers'){assert(windows.filter(w=>w.start===91236).every(w=>sites.includes(w.site)));assert(windows.filter(w=>w.start!==91236).every(w=>r.incoming.candidates.some(x=>x.site===w.site)));assert(r.callers.some(x=>x.entry===91236&&x.orderingUsable));}else assert(!sites.some(site=>windows.some(w=>w.site===site)));
}
