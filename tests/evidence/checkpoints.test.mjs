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
from load_entry import fields, map_ready, load_ready, event_words, navigation_matches, list_markers, returned_frame, load_root, terminated_path, copy_arguments, working_file, settings_request, parent_mapping, saved_load_return, caller_cleanup_jump
from load_entry import named_file
from load_entry import native_path_pair
from load_entry import archive_fields, declared_return, reader_frame, region_target, region_iret
import request_provider as provider
from native_save import save_ready, save_root, save_return, input_map_ready, observe_delivery
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
class PathMemory:
    data=b'abc'+bytes([0])
    calls=0
    def read_memory(self,address,size):
        assert size==1
        self.calls+=1
        index=int(address.split(':')[1],16)-4
        return NS(data=self.data[index:index+1])
p=PathMemory(); assert terminated_path(p,0x1000,4,80)=={'length':3,'bytes_hex':'616263'}
assert p.calls==4
p=PathMemory(); p.data=bytes([0]); assert terminated_path(p,0x1000,4,80)['length']==0
assert p.calls==1
p=PathMemory(); p.data=b'x'*82
rejects(lambda: terminated_path(p,0x1000,4,80))
rejects(lambda: terminated_path(Memory(),0,0,80))
rejects(lambda: terminated_path(Memory(),0x1000,4,100))
rejects(lambda: terminated_path(ShortMemory(),0x1000,4,82))
class NonzeroMemory:
    def read_memory(self,address,size): return NS(data=b'x')
rejects(lambda: terminated_path(NonzeroMemory(),0x1000,0xFFFF,80))
args=[1,2,3,4,5,6,0,0,0,0]
assert copy_arguments(__import__('struct').pack('<10H',*args))==tuple(args[:6])
rejects(lambda: copy_arguments(bytes(19)))
for index in range(6,10):
    wrong=args.copy(); wrong[index]=1
    rejects(lambda: copy_arguments(__import__('struct').pack('<10H',*wrong)))
from tempfile import TemporaryDirectory
from pathlib import Path
with TemporaryDirectory() as temp:
    run=Path(temp); (run/'drive-c').mkdir()
    assert working_file(run)=={'exists':False}
    path=run/'drive-c'/'darkrun.GFF'; path.write_bytes(b'abc')
    assert working_file(run)=={'exists':True,'length':3,'sha256':hashlib.sha256(b'abc').hexdigest()}
    path.unlink(); path.mkdir(); rejects(lambda: working_file(run))
class Linked:
    def __truediv__(self,other): return self
    def is_symlink(self): return True
    def is_junction(self): return False
rejects(lambda: working_file(Linked()))
class Oversized(Linked):
    def is_symlink(self): return False
    def exists(self): return True
    def is_file(self): return True
    def stat(self): return NS(st_size=128*1024*1024+1)
rejects(lambda: working_file(Oversized()))
class SettingsMemory:
    tag=0x46455250
    number=100
    def read_memory(self,address,size):
        assert size==12
        return NS(data=__import__('struct').pack('<IIHH',self.tag,self.number,4,0x1000))
s=SettingsMemory()
assert settings_request(s,registers,0x66F)=={'tag':'PREF','id':100,'output':'1000:0004'}
s.number=101; rejects(lambda: settings_request(s,registers,0x66F))
s.tag=0x51455247; s.number=2
assert settings_request(s,registers,0x743)['id']==2
rejects(lambda: settings_request(s,registers,0x66F))
rejects(lambda: settings_request(s,registers,0))
rejects(lambda: settings_request(ShortMemory(),registers,0x743))
class ParentMemory:
    segment=0x2000
    child_offset=0x659
    code=bytes([0x83,0xC4,4,0x9A,0x52,0,0xE9,0x56])
    def read_memory(self,address,size):
        if address=='1000:0102': return NS(data=__import__('struct').pack('<HH',self.child_offset,0x2000))
        if address=='1000:0200': return NS(data=__import__('struct').pack('<HH',0xBE2,self.segment))
        assert address=='2000:0659' and size==8
        return NS(data=self.code)
