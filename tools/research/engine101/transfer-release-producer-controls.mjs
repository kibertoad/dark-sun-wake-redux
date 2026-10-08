import {readFileSync,writeFileSync} from 'node:fs';import {resolve} from 'node:path';import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const b=GAME_DIR+'/analysis/reporter-audit',d=`${b}/issue5-transfer-release-callers101`;
const base=JSON.parse(readFileSync(`${d}/actual-near-callers.json`));delete base.target;delete base.searchRegions;
const reading=JSON.parse(readFileSync(`${d}/direct-release-producer-reading.json`));
base.regions=base.regions.filter(r=>r.start===91236);
for(const field of [...new Set(reading.argumentFields.map(x=>x.field))]){
 const c={...base,entry:91236,query:{offset:field}};
 const f=`${d}/field-${field}-owned.json`;writeFileSync(f,JSON.stringify(c));const r=run(['operand-candidates',f]);writeFileSync(`${d}/field-${field}-owned.report.json`,JSON.stringify(r));
 const writes=reading.ownFieldWriters.filter(x=>x.field===field),reads=reading.argumentFields.filter(x=>x.field===field);
 assert(writes.every(x=>r.candidates.some(y=>y.site===x.site&&y.countedAsUse&&y.width===2&&y.access.includes('write'))));
 assert(reads.every(x=>r.candidates.some(y=>y.site===x.pushSite&&y.countedAsUse&&y.width===2&&y.access.includes('read'))));
 console.log(JSON.stringify({field,ownedWrites:writes.map(x=>x.site),ownedArgumentReads:reads.map(x=>x.pushSite),negativeUsable:r.negativeUsable}));
}
const c={...base,entry:91236,query:{offset:reading.argumentFields[0].field},instructionLimit:1};const f=`${d}/producer-field-one-instruction.json`;writeFileSync(f,JSON.stringify(c));const r=run(['operand-candidates',f]);writeFileSync(`${d}/producer-field-one-instruction.report.json`,JSON.stringify(r));assert(!r.candidates.some(y=>y.countedAsUse&&reading.argumentFields.some(x=>x.pushSite===y.site)));console.log('One-instruction control loses source field ownership.');
const core=JSON.parse(readFileSync(`${b}/issue5-transfer-caller-graph101/actual.json`));
for(const [name,target,regionNames] of [['request-return',80221,['documented-handle-request','documented-free-slot-scan']],['allocator-return',80088,['documented-free-slot-scan']]]){
 const q={...base,target,searchRegions:base.regions.map(r=>r.name),regions:[...base.regions,...core.regions.filter(r=>regionNames.includes(r.name))]};
 if(target===80088)q.regions.push({name:'documented-slot-allocator',start:80088,end:80221,segment:0x1bf3,ip:0x27a8,entries:[80088],evidence:'FND-CONFIG-183 complete allocator; accepted state and preservation remain unverified'});
 const file=`${d}/producer-${name}-order.json`;writeFileSync(file,JSON.stringify(q));const report=run(['call-order',file]);writeFileSync(`${d}/producer-${name}-order.report.json`,JSON.stringify(report));assert(report.incoming.confirmed.length&&report.callers.some(x=>x.entry===91236&&x.orderingUsable));console.log(JSON.stringify({name,confirmed:report.incoming.confirmed.map(x=>x.site),orderingUsable:true}));
}
