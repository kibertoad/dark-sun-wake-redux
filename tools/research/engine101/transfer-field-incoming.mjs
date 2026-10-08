import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import assert from 'node:assert/strict';
import {sourceXxh3} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
import {readMz,incomingCalls} from '../../../node_modules/@scientific-method/executable-reader/dist/src/legacy-image.js';
const bytes=readFileSync('C:/GOG Games/Dark Sun 2/DSUN.EXE');assert.equal(sourceXxh3(bytes),'e296af55ba2ecde7e77f555c90f33d0b');
const image=readMz(bytes),d='C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-field-incoming101';mkdirSync(d,{recursive:true});
for(const [name,target] of [['allocator-field-writer',432567],['cleanup-field-writer',433525],['transfer-field-consumer',139006]]){
 const r=incomingCalls(image,target,{limit:256,controls:[155995]});
 writeFileSync(`${d}/${name}.json`,JSON.stringify(r));
 console.log(JSON.stringify({name,report:r}));
}