copy_regs={'ss':'1000','bp':'0100'}; load_regs={'ss':'1000','sp':'0200'}
p=ParentMemory(); assert parent_mapping(p,0,copy_regs,load_regs)==('2000','2000:0BE2')
p.child_offset=0x660; rejects(lambda: parent_mapping(p,0,copy_regs,load_regs)); p.child_offset=0x659
p.segment=0x3000; rejects(lambda: parent_mapping(p,0,copy_regs,load_regs))
p.segment=0x2000; p.code=bytes(8); rejects(lambda: parent_mapping(p,0,copy_regs,load_regs))
p=ParentMemory(); assert saved_load_return(p,load_regs)=='2000:0BE2'
p.segment=0x3000; assert saved_load_return(p,load_regs)=='3000:0BE2'
rejects(lambda: saved_load_return(ShortMemory(),load_regs))
rejects(lambda: saved_load_return(Memory(),load_regs))
assert caller_cleanup_jump(bytes([0xEB,0x0E]))
assert not caller_cleanup_jump(bytes([0xEB,0x0D]))
assert not caller_cleanup_jump(bytes([0x83,0xC4,2]))
assert not caller_cleanup_jump(bytes([0xEB]))
assert save_ready(loaded,{'load_mode':0,'selected_row':1,'list_top':0,'exit_byte':0})
for wrong in [{'load_mode':1},{'selected_row':0},{'list_top':10},{'exit_byte':1}]:
    assert not save_ready(loaded,{'load_mode':0,'selected_row':1,'list_top':0,'exit_byte':0,**wrong})
class SaveCode(LoadCode):
    call=__import__('struct').pack('<BH',0xE8,(0x3FB-0xBF2)&65535)
    def read_memory(self,address,size):
        return NS(data=self.call if address=='2000:0BEF' else self.jump)
s=SaveCode(); assert save_root(s,'1000:002F')=='2000:03FB'
s.call=bytes(3); rejects(lambda: save_root(s,'1000:002F'))
class SaveFrame:
    row=1
    offset=0xBF2
    def read_memory(self,address,size):
        assert size==6
        return NS(data=__import__('struct').pack('<HHH',self.offset,0x2000,self.row))
s=SaveFrame(); assert save_return(s,registers)=='2000:0BF2'
s.row=0; rejects(lambda: save_return(s,registers)); s.row=1
s.offset=0xBE2; rejects(lambda: save_return(s,registers))
rejects(lambda: named_file(Path('.'),'../SAVE02.SAV'))
class MapHead:
    tag=b'WIND'
    number=19501
    def read_memory(self,address,size):
        assert size==160
        body=bytearray(size); body[:4]=self.tag
        body[8:12]=self.number.to_bytes(4,'little'); body[158:160]=(8).to_bytes(2,'little')
        return NS(data=bytes(body))
postsave={**mapped,'window':None}
ui={'count':1,'exit_byte':0,'global_handler':'28C9:0CFF','event_target':'0000:0000',
    'save_load_window':'0000:0000','list_pointer':'0000:0000','head':'2000:0004'}
h=MapHead(); assert input_map_ready(h,0,postsave,ui)
assert not input_map_ready(h,0,postsave,{**ui,'count':0})
assert not input_map_ready(h,0,{**postsave,'mode':2},ui)
assert not input_map_ready(h,0,postsave,{**ui,'head':'0000:0000'})
h.number=18500; assert not input_map_ready(h,0,postsave,ui)
h.tag=b'FAIL'; rejects(lambda: input_map_ready(h,0,postsave,ui))
class Delivery:
    calls=0
    def observe(self,operation,timeout):
        assert operation is key_handle and timeout==0
        self.calls+=1
        assert self.calls<=2
        return NS(pending=self.calls==1)
key_handle=object(); session=Delivery()
pending=observe_delivery(session,key_handle,None)
done=observe_delivery(session,key_handle,pending)
assert observe_delivery(session,key_handle,done) is done and session.calls==2
def path_value(value): return {'bytes_hex':value.encode('ascii').hex()}
assert native_path_pair(path_value('C:\\SAVE02.SAV'),path_value('C:\\darkrun.GFF'))
assert not native_path_pair(path_value('C:\\SAVE03.SAV'),path_value('C:\\darkrun.GFF'))
assert not native_path_pair(path_value('C:\\SAVE02.SAV'),path_value('D:\\darkrun.GFF'))
assert not native_path_pair(path_value(''),path_value('C:\\darkrun.GFF'))
assert archive_fields(Memory(),0)['active']['handle_field'] is None
class ArchiveMemory(Memory):
    pointer=bytes([0x20,0,0,16])
    def read_memory(self,address,size):
        if address=='57E0:9D9F': return NS(data=self.pointer)
        if address=='1000:0048': return NS(data=(5).to_bytes(4,'little'))
        return super().read_memory(address,size)
