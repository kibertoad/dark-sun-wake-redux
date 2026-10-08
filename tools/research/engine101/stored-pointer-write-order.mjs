import {readFileSync} from 'node:fs';import assert from 'node:assert/strict';
const r=JSON.parse(readFileSync('UserContent/analysis/reporter-audit/stored-pointer-producer101/getter-store-suffix.report.json'));
for(const p of r.paths){const high=p.events.find(e=>e.kind==='write'&&e.site===193808),low=p.events.find(e=>e.kind==='write'&&e.site===193812);assert(high&&low);assert(high.order<low.order);assert.equal(high.width,2);assert.equal(low.width,2);assert.equal(high.offset.value,0xa059);assert.equal(low.offset.value,0xa057);}
console.log('Reached suffix reports retain both word stores with the high-word store before the low-word store.');
