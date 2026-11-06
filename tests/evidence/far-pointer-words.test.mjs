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

test('overlay near patterns preserve instruction bounds, signed displacements and word IP wrap',()=>{
  const tool=fileURLToPath(new URL('../../tools/research/exec-census/overlay_near_patterns.py',import.meta.url));
  const script=`
import runpy, sys
api = runpy.run_path(sys.argv[1])
scan, control = api['relative_patterns'], api['check_control']
cases = [
 ('e8 02 00', 5, 'call16'), ('e9 02 00', 5, 'jump16'),
 ('eb 03', 5, 'jump8'), ('75 03', 5, 'condition8'),
 ('e0 03', 5, 'loop8'), ('e1 03', 5, 'loop8'),
 ('e2 03', 5, 'loop8'), ('e3 03', 5, 'loop8'),
 ('0f 85 01 00', 5, 'condition16'),
 ('e8 fd ff', 0, 'call16'), ('eb fe', 0, 'jump8'),
 ('0f 85 fc ff', 0, 'condition16')]
for encoded, target, kind in cases:
 data = bytes.fromhex(encoded)
 rows = scan(data, 0, len(data), 0, target)
 assert rows == [{'site':0, 'size':len(data), 'kind':kind}], (encoded, rows)
 for end in range(1, len(data)):
  assert scan(data, 0, end, 0, target) == [], (encoded, end)
data = bytes.fromhex('e8 00 00')
assert scan(data, 0, 3, -0xfffe, 1)[0]['site'] == 0
assert scan(data, 0, 3, -0xfffe, 0xffff) == []
# A byte pattern inside an immediate remains a candidate, and cannot pass
# a positive control unless the independently supplied ownership marks it.
data = bytes.fromhex('b8 eb fe')
rows = scan(data, 0, 3, 0, 1)
assert rows == [{'site':1, 'size':2, 'kind':'jump8'}]
control(rows, {1}, 1)
for starts, site in [({0},1), ({1},0), ({1},2)]:
 try: control(rows, starts, site)
 except ValueError: pass
 else: raise AssertionError('missing/inside control accepted')
for bounds in [(-1,3,1), (0,4,1), (2,1,1), (0,3,0x10000)]:
 try: scan(data, bounds[0], bounds[1], 0, bounds[2])
 except ValueError: pass
 else: raise AssertionError('invalid bounds accepted')
assert scan(data, 0, 0, 0, 1) == []
print('overlay controls passed')
`;
  const result=spawnSync(enginePython(),['-I','-c',script,tool],{encoding:'utf8',timeout:30000,maxBuffer:1024*1024});
  if(result.error)throw result.error;
  assert.equal(result.status,0,result.stderr);
  assert.equal(result.stdout.trim(),'overlay controls passed');
});
