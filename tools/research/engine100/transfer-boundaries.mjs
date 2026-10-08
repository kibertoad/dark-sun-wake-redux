import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve} from 'node:path';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const b='C:/GOG Games/Dark Sun 2/analysis/reporter-audit', d=`${b}/issue5-transfer-boundaries100`;
mkdirSync(d,{recursive:true});
const id=JSON.parse(readFileSync(`${b}/issue5-root91/documented-runtime-dependencies.json`));
const c={source:id.source,sourceKind:'mz',xxh3:id.xxh3,entry:0x154de,regions:[{name:'actual-transfer-primitive',start:0x154de,end:0x15853,segment:0x1bf3,ip:0x43ae,entries:[0x154de],evidence:'FND-CONFIG-192 complete source span; hardware values and native inputs unknown'}]};
for(const [n,q] of [['documented-primitive',c],['one-instruction',{...c,instructionLimit:1}],['before-common-transfer',{...c,regions:c.regions.map(r=>({...r,end:0x15710}))}]]) {
 const f=`${d}/${n}.json`;writeFileSync(f,JSON.stringify(q));const r=run(['bounds',f]);writeFileSync(`${d}/${n}.report.json`,JSON.stringify(r));
 console.log(JSON.stringify({name:n,keys:Object.keys(r),report:r}));
}