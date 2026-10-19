import {test} from 'node:test';
import assert from 'node:assert/strict';
import {spawnSync} from 'node:child_process';
import {resolve} from 'node:path';
import {enginePython} from '../../tools/tool-dependencies.mjs';

test('checkpoint probes reject stale identities, unready states and wrapping named reads', () => {
  const code = `
import sys, hashlib
from types import SimpleNamespace as NS
sys.path.insert(0, sys.argv[1])
from regenerate_checkpoints import admit_state, ready, checked_read, main
from verify_checkpoints import compare
from checkpoint_rng import remainder
from startup_window import wait_for_stop
from dinorefurb_dosbox_session import RUST_DOS_PINNED_SHA256
def rejects(fn):
    try: fn()
    except (ValueError, SystemExit): return
    raise AssertionError('accepted unsupported state')
admit_state(NS(emulator_sha256=RUST_DOS_PINNED_SHA256, stop={}))
rejects(lambda: admit_state(NS(emulator_sha256='0'*64, stop={})))
rejects(lambda: admit_state(NS(emulator_sha256=RUST_DOS_PINNED_SHA256, stop=None)))
assert ready({'window':{'number':19500,'flags':8200},'mouse_enabled':1},19500)
assert not ready({'window':{'number':19500,'flags':8194},'mouse_enabled':1},19500)
assert not ready({'window':None,'mouse_enabled':1},19500)
assert not ready({'window':{'number':19500,'flags':8200},'mouse_enabled':0},19500)
class Memory:
    def read_memory(self,address,size): return NS(data=bytes(size))
assert checked_read(Memory(),0x1000,0x10,2) == bytes(2)
for seg,off,size in [(0x10000,0,1),(0xFFFF,0x10,1),(0,0xFFFF,2),(0,0,262),(0,0,0)]:
    rejects(lambda: checked_read(Memory(),seg,off,size))
png=b'synthetic frame'
expected={'state':{'mode':0},'frame_sha256':hashlib.sha256(png).hexdigest()}
compare(expected,{'mode':0},png)
rejects(lambda: compare(expected,{'mode':1},png))
rejects(lambda: compare(expected,{'mode':0},b'changed frame'))
assert remainder(32767,10)==7 and remainder(32767,-10)==7
rejects(lambda: remainder(1,0))
operation=NS(op=17)
class Pending:
    calls=0
    def observe(self, op, timeout):
        assert op is operation
        self.calls+=1
        return NS(pending=self.calls<3)
p=Pending()
_,exhausted=wait_for_stop(p,operation)
assert p.calls==3 and not exhausted
sys.argv=['probe','synthetic','unsupported']
rejects(main)
`;
  const result = spawnSync(enginePython(), ['-I', '-c', code,
    resolve('tools/research/rust-dos-runs')], {encoding: 'utf8'});
  assert.equal(result.status, 0, result.stderr || result.stdout);
});
