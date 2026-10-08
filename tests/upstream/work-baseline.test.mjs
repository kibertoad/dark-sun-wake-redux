import {test} from 'node:test';
import assert from 'node:assert/strict';
import {union,bytes,inventoryDelta,validateRegion} from '../../tools/evidence/work-baseline.mjs';
test('baseline union counts shared tails once while preserving discontiguous gaps',()=>{
 assert.equal(bytes([[0,10],[8,20],[32,40]]),28);
 assert.deepEqual(union([[32,40],[8,20],[0,10]]),[[0,20],[32,40]]);
 assert.equal(bytes([[0,10],[10,12],[9,11]]),12);
 assert.throws(()=>bytes([[4,4]]),/Invalid body range/);
});
test('region audit rejects incomplete partitions and impossible outside counts',()=>{
 const valid={start:'1000:0000',size:100,body_bytes:60,instructions:70,instructions_outside:10,data:5,data_outside:5,undefined:25,undefined_outside:25};
 assert.doesNotThrow(()=>validateRegion(valid));
 assert.throws(()=>validateRegion({...valid,undefined:24}),/Invalid region partition/);
 assert.throws(()=>validateRegion({...valid,instructions_outside:71}),/Outside count exceeds total/);
 assert.throws(()=>validateRegion({...valid,body_bytes:101}),/Body count exceeds region/);
 assert.throws(()=>validateRegion({...valid,size:NaN}),/Invalid audit count/);
});
test('baseline changes separate discovery, removal and size changes with evidence unchanged',()=>{
 const previous=new Map([['A',10],['B',20],['C',30]]);
 const unchanged=new Map([['A',{size:10}],['B',{size:20}],['C',{size:30}]]);
 assert.deepEqual(inventoryDelta(previous,unchanged),{added:0,removedOrMerged:0,changedBodySizes:0});
 const changed=new Map([['A',{size:12}],['C',{size:30}],['D',{size:7}]]);
 assert.deepEqual(inventoryDelta(previous,changed),{added:1,removedOrMerged:1,changedBodySizes:1});
});
