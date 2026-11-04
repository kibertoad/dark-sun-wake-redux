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
from load_entry import fields, map_ready, load_ready, event_words, navigation_matches, list_markers, returned_frame, load_root
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
class ShortMemory:
    def read_memory(self,address,size): return NS(data=bytes(max(0,size-1)))
rejects(lambda: fields(ShortMemory(),0))
ui=fields(Memory(),0)
assert ui['count']==0 and ui['event_window'] is None and ui['selected_row']==0
class TargetMemory(Memory):
    tag=b'WIND'
    def read_memory(self,address,size):
        if address=='57E0:A0FD': return NS(data=bytes([0,0,0,16]))
        if address=='1000:0000':
            body=bytearray(size); body[:4]=self.tag
            body[8:12]=(123).to_bytes(4,'little')
            body[0xF5:0xF9]=bytes([7,0,0,32])
            return NS(data=bytes(body))
        if address=='4C4C:0000': return NS(data=(65535).to_bytes(2,'little'))
        if address=='4C4C:0002': return NS(data=(29).to_bytes(2,'little'))
        if address=='50AF:0004':
            assert size==1
            return NS(data=bytes([1]))
        return super().read_memory(address,size)
mem=TargetMemory(); ui=fields(mem,0)
assert ui['selected_row']==65535 and ui['list_top']==29 and ui['load_mode']==1
assert ui['event_window']=={'number':123,'callback':'2000:0007'}
mem.tag=b'FAIL'; rejects(lambda: fields(mem,0))
mapped={'window':{'number':19501,'flags':8},'mouse_enabled':1,
        'mode':1,'leader':1,'slot_kinds':[0,2,0,2]}
assert map_ready(mapped)
assert not map_ready({**mapped,'leader':0})
assert not map_ready({**mapped,'mode':0})
loaded={**mapped,'window':{'number':18500,'flags':8}}
assert load_ready(loaded,{'load_mode':1})
assert not load_ready(loaded,{'load_mode':0})
assert not load_ready(mapped,{'load_mode':1})
class FrameMemory:
    def read_memory(self,address,size):
        assert size==24
        off=int(address.split(':')[1],16)
        return NS(data=b''.join((off+i).to_bytes(2,'little') for i in range(12)))
registers={'ss':'1000','sp':'0020'}
assert event_words(FrameMemory(),registers,0)[0]==32
assert event_words(FrameMemory(),registers,4)[0]==36
rejects(lambda: event_words(FrameMemory(),registers,2))
rejects(lambda: event_words(ShortMemory(),registers,4))
rejects(lambda: event_words(FrameMemory(),{'ss':'1000','sp':'FFF0'},4))
event=[0]*12; event[0]=6; event[6]=0x5000
call={'event_words':event.copy(),'fields':{'global_handler':'synthetic callback'}}
assert navigation_matches(call,event,'return','return','synthetic callback')
assert not navigation_matches(call,event,'other','return','synthetic callback')
assert not navigation_matches(call,event,'return','return','other callback')
assert not navigation_matches(None,event,'return','return','synthetic callback')
wrong=event.copy(); wrong[6]=0x4800
assert not navigation_matches(call,wrong,'return','return','synthetic callback')
class Markers:
    def read_memory(self,address,size):
        off=int(address.split(':')[1],16)
        assert size==1 and off>=4 and (off-4)%125==0
        return NS(data=bytes([(off-4)//125]))
ui={'list_top':0,'selected_row':1,'list_pointer':'1000:0004'}
assert list_markers(Markers(),ui)==list(range(10))
for override in [{'list_top':10},{'selected_row':65535},{'list_pointer':'0000:0000'},
                 {'list_pointer':'1000:FFF0'},{'list_pointer':'FFFF:0004'}]:
    rejects(lambda: list_markers(Memory(),{**ui,**override}))
rejects(lambda: list_markers(ShortMemory(),ui))
assert returned_frame({'ss':'1000','sp':'0020'},{'ss':'1000','sp':'0024'})
assert not returned_frame({'ss':'1000','sp':'0020'},{'ss':'1001','sp':'0024'})
assert not returned_frame({'ss':'1000','sp':'0020'},{'ss':'1000','sp':'0010'})
assert not returned_frame({'ss':'1000','sp':'FFFE'},{'ss':'1000','sp':'0002'})
class LoadCode:
    jump=bytes([0xEA,0x85,7,0,32])
    call=bytes([0xE8,0x0B,0xFA])
    def read_memory(self,address,size):
        data=self.call if address=='2000:0BDF' else self.jump
        assert len(data)==size
        return NS(data=data)
code=LoadCode()
assert load_root(code,'1000:002F')==('2000:05ED','2000:0BE2')
code.jump=bytes([0x90,0x85,7,0,32]); rejects(lambda: load_root(code,'1000:002F'))
code.jump=bytes([0xEA,0x86,7,0,32]); rejects(lambda: load_root(code,'1000:002F'))
code.jump=LoadCode.jump; code.call=bytes([0xE8,0x0C,0xFA])
rejects(lambda: load_root(code,'1000:002F'))
enter=event.copy(); enter[6]=0x1C0D
enter_call={**call,'event_words':enter.copy()}
assert navigation_matches(enter_call,enter,'return','return','synthetic callback',0x1C0D)
assert not navigation_matches(enter_call,enter,'return','return','synthetic callback')
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