a=ArchiveMemory(); assert archive_fields(a,0)['active']['handle_field']==5
a.pointer=bytes([0xE0,0xFF,0,16]); rejects(lambda: archive_fields(a,0))
rejects(lambda: archive_fields(ShortMemory(),0))
class RegionFrame:
    offset=0xF26
    def read_memory(self,address,size):
        assert size==4
        return NS(data=__import__('struct').pack('<HH',self.offset,0x2000))
r=RegionFrame(); assert declared_return(r,registers,0xF26)=='2000:0F26'
r.offset=0xF27; rejects(lambda: declared_return(r,registers,0xF26))
r=SaveFrame(); r.offset=0xF2F; r.row=0
assert reader_frame(r,registers,0x2000)==0
r.row=1; rejects(lambda: reader_frame(r,registers,0x2000)); r.row=0
rejects(lambda: reader_frame(r,registers,0x3000))
class TrapMemory:
    offset=0xF26
    segment=0x2000
    saved=0xF26
    loaded=0x2000
    def read_memory(self,address,size):
        if address=='56EF:0002': return NS(data=self.saved.to_bytes(2,'little'))
        if address=='56EF:0010': return NS(data=self.loaded.to_bytes(2,'little'))
        if size==6: return NS(data=__import__('struct').pack('<HHH',self.offset,self.segment,0x202))
        assert size==4
        return NS(data=__import__('struct').pack('<HH',self.offset,self.segment))
t=TrapMemory(); assert region_target(t,registers,0)==('2000:0F26',False)
t.segment=0x56EF; assert region_target(t,registers,0)==('56EF:0F26',True)
t.offset=0; assert region_target(t,registers,0)==('56EF:0F26',True)
t.segment=0x2000; assert region_target(t,registers,0)==('2000:0F26',False)
t.saved=1; rejects(lambda: region_target(t,registers,0)); t.saved=0xF26
t.segment=0x3000; rejects(lambda: region_target(t,registers,0))
t.segment=0x2000; t.offset=0xF26
assert region_iret(t,{'ss':'1000','sp':'001E'},registers,0)=='2000:0F26'
assert region_iret(t,{'ss':'1000','sp':'001C'},registers,0) is None
t.offset=0; rejects(lambda: region_iret(t,{'ss':'1000','sp':'001E'},registers,0))
class ProviderMemory:
    returned=0x4D0; number=24; count=0xFFFFFFFF; start=0; slot=0x20000020
    length=12; offset=40; pointer=0x20000020
    def read_memory(self,address,size):
        import struct
        if address=='1000:0100':
            return NS(data=struct.pack('<HHIIIII',self.returned,0x3000,int.from_bytes(b'SAVE','little'),self.number,self.start,self.count,self.slot))
        values={'1000:00E2':self.length,'1000:00DE':self.offset,'1000:00E6':self.length,
                '1000:00F2':self.pointer,'1000:00EA':0x20000080,'2000:0048':7,'2000:0024':3}
        return NS(data=values[address].to_bytes(size,'little'))
p=ProviderMemory(); regs={'ss':'1000','sp':'0100','bp':'00FE'}
entry=provider.request(p,regs,0x3000)
assert entry['number']==24 and provider.selected(p,regs,entry)['provider_handle']==7
rejects(lambda: provider.selected(p,{**regs,'bp':'00FC'},entry))
p.returned=0; rejects(lambda: provider.request(p,regs,0x3000)); p.returned=0x4D0
p.count=12; rejects(lambda: provider.request(p,regs,0x3000)); p.count=0xFFFFFFFF
p.start=1; rejects(lambda: provider.request(p,regs,0x3000)); p.start=0
p.slot=0; rejects(lambda: provider.request(p,regs,0x3000)); p.slot=0x20000020
p.number=25; assert provider.request(p,regs,0x3000) is None
p.number=26; assert provider.request(p,regs,0x3000)['number']==26
p.length=0; rejects(lambda: provider.selected(p,regs,entry)); p.length=12
p.offset=0x80000000; rejects(lambda: provider.selected(p,regs,entry)); p.offset=40
p.pointer=0x2000FFE0; rejects(lambda: provider.selected(p,regs,entry))
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
