import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';import assert from 'node:assert/strict';
import {readMz,incomingCalls} from '../../../node_modules/@scientific-method/executable-reader/dist/src/legacy-image.js';import {sourceXxh3} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
const bytes=readFileSync('C:/GOG Games/Dark Sun 2/DSUN.EXE');assert.equal(sourceXxh3(bytes),'e296af55ba2ecde7e77f555c90f33d0b');
const image=readMz(bytes),d='C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-release-callers101';mkdirSync(d,{recursive:true});
const r=incomingCalls(image,80373,{limit:256,controls:[156129]});writeFileSync(`${d}/relocated-incoming.json`,JSON.stringify(r));console.log(JSON.stringify({relocated:r}));
const near=[];const base=image.header+(0x1bf3-0x1000)*16;
for(let site=base;site+3<=Math.min(base+0x10000,image.end);site++)if(bytes[site]===0xe8||bytes[site]===0xe9){const target=base+((site-base+3+bytes.readInt16LE(site+1))&0xffff);if(target===80373)near.push({site,kind:bytes[site]===0xe8?'near-call-byte-candidate':'near-jump-byte-candidate'});}
const controlSite=80231;assert.equal(bytes[controlSite],0xe8);assert.equal(base+((controlSite-base+3+bytes.readInt16LE(controlSite+1))&0xffff),79927);
const q={candidates:near,control:{site:controlSite,target:79927},negativeUsable:false,limitations:['Raw bytes are not instruction ownership.','Only the documented resident source segment view is scanned with 16-bit wrap; segment aliases, other contexts, computed/stored transfers remain unverified.','No native root-slot preservation follows from absent or guarded direct calls.']};writeFileSync(`${d}/independent-near-candidates.json`,JSON.stringify(q));console.log(JSON.stringify(q));
