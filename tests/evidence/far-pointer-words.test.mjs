import {test} from 'node:test';
import assert from 'node:assert/strict';
import {fileURLToPath} from 'node:url';
import {spawnSync} from 'node:child_process';
import {enginePython} from '../../tools/tool-dependencies.mjs';

test('offset-word candidates use typed encoded immediates and import without a source',()=>{
  const tool=fileURLToPath(new URL('../../tools/research/exec-census/far_pointer_words.py',import.meta.url));
  const fixtures=fileURLToPath(new URL('../fixtures/synthetic/offset-word-immediates.json',import.meta.url));
  const script=`
import json, runpy, sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_16
classify = runpy.run_path(sys.argv[1])['is_immediate_word']
decoder = Cs(CS_ARCH_X86, CS_MODE_16)
decoder.detail = True
with open(sys.argv[2], encoding='utf-8') as source:
    cases = json.load(source)
checks = 0
for address in (0, 0x12340):
    for case in cases:
        data = bytes.fromhex(case['hex'])
        instruction = next(decoder.disasm(data, address))
        assert instruction.size == len(data), case['name']
        for offset, value, expected in case['words']:
            actual = classify(instruction, address + offset, value)
            assert actual == expected, (case['name'], address, offset, actual, expected)
            checks += 1
print(checks)
`;
  const result=spawnSync(enginePython(),['-I','-c',script,tool,fixtures],{encoding:'utf8',timeout:30000,maxBuffer:1024*1024});
  if(result.error)throw result.error;
  assert.equal(result.status,0,result.stderr);
  assert.equal(result.stdout.trim(),'34');
});
