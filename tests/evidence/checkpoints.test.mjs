import {test} from 'node:test';
import assert from 'node:assert/strict';
import {spawnSync} from 'node:child_process';
import {resolve} from 'node:path';
import {enginePython} from '../../tools/tool-dependencies.mjs';

test('checkpoint probes reject stale identities, unready states and wrapping named reads', () => {
  const code = `
import sys, hashlib, struct
from types import SimpleNamespace as NS
sys.path.insert(0, sys.argv[1])
from regenerate_checkpoints import admit_state, ready, checked_read, main
from verify_checkpoints import compare
from checkpoint_rng import remainder
from startup_window import wait_for_stop
from load_entry import fields, map_ready, load_ready, event_words, navigation_matches, list_markers, returned_frame, load_root, terminated_path, copy_arguments, working_file, settings_request, parent_mapping, saved_load_return, caller_cleanup_jump
from load_entry import named_file
from load_entry import private_span
from load_entry import private_row
import archive_origin as origin
from overlay_returns import ReaderReturn
import map_restore
import object_restore
import script_restore
import item_restore
import getter_metadata
from restore_caller import CallerTrace
from map_writer import MapWriter
from load_entry import native_path_pair
from load_entry import archive_fields, declared_return, reader_frame, region_target, region_iret
import request_provider as provider
from native_save import save_ready, save_root, save_return, input_map_ready, observe_delivery
from dinorefurb_dosbox_session import RUST_DOS_PINNED_SHA256
def rejects(fn):
    try: fn()
    except (ValueError, SystemExit): return
    raise AssertionError('accepted unsupported state')
pool_table=bytearray(1572)
struct.pack_into('<H',pool_table,3*7+1,12)
struct.pack_into('<H',pool_table,3*12+1,9999)
pool_table[3*12]=2
analysis=getter_metadata.pools(pool_table,[7,9999,12])
assert analysis['lists']['56ED']['indices']==[7,12]
assert analysis['lists']['56ED']['terminated']
assert analysis['lists']['56ED']['nonzero_kind_indices']==[12]
assert analysis['shared_indices']==[12]
struct.pack_into('<H',pool_table,3*12+1,7)
assert getter_metadata.pools(pool_table,[7,9999,9999])['lists']['56ED']['cycle_at']==7
assert getter_metadata.pools(pool_table,[524,9999,9999])['lists']['56ED']['invalid_index']==524
assert getter_metadata.pools(pool_table,[9999]*3)['lists']['56ED']['indices']==[]
rejects(lambda:getter_metadata.pools(pool_table[:-1],[9999]*3))
rejects(lambda:getter_metadata.pools(pool_table,[9999,-1,9999]))
class GetterMemory:
    short=False
    def read_memory(self,address,size):
        assert size<=261
        return NS(data=bytes(size-1 if self.short else size))
memory=GetterMemory()
captured=getter_metadata.capture(memory,0)
assert captured['spans']['objects']['size']==1572
assert captured['spans']['field_offsets']['size']==980
assert all(x['equal'] for x in getter_metadata.compare_buffers(captured,bytes(1078)).values())
reference=bytearray(1078); reference[980]=1
compared=getter_metadata.compare_buffers(captured,reference)
assert compared['field_offsets']['equal'] and not compared['field_metadata']['equal']
rejects(lambda:getter_metadata.compare_buffers(captured,bytes(1077)))
rejects(lambda:getter_metadata.compare_buffers({'spans':{'field_offsets':{'size':980,'hex':''}}},bytes(1078)))
assert captured['pool_analysis']['lists']['56ED']['cycle_at']==0
assert getter_metadata.fixed_read(memory,0x1000,0xFFE0,32)==bytes(32)
rejects(lambda:getter_metadata.fixed_read(memory,0x1000,0xFFE0,33))
rejects(lambda:getter_metadata.fixed_read(memory,0xFFFF,0,32))
rejects(lambda:getter_metadata.capture(memory,0xB000))
memory.short=True
rejects(lambda:getter_metadata.capture(memory,0))

table=bytes((i%251 for i in range(9200)))
cleared=item_restore.prediction(table)
assert all(cleared[at:at+2]==bytes(2) for at in range(12,9200,23))
assert all(cleared[i]==table[i] for i in range(9200) if i%23 not in (12,13))
assert item_restore.comparison(cleared,cleared)['equal']
assert not item_restore.comparison(table,cleared)['equal']
rejects(lambda:item_restore.prediction(table[:-1]))
for address,size in [('0000:0000',9200),('FFFF:0010',792),('1000:FFFF',2),('1000:0000',3)]:
    rejects(lambda:item_restore.bounds(address,size))
assert item_restore.bounds('1000:0000',9200)==(65536,74736)
class ItemClient:
    def set_breakpoint(self,*args,**kwargs): pass
    def remove_breakpoints(self,*args): pass
    def read_memory(self,address,size):
        seg,off=(int(x,16) for x in address.split(':'))
        if (seg,off)==(0x57E0,0x19C1): return NS(data=bytes([0,0,0,0x60]))
        if seg==0x6000: return NS(data=table[off:off+size])
        return NS(data=bytes(size))
item=item_restore.ItemTrace(ItemClient(),0,{'ss':'57E0','sp':'8000','cs':'1234'})
rejects(item.finish)
rejects(lambda:item.handle({'reason':'breakpoint','cs_ip':item.clear,'registers':{'ss':'57E0','bp':'7FFC','ds':'57E0'}}))
assert item.handle({'reason':'breakpoint','cs_ip':item.clear,'emu_ns':0,'registers':{'ss':'57E0','bp':'7FFE','ds':'57E0','sp':'7F00','ax':'0000'}})
assert item.expected==cleared and item.report['clear_entry']['nonzero_words']==400
rejects(lambda:item.handle({'reason':'breakpoint','cs_ip':item.entry,'registers':{'ss':'57E0','sp':'7F00','ds':'57E0'}}))
caller=CallerTrace(ItemClient(),0,{'ss':'57E0','bp':'8000','cs':'1234','ax':'0001'})
assert caller.report['reader_al']==1 and not caller.complete
rejects(lambda:caller.close_frame({'ss':'57E0','sp':'7F00','ds':'57E0'}))
rejects(lambda:caller.handle({'reason':'breakpoint','cs_ip':caller.end,'registers':{'ss':'57E0','bp':'7FFF'}}))
rejects(lambda:caller.handle({'reason':'breakpoint','cs_ip':caller.end,'registers':{'ss':'57E0','bp':'8000'}}))
caller.report['post_setup']={}
assert caller.handle({'reason':'breakpoint','cs_ip':caller.end,'emu_ns':0,'registers':{'ss':'57E0','bp':'8000','sp':'7F00','ax':'0001'}})
class MapClient:
    value=0
    binding=struct.pack('<HH',4,0x6000)
    def set_watchpoint(self,address,size): assert address=='6000:126F' and size==1
    def remove_watchpoints(self,address): assert address=='6000:126F'
    def read_memory(self,address,size):
        values={'57E0:0538':self.binding,'6000:126F':bytes([self.value]),
                '57E0:8002':struct.pack('<4H',0x2222,0x4444,107,36)}
        return NS(data=values[address])
mc=MapClient()
mw=MapWriter(mc,0,'6000:0004')
rejects(mw.finish)
mc.value=0x60
ms={'reason':'watchpoint','cs_ip':'25AF:05F9','emu_ns':0,'registers':{
    'ss':'57E0','bp':'8000','sp':'7FFE','ax':'1200','ds':'57E0','es':'6000','bx':'126F','dx':'006B','cx':'0024'}}
assert mw.handle(ms) and mw.report['changes'][-1]['caller']=='4444:2222'
assert mw.finish()['final_value']==0x60
for field,value in [('dx','006A'),('cx','0023'),('bx','126E'),('ds','57DF')]:
    wrong={**ms,'registers':{**ms['registers'],field:value}}
    rejects(lambda:mw.handle(wrong))
rejects(lambda:mw.handle({**ms,'cs_ip':'25AF:05F5'}))
mc.binding=struct.pack('<HH',5,0x6000)
rejects(lambda:mw.handle(ms))
rejects(mw.finish)
assert caller.complete and caller.report['epilogue']['al']==1
caller.close={'ss':0x57E0,'bp':0x7F00}
caller.report['close_epilogue']={}
def consumed_breakpoint(*args): raise AssertionError('deleted consumed one-shot breakpoint')
caller.client.remove_breakpoints=consumed_breakpoint
assert caller.handle({'reason':'breakpoint','cs_ip':caller.end,'emu_ns':0,'registers':{'ss':'57E0','bp':'8000','sp':'7F00','ax':'0001'}})
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
    identity=working_file(run)
    assert private_span(run,{'offset':1,'count':2},identity)==b'bc'
    rejects(lambda: private_span(run,{'offset':2,'count':2},identity))
    rejects(lambda: private_span(run,{'offset':0,'count':3},identity))
    path.write_bytes(b'abd'); rejects(lambda: private_span(run,{'offset':1,'count':2},identity))
    import struct
    path.write_bytes(struct.pack('<III',24,40,12)); identity=working_file(run)
    row={'file_row_offset':0,'number':24,'offset':40,'length':12}
    assert private_row(run,row,identity)
    assert not private_row(run,{**row,'number':26},identity)
    rejects(lambda: private_row(run,{**row,'file_row_offset':1},identity))
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
class OriginMemory:
    options=2; reserve=0x4D4; returned=0x42; io=11; count=30; length=512; row_number=24
    output=0x57E0144E; tag=b'SAVE'
    row_off=0x177
    def read_memory(self,address,size):
        import struct
        if address=='1000:0100' and size==18:
            return NS(data=struct.pack('<HHIIHI',self.returned,0x2000,0x30000020,self.reserve,self.options,self.output))
        if address=='2000:003D': return NS(data=struct.pack('<BHH',0x9A,0x66,0x38FF))
        if address=='1000:00F4': return NS(data=struct.pack('<HH',0x20,0x2000))
        if address=='2000:0024': return NS(data=self.io.to_bytes(2,'little'))
        if address=='2000:0048': return NS(data=(14).to_bytes(4,'little'))
        if address=='2000:003C': return NS(data=struct.pack('<II',1000,self.length))
        if address=='1000:00F8': return NS(data=struct.pack('<HH',0x5B,0x2000))
        if address=='1000:00FC': return NS(data=struct.pack('<HH',self.row_off,0x2000))
        if address=='2000:005B': return NS(data=struct.pack('<4sI',self.tag,self.count))
        if address=='2000:0177': return NS(data=struct.pack('<III',self.row_number,40,12))
        raise AssertionError(address)
def rooted(client,seg,off,bound):
    assert (seg,off,bound)==(0x3000,0x20,82)
    return {'bytes_hex':b'C:\\darkrun.GFF'.hex()}
o=OriginMemory(); open_regs={'ss':'1000','sp':'0100'}
assert origin.opening(o,open_regs,0,rooted)['reserve']==0x4D4
o.options=4; rejects(lambda: origin.opening(o,open_regs,0,rooted)); o.options=2
o.returned=0; rejects(lambda: origin.opening(o,open_regs,0,rooted)); o.returned=0x42
assert origin.opening(o,open_regs,0,lambda *args:{'bytes_hex':b'C:\\other.GFF'.hex()}) is None
original={'ss':0x1000,'bp':0x100,'number':24,'io_handle':11}
regs={'ss':'1000','bp':'0100','ax':'0000'}
archive={'working_handle':14,'active':{'pointer':'2000:0020','handle_field':14},'head':{'pointer':'2000:0020','handle_field':14}}
published=origin.publication(o,regs,original,archive)
rejects(lambda: origin.publication(o,{**regs,'ax':'FFFF'},original,archive))
o.io=12; rejects(lambda: origin.publication(o,regs,original,archive)); o.io=11
selection={**published,'offset':40,'length':12}
assert origin.directory(o,regs,original,selection,published)['row_index']==23
rejects(lambda: origin.directory(o,regs,original,{**selection,'io_handle':12},published))
o.count=0x8000001E; rejects(lambda: origin.directory(o,regs,original,selection,published)); o.count=30
o.row_off=0x178; rejects(lambda: origin.directory(o,regs,original,selection,published)); o.row_off=0x177
o.row_number=26; rejects(lambda: origin.directory(o,regs,original,selection,published)); o.row_number=24
o.options=10; o.output=0x57E0145A
region_path=lambda *args:{'bytes_hex':b'C:\\RGN032.GFF'.hex()}
assert origin.opening(o,open_regs,0,region_path,region=True)['output']==o.output
rejects(lambda: origin.opening(o,open_regs,0,rooted))
assert origin.publication(o,regs,original,{**archive,'region_handle':14,'working_handle':5},region=True)==published
rejects(lambda: origin.publication(o,regs,original,{**archive,'region_handle':5},region=True))
o.tag=b'GMAP'; assert origin.directory(o,regs,original,selection,published,expected_tag=b'GMAP')['number']==24
rejects(lambda: origin.directory(o,regs,original,selection,published)); o.tag=b'SAVE'
o.length=20; rejects(lambda: origin.directory(o,regs,original,selection,published))
base=bytes([0x3F])*12544
first=bytearray(1568); second=bytearray(1568)
first[98//8] |= 1<<(98%8)
second[97//8] |= 1<<(97%8)
result=map_restore.expected_flags(base,first,second)
assert result[1]==0x7F and result[128*97]==0xBF and result[128]==0x3F
assert map_restore.comparison(result,result)['equal']
assert map_restore.comparison(result,base)['different_bytes']==2
assert map_restore.comparison(result,result)['differing_cells']==[]
changed=bytearray(12544)
changed[129]=0x81
assert map_restore.comparison(changed,bytes(12544))['differing_cells']==[{'offset':129,'x':1,'y':1,'actual':129,'expected':0,'xor':129}]
many=map_restore.comparison(bytes([1])*12544,bytes(12544))
assert len(many['differing_cells'])==32 and many['omitted_cells']==12512
assert [c['offset'] for c in many['differing_cells']]==list(range(32))
rejects(lambda:map_restore.comparison(bytes(12543),bytes(12544)))
rejects(lambda: map_restore.expected_flags(base,bytes(1567),second))
assert map_restore.overlaps({'a':('2000:0020',2),'b':('2002:0001',2),'c':('2000:0022',2)})==[['a','b'],['b','c']]
rejects(lambda: map_restore.bounds('2000:FF00',12544))
rejects(lambda: map_restore.bounds('FFFF:0000',1568))
rejects(lambda: map_restore.bounds('0000:0000',2))
class MapRequestMemory:
    slot=0x57E00538; caller=0x1D30; count=0xFFFFFFFF
    def read_memory(self,address,size):
        import struct
        if address=='1000:0100': return NS(data=struct.pack('<HHIIIII',0x4D0,0x38FF,int.from_bytes(b'GMAP','little'),50,0,self.count,self.slot))
        if address=='1000:011A': return NS(data=struct.pack('<HH',self.caller,0x2000))
        if address=='56EF:0010': return NS(data=(0x2000).to_bytes(2,'little'))
        if address=='2000:1D2B': return NS(data=struct.pack('<BHH',0x9A,0x4AB,0x38FF))
        raise AssertionError(address)
mr=MapRequestMemory(); regs={'ss':'1000','sp':'0100'}
assert map_restore.request(mr,regs,0)['kind']=='map'
mr.slot=0; rejects(lambda: map_restore.request(mr,regs,0)); mr.slot=0x57E00538
mr.caller=0; rejects(lambda: map_restore.request(mr,regs,0)); mr.caller=0x1D30
mr.count=12544; rejects(lambda: map_restore.request(mr,regs,0))
class ObjectMemory:
    flags=0; slot=4; ret=0xC3F; count=16; out_slot=0x100000F2
    def __init__(self): self.operations=[]
    def read_memory(self,address,size):
        import struct
        if address=='1000:0100': return NS(data=struct.pack('<7H',self.ret,0x2000,6,5,self.slot,1,self.flags))
        if address=='56EF:0010': return NS(data=(0x2000).to_bytes(2,'little'))
        if address=='2000:0C3A': return NS(data=struct.pack('<BHH',0x9A,0xEFF,0x31E0))
        if address=='1000:0080': return NS(data=struct.pack('<HHIIIII',0x4D0,0x38FF,int.from_bytes(b'OJFF','little'),6,0,0xFFFFFFFF,self.out_slot))
        if address=='1000:009A': return NS(data=struct.pack('<HH',0xF40,0x31E0))
        if address=='1000:00F2': return NS(data=struct.pack('<HH',0xDA,0x1000))
        if address=='1000:00B4': return NS(data=struct.pack('<H',4))
        values={'1000:0062':self.count,'1000:005E':40,'1000:0066':self.count,
                '1000:0072':0x30000020,'1000:006A':0x100000DA,'3000:0048':4,'3000:0024':5,'57E0:9D91':self.count}
        if address in values: return NS(data=values[address].to_bytes(size,'little'))
        seg,off=(int(x,16) for x in address.split(':'))
        if seg==0x57E0 and size==2 and off>=0x67BC and (off-0x67BC)%37==0:
            return NS(data=(5 if (off-0x67BC)//37==4 else 0xFFFF).to_bytes(2,'little'))
        raise AssertionError((address,size))
    def set_breakpoint(self,*args,**kwargs): self.operations.append(('break',args))
    def remove_breakpoints(self,*args): self.operations.append(('unbreak',args))
om=ObjectMemory(); root_regs={'ss':'1000','sp':'0100','bp':'0200','ax':'0000','ds':'57E0'}
of=object_restore.loader_frame(om,root_regs,0); assert of['slot']==4
om.flags=4; rejects(lambda: object_restore.loader_frame(om,root_regs,0)); om.flags=0
om.slot=320; rejects(lambda: object_restore.loader_frame(om,root_regs,0)); om.slot=4
om.ret=0; rejects(lambda: object_restore.loader_frame(om,root_regs,0)); om.ret=0xC3F
core_regs={**root_regs,'sp':'0080','bp':'007E'}
assert object_restore.ojff_frame(om,core_regs,0,of)['bp']==0x7E
om.out_slot=0; rejects(lambda: object_restore.ojff_frame(om,core_regs,0,of)); om.out_slot=0x100000F2
trace=object_restore.ObjectTrace(om,0); trace.capture_before()
def os_stop(at,regs): return {'reason':'breakpoint','cs_ip':at,'registers':regs,'emu_ns':0}
assert trace.handle(os_stop('31E0:0EFF',root_regs))
rejects(lambda: trace.handle(os_stop('31E0:0EFF',root_regs)))
rejects(trace.finish)
assert trace.handle(os_stop('38FF:07B1',core_regs))
om.count=17; rejects(lambda: trace.handle(os_stop('38FF:094A',core_regs))); om.count=16
assert trace.handle(os_stop('38FF:094A',core_regs))
assert trace.handle(os_stop('38FF:09D4',core_regs))
loader_regs={**root_regs,'bp':'00FE','sp':'00B0'}
assert trace.handle(os_stop('31E0:0F40',loader_regs))
assert trace.handle(os_stop('2D40:0B54',loader_regs))
rejects(lambda: trace.handle(os_stop('31E0:1288',{**loader_regs,'bp':'00FC'})))
assert trace.handle(os_stop('31E0:1288',{**loader_regs,'ax':'0004'}))
finished=trace.finish()
assert finished['census_matches_calls'] and finished['slot_319_entry_at_return']==0xFFFF
assert finished['child_counts']=={'script-chain':1}
assert finished['objects'][0]['selected']['count']==16
import struct
table=b''.join(struct.pack('<3H',0 if i<2 else i,10+i,7+i%2) for i in range(1316))
trigger=bytearray(2600)
struct.pack_into('<2H',trigger,0,0,5)
struct.pack_into('<2H',trigger,13,2,0)
struct.pack_into('<2H',trigger,26,65535,7)
nodes=[{'slot':3,'pairs':[0,2,9,9],'next':1},{'slot':1,'pairs':[1,65535,9,9],'next':-1}]
en,et,stats=script_restore.prediction(table,nodes,trigger)
assert en[0]['pairs']==[10,12,7,7] and en[1]==nodes[1]
assert struct.unpack_from('<2H',et,0)==(10,7) and et[13:]==trigger[13:]
assert stats['pairs_matched']==2 and stats['pairs_unmatched']==2 and stats['triggers_matched']==1
before={'head':3,'trigger_pointer':'2000:0020','nodes':nodes}
after={**before,'nodes':en}
assert script_restore.comparison(after,et,before,en,et)['chain_equal']
assert not script_restore.comparison(before,trigger,before,en,et)['trigger_equal']
rejects(lambda: script_restore.prediction(table[:-1],nodes,trigger))
rejects(lambda: script_restore.address_range('2000:FF00',2600))
class ConversionMemory:
    head=3; link=1
    def read_memory(self,address,size):
        seg,off=(int(x,16) for x in address.split(':'))
        if address=='57E0:5AF5': return NS(data=self.head.to_bytes(2,'little'))
        if address=='57E0:40C0': return NS(data=struct.pack('<HH',0x20,0x2000))
        for node in nodes:
            if seg==0x4F49 and off==0x8AB+19*node['slot']: return NS(data=struct.pack('<4H',*node['pairs']))
            if seg==0x4F49 and off==0x8B5+19*node['slot']: return NS(data=bytes([self.link if node['slot']==3 else 255]))
        if seg==0x2000: return NS(data=bytes(trigger[off-0x20:off-0x20+size]))
        raise AssertionError((address,size))
cm=ConversionMemory(); captured,raw=script_restore.snapshot(cm,0)
assert captured==before and raw==trigger
cm.head=48; rejects(lambda: script_restore.snapshot(cm,0)); cm.head=3
cm.link=3; rejects(lambda: script_restore.snapshot(cm,0)); cm.link=254; rejects(lambda: script_restore.snapshot(cm,0))
class ConversionFrames:
    size=7896; data_slot=0x57E0023C
    def __init__(self): self.operations=[]
    def read_memory(self,address,size):
        if address=='57E0:0200': return NS(data=struct.pack('<HHIII',0x184C,0x2000,int.from_bytes(b'GPLI','little'),1,0x57E00238))
        if address=='56EF:0010': return NS(data=(0x2000).to_bytes(2,'little'))
        if address=='57E0:0242': return NS(data=struct.pack('<HH',0xCC0,0x2000))
        if address=='57E0:0246': return NS(data=bytes([1]))
        if address=='57E0:023C': return NS(data=bytes(4))
        if address=='2000:1847': return NS(data=struct.pack('<BHH',0x9A,0x5B5,0x38FF))
        if address=='57E0:01E2': return NS(data=struct.pack('<HH',0x20,0x3000))
        if address=='57E0:01DE': return NS(data=struct.pack('<HH',0x80,0x3000))
        if address=='3000:0080': return NS(data=struct.pack('<4sI',b'GPLI',0x80000001))
        if address=='3000:0048': return NS(data=(7).to_bytes(4,'little'))
        if address=='3000:0024': return NS(data=(4).to_bytes(2,'little'))
        if address=='57E0:0238': return NS(data=self.size.to_bytes(4,'little'))
        if address=='57E0:0180': return NS(data=struct.pack('<HHIIIII',0x4D0,0x38FF,int.from_bytes(b'GPLI','little'),1,0,0xFFFFFFFF,self.data_slot))
        if address=='57E0:019A': return NS(data=struct.pack('<HH',0x1879,0x2000))
        raise AssertionError((address,size))
    def set_breakpoint(self,*args,**kwargs): self.operations.append(('break',args))
    def remove_breakpoints(self,*args): self.operations.append(('unbreak',args))
cf=ConversionFrames(); st=script_restore.ScriptTrace(cf,0)
qregs={'ss':'57E0','ds':'57E0','sp':'0200','bp':'0240','ax':'0000'}
rejects(lambda: st.handle(os_stop('38FF:05B5',{**qregs,'ds':'1000'})))
assert st.handle(os_stop('38FF:05B5',qregs))
sregs={**qregs,'bp':'01E8','es':'57E0','bx':'0238','eax':'00001ED8'}
rejects(lambda: st.handle(os_stop('38FF:0548',{**sregs,'eax':'00000001'})))
assert st.handle(os_stop('38FF:0548',sregs)) and st.report['size_selected']['indexed']
cf.size=7895; rejects(lambda: st.handle(os_stop('38FF:0613',{**qregs,'bp':'01FE'}))); cf.size=7896
assert st.handle(os_stop('38FF:0613',{**qregs,'bp':'01FE'}))
cf.data_slot=0; rejects(lambda: st.handle(os_stop('38FF:07B1',{**qregs,'sp':'0180'}))); cf.data_slot=0x57E0023C
assert st.handle(os_stop('38FF:07B1',{**qregs,'sp':'0180'}))
rejects(lambda: st.handle(os_stop('444C:0092',{**qregs,'bp':'023E'})))
rejects(st.finish)
class ReaderMemory:
    offset=0xF2F; segment=0x2000; saved=0xF2F; loaded=0x2000; code=bytes([0x59,0x88,0x46,0xFE])
    def __init__(self): self.operations=[]
    def read_memory(self,address,size):
        import struct
        if address=='1000:0020': return NS(data=struct.pack('<HH',self.offset,self.segment))
        if address=='56EF:0002': return NS(data=self.saved.to_bytes(2,'little'))
        if address=='56EF:0010': return NS(data=self.loaded.to_bytes(2,'little'))
        if address=='56EF:0000': return NS(data=bytes([0xCD,0x3F]))
        if address=='4AE5:0559': return NS(data=bytes([0xCF]))
        if address=='1000:001E': return NS(data=struct.pack('<HHH',0xF2F,self.loaded,0))
        if address=='2000:0F2F': return NS(data=self.code)
        raise AssertionError(address)
    def set_watchpoint(self,*args,**kwargs): self.operations.append(('watch',args))
    def remove_watchpoints(self,*args): self.operations.append(('unwatch',args))
    def set_breakpoint(self,*args,**kwargs): self.operations.append(('break',args))
    def remove_breakpoints(self,*args): self.operations.append(('unbreak',args))
def reader_stop(address,sp='0024',reason='breakpoint'):
    return {'cs_ip':address,'reason':reason,'registers':{'ss':'1000','sp':sp}}
m=ReaderMemory(); original={'ss':'1000','sp':'0020'}
rr=ReaderReturn(m,original,0)
assert rr.handle(reader_stop('2000:0F2F'))=='returned'
assert rr.watch is None
m=ReaderMemory(); rr=ReaderReturn(m,original,0)
rejects(lambda: rr.handle(reader_stop('2000:0F2F','0022')))
m.code=bytes(4); rejects(lambda: rr.handle(reader_stop('2000:0F2F')))
m=ReaderMemory(); rr=ReaderReturn(m,original,0)
m.offset=0; m.segment=0x56EF
assert rr.handle(reader_stop('3000:0000','0010','watchpoint'))=='continued'
assert rr.handle(reader_stop('56EF:0000'))=='continued'
assert rr.watch is None and rr.awaiting_iret
assert rr.handle(reader_stop('4AE5:0559','001C'))=='continued'
assert rr.handle(reader_stop('4AE5:0559','001E'))=='continued'
assert rr.handle(reader_stop('2000:0F2F'))=='returned'
assert [e['kind'] for e in rr.events]==['relocation','trap','nested-iret','reload']
rejects(lambda: region_target(m,original,0,0x1234))
rejects(lambda: region_iret(m,{'ss':'1000','sp':'001E'},original,0,0x1234))
class PayloadMemory:
    short=False
    def read_memory(self,address,size):
        assert 0<size<=261
        return NS(data=b'x'*(size-1 if self.short else size))
memory=PayloadMemory(); selection={'count':1920,'destination':'2000:0020'}
assert provider.payload(memory,selection,24,b'x'*1920)['equal']
assert not provider.payload(memory,selection,24,b'y'*1920)['equal']
assert provider.payload(memory,{'count':2,'destination':'2000:0020'},26,b'xx')['equal']
rejects(lambda: provider.payload(memory,selection,26,b'x'*1920))
rejects(lambda: provider.payload(memory,selection,25,b'x'*1920))
rejects(lambda: provider.payload(memory,{**selection,'destination':'2000:FF00'},24,b'x'*1920))
rejects(lambda: provider.payload(memory,{**selection,'destination':'FFFF:0000'},24,b'x'*1920))
memory.short=True; rejects(lambda: provider.payload(memory,selection,24,b'x'*1920))
rejects(lambda: map_restore.read(memory,'2000:0020',12544))
memory.short=False; assert map_restore.read(memory,'2000:0020',12544)==b'x'*12544
rejects(lambda: named_file(Path('.'),'RGN033.GFF'))
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
  const result = spawnSync(enginePython(), ['-I', '-',
    resolve('tools/research/rust-dos-runs')], {encoding: 'utf8', input: code});
  assert.equal(result.status, 0, result.stderr || result.stdout);
});
