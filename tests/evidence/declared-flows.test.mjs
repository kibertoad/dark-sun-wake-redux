import {test} from 'node:test';
import assert from 'node:assert/strict';
import {spawnSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';
import {enginePython} from '../../tools/tool-dependencies.mjs';

const generator=fileURLToPath(new URL('../../tools/research/exec-census/walk_declared_flows.py',import.meta.url));
const code='import json,runpy,sys; m=runpy.run_path(sys.argv[1]); p=json.load(sys.stdin); print(json.dumps(m["merge_decode"](p["generated"],p["previous"])))';
function merge(generated,previous){
 const result=spawnSync(enginePython(),['-I','-c',code,generator],{input:JSON.stringify({generated,previous}),encoding:'utf8',maxBuffer:65536});
 if(result.error)throw result.error;
 if(result.status!==0)throw new Error(result.stderr);
 return JSON.parse(result.stdout);
}
const repair={range:'0x00000110..0x00000120',owner:'0x00000100',evidence:'synthetic reviewed tail'};
test('evidenced decode repairs survive matching and later absent walk coverage',()=>{
 const previous=[{range:'0x00000200..0x00000210',owner:'0x00000200'},repair];
 const fresh={range:'0x00000300..0x00000310',owner:'0x00000300'};
 const matched=merge([fresh,{range:repair.range,owner:repair.owner}],previous);
 assert.deepEqual(matched,[fresh,repair]);
 const absent=merge([],matched);
 assert.deepEqual(absent,[repair]);
 assert.deepEqual(merge([],absent),absent);
 assert.deepEqual(previous,[{range:'0x00000200..0x00000210',owner:'0x00000200'},repair]);
});
test('manual decode ownership conflicts and malformed declarations fail',()=>{
 assert.throws(()=>merge([{range:repair.range,owner:'0x000000F0'}],[repair]),/Conflicting decode owner/);
 assert.throws(()=>merge([],[repair,{...repair}]),/Duplicate manual decode range/);
 for(const evidence of ['', '  ', [], null])assert.throws(()=>merge([],[{...repair,evidence}]),/nonempty evidence string/);
 assert.throws(()=>merge([],[{...repair,owner:''}]),/range and owner/);
});
