import {test} from 'node:test';
import assert from 'node:assert/strict';
import {spawnSync} from 'node:child_process';
import {resolve} from 'node:path';
import {enginePython} from '../../tools/tool-dependencies.mjs';

test('post-label endpoint admits only disjoint header-verified printable prefixes and retains late records', () => {
  check(`
import sys, struct
from types import SimpleNamespace as NS
sys.path.insert(0,sys.argv[1])
from setup_parser import observe_labels, observe_fields, expected_labels, address, PHASES
def reject(fn):
    try:fn()
    except ValueError:return
    raise AssertionError('unsupported label observation admitted')
loaded=0x1000; ds=0x1E36
counts={'main':2,'digital':3,'music_only':-1}
names={'main':[b'A'*39,b'B'*39],'digital':[b'C',b'D',b'E'],'music_only':[]}
expected={k:b''.join(n+b'\\0'*(254-len(n)) for n in rows) for k,rows in names.items()}
projection={'counts':counts,'payloads':expected}
stop={'reason':'breakpoint','cs_ip':address(loaded,*PHASES['labels']),
      'registers':{'ds':f'{ds:04X}','ss':'3000','sp':'0100','ax':'0000'}}
class Memory:
    def __init__(self):
        self.groups={'main':0x4000,'digital':0x4100,'music_only':0x4200}
        self.labels={'main':0x5200,'digital':0x5300,'music_only':0x5400}
        self.label_reads=0;self.label_headers=0;self.raw_reads=0
        self.bad_header=False;self.short_header=False;self.short_prefix=False
        self.mismatch=False;self.record_mismatch=False;self.drift=False
    def read_memory(self,where,size):
        values={f'{ds:04X}:EC74':struct.pack('<hhh',2,3,-1),
                f'{ds:04X}:188C':struct.pack('<6H',4,self.groups['main'],4,self.groups['music_only'],4,self.groups['digital']),
                f'{ds:04X}:1888':bytes(4),f'{ds:04X}:ECCC':struct.pack('<H',1)}
        if where==f'{ds:04X}:1898':
            self.raw_reads+=1
            value=struct.pack('<6H',4,self.labels['digital'],4,self.labels['music_only'],4,self.labels['main'])
            if self.drift and self.raw_reads>1:value=bytes(12)
            assert size==12;return NS(data=value)
        if where in values:
            assert len(values[where])==size;return NS(data=values[where])
        for group,segment in self.groups.items():
            if where==f'{segment:04X}:0000':
                assert size==2;return NS(data=struct.pack('<H',((counts[group]*254&65535)+19)//16))
            if where==f'{segment:04X}:0004':
                assert group!='music_only' and size==len(expected[group])
                value=bytearray(expected[group])
                if self.record_mismatch and group=='main':value[100]^=1
                return NS(data=bytes(value))
        for group,segment in self.labels.items():
            if where==f'{segment:04X}:0000':
                self.label_headers+=1;assert size==2
                value=struct.pack('<H',((counts[group]*80&65535)+19)//16+int(self.bad_header and group=='music_only'))
                return NS(data=value[:1] if self.short_header else value)
            for index,name in enumerate(names[group]):
                if where==f'{segment:04X}:{4+80*index:04X}':
                    assert self.label_headers==3 and group!='music_only' and size==len(name)+1
                    self.label_reads+=1;value=bytearray(name+b'\\0')
                    if self.mismatch and group=='main' and index==0:
                        value=bytearray(v^1 for v in value)
                    return NS(data=bytes(value[:-1] if self.short_prefix else value))
        raise AssertionError('read outside bounded named prefixes')
m=Memory();baseline=observe_fields(m,stop,loaded,'labels')
r,payloads,records=observe_labels(m,stop,loaded,projection,baseline)
assert r['all_populated_equal'] and m.label_reads==5 and m.label_headers==3
assert r['groups']['music_only']['compared_bytes']==0 and r['groups']['music_only']['equal'] is None
assert records=={k:v for k,v in expected.items() if v}
assert payloads['main']==(b'A'*39+b'\\0'+b'B'*39+b'\\0',)*2
for attr in ['bad_header','short_header','short_prefix','drift']:
    m=Memory();setattr(m,attr,True);reject(lambda:observe_labels(m,stop,loaded,projection,baseline))
    if attr in ['bad_header','short_header']:assert m.label_reads==0
m=Memory();m.mismatch=True;r,payloads,records=observe_labels(m,stop,loaded,projection,baseline)
assert not r['all_populated_equal'] and r['groups']['main']['differing_bytes']==40
assert r['groups']['main']['first_differing_offsets']==list(range(32))
m=Memory();m.record_mismatch=True;r,payloads,records=observe_labels(m,stop,loaded,projection,baseline)
assert not r['all_populated_equal'] and records['main']!=expected['main']
for segment in [0,0x2000,0x4000,0x5201,0xFFF0]:
    m=Memory();m.labels['digital']=segment
    reject(lambda:observe_labels(m,stop,loaded,projection,baseline));assert m.label_reads==0
bad={**baseline,'mode':2};reject(lambda:observe_labels(Memory(),stop,loaded,projection,bad))
bad={**stop,'cs_ip':address(loaded,*PHASES['returned'])}
reject(lambda:observe_labels(Memory(),bad,loaded,projection,baseline))
for name in [b'Q'*40,b'bad\\nname',b'bad\\x01name']:
    body=name+b'\\0'*(508-len(name))
    reject(lambda:expected_labels({'counts':counts,'payloads':{**expected,'main':body}}))
reject(lambda:expected_labels({'counts':{**counts,'main':0},'payloads':expected}))
`, 'tools/research/rust-dos-runs');
});

function check(code, directory) {
  const result = spawnSync(enginePython(), ['-I', '-', resolve(directory)], {encoding: 'utf8', input: code});
  assert.equal(result.status, 0, result.stderr || result.stdout);
}

test('notice guard reads only a stable bounded incoming far return at the declared entry', () => {
  check(`
import sys,struct,copy
from types import SimpleNamespace as NS
sys.path.insert(0,sys.argv[1])
from setup_parser import observe_notice_return,address
loaded=0x05BA
stop={'reason':'breakpoint','cs_ip':address(loaded,0x190F,0),
      'registers':{'ds':'13F0','ss':'22D0','sp':'0DE2'}}
class Memory:
    def __init__(self,short=False,drift=False):self.calls=0;self.short=short;self.drift=drift
    def read_memory(self,where,size):
        assert where=='22D0:0DE2' and size==4
        self.calls+=1
        body=struct.pack('<HH',0x1234,0xAA0+self.calls*int(self.drift))
        return NS(data=body[:3] if self.short else body)
def reject(s,m=None):
    try:observe_notice_return(m or Memory(),s,loaded)
    except ValueError:return
    raise AssertionError('unsupported notice return admitted')
m=Memory();r=observe_notice_return(m,stop,loaded)
assert m.calls==2 and r['return_ip']==0x1234 and r['return_cs']==0xAA0
assert r['canonical_return_segment']==0x14E6
for key,value in [('reason','paused'),('cs_ip',address(loaded,0x190F,1))]:
    s=copy.deepcopy(stop);s[key]=value;reject(s)
for key,value in [('ds','0000'),('ss','0000'),('ss','A000'),('sp','FFFD'),('sp','10000'),('sp','invalid')]:
    s=copy.deepcopy(stop);s['registers'][key]=value;reject(s)
reject(stop,Memory(short=True));reject(stop,Memory(drift=True))
`, 'tools/research/rust-dos-runs');
});

test('notice objects require the fixed caller and arguments before bounded stable text reads', () => {
  check(`
import sys,struct,copy
from types import SimpleNamespace as NS
sys.path.insert(0,sys.argv[1])
from setup_parser import observe_notice_objects,address
loaded=0x05BA;ds=0x13F0
stop={'reason':'breakpoint','cs_ip':address(loaded,0x190F,0),
      'registers':{'ds':'13F0','ss':'22D0','sp':'0DE2'}}
class Memory:
    def __init__(self):
        self.frame=[0x050D,0x0C06,0xAF36,ds,0xD244,ds]
        self.body=b'A'+bytes([10])+b'BC'+bytes([0]);self.choices=b'OK'+bytes([0])
        self.short=False;self.drift=False;self.frame_drift=False;self.seen={};self.text_reads=0
    def read_memory(self,where,size):
        self.seen[where,size]=self.seen.get((where,size),0)+1
        if where=='22D0:0DE2':
            assert size in (4,12)
            value=struct.pack('<6H',*self.frame)[:size]
            if self.frame_drift and size==12 and self.seen[where,size]>1:value=bytes(size)
        elif where==f'{ds:04X}:AF36':assert size==2;value=bytes(2)
        elif where==f'{ds:04X}:D244':assert size==12;value=struct.pack('<6H',*range(6))
        else:
            assert size==1 and where.startswith(f'{ds:04X}:')
            offset=int(where.split(':')[1],16);self.text_reads+=1
            start,body=(0xAF38,self.body) if offset<0xB138 else (0xB138,self.choices)
            index=offset-start;assert 0<=index<len(body)
            value=body[index:index+1]
            if self.drift and self.seen[where,size]>1:value=bytes([255])
        return NS(data=value[:-1] if self.short else value)
def reject(m):
    try:observe_notice_objects(m,stop,loaded)
    except ValueError:return
    raise AssertionError('unsupported notice object profile admitted')
m=Memory();r,p=observe_notice_objects(m,stop,loaded)
assert r['selector']==0 and r['style_words']==list(range(6))
assert r['text']['body']['row_lengths']==[1,2] and r['text']['body']['lf_count']==1
assert p=={'body':m.body,'choices':m.choices} and m.text_reads==16
for index,value in [(0,0x050E),(1,0x0C07),(2,0xAF38),(3,0),(4,0xD246),(5,0)]:
    m=Memory();m.frame[index]=value;reject(m);assert m.text_reads==0
for attr in ('short','drift','frame_drift'):
    m=Memory();setattr(m,attr,True);reject(m)
for body in (bytes([13,0]),bytes([127,0]),b'A'*512):
    m=Memory();m.body=body;reject(m)
m=Memory();m.choices=b'A'*128;reject(m)
`, 'tools/research/rust-dos-runs');
});

test('notice pre-input endpoint binds wrapped locals and retains unexpected values without following pointers', () => {
  check(`
import sys,struct,copy
from types import SimpleNamespace as NS
sys.path.insert(0,sys.argv[1])
from setup_parser import observe_notice_poll,address,require_notice_profile
loaded=0x05BA;ds=0x13F0;bp=0x0DE0
stop={'reason':'breakpoint','cs_ip':address(loaded,0x190F,0x054B),
      'registers':{'ds':'13F0','ss':'22D0','sp':'F676','bp':'0DE0'}}
objects={'entry':{'ss':0x22D0,'sp':0x0DE2,'return_ip':0x050D,'return_cs':0x0C06},
         'selector':0,'style_words':[15,1,7,0,15,2],
         'text':{'body':{'length':99,'lf_count':2},'choices':{'length':8,'lf_count':0}}}
texts={'body':b'A'*99+bytes([0]),'choices':b'B'*8+bytes([0])}
class Memory:
    def __init__(self):
        self.seen={};self.drift=False;self.short=False;self.bad_frame=False;self.changed=False
    def read_memory(self,where,size):
        self.seen[where,size]=self.seen.get((where,size),0)+1
        seg,off=(int(v,16) for v in where.split(':'))
        if seg==0x22D0:
            if off==0x0DE2:
                assert size==12;value=struct.pack('<6H',0x050D,0x0C06,0xAF36,ds,0xD244,ds)
                if self.bad_frame:value=bytes(12)
            elif off==0xF67A:assert size==12;value=struct.pack('<6H',*range(6))
            else:
                assert size==2 and (bp-off) in (4,6,10,20,22,28,30,32,34,36)
                value=struct.pack('<H',0xFFFF)
        else:
            assert seg==ds
            if off==0x18F6:assert size==4;value=struct.pack('<HH',1,1)
            elif off==0xAF36:assert size==2;value=bytes(2)
            elif off==0xD244:assert size==12;value=struct.pack('<6H',*objects['style_words'])
            elif 0xAF38<=off<0xAF38+100:
                value=texts['body'][off-0xAF38:off-0xAF38+size]
                if self.changed:value=bytes(size)
            elif off==0xB138:assert size==9;value=texts['choices']
            else:raise AssertionError('unbound read')
        if self.drift and self.seen[where,size]>1:value=bytes([123])*size
        return NS(data=value[:-1] if self.short else value)
def reject(fn):
    try:fn()
    except ValueError:return
    raise AssertionError('unsupported notice poll admitted')
r,p=observe_notice_poll(Memory(),stop,loaded,objects,texts)
assert r['locals']['choice_rows']==65535 and r['row_words']==list(range(6))
assert r['prefixes_retained'] and r['style_retained'] and p==texts
m=Memory();m.changed=True;r,p=observe_notice_poll(m,stop,loaded,objects,texts)
assert not r['prefixes_retained'] and p['body']!=texts['body']
for key,value in [('ds','0000'),('ss','22D1'),('sp','F674'),('bp','0DE2')]:
    s=copy.deepcopy(stop);s['registers'][key]=value
    reject(lambda:observe_notice_poll(Memory(),s,loaded,objects,texts))
s=copy.deepcopy(stop);s['cs_ip']=address(loaded,0x190F,0x0724)
reject(lambda:observe_notice_poll(Memory(),s,loaded,objects,texts))
for attr in ('short','drift','bad_frame'):
    m=Memory();setattr(m,attr,True);reject(lambda:observe_notice_poll(m,stop,loaded,objects,texts))
o=copy.deepcopy(objects);o['selector']=1;reject(lambda:require_notice_profile(o))
`, 'tools/research/rust-dos-runs');
});

test('notice input endpoints bind poll arguments, selected state and the live caller return without expired locals', () => {
  check(`
import sys,struct,copy
from types import SimpleNamespace as NS
sys.path.insert(0,sys.argv[1])
from setup_parser import observe_notice_input,address
loaded=0x05BA;entry={'ss':0x22D0,'sp':0x0DE2}
class Memory:
    def __init__(self):self.short=False;self.drift=False;self.bad=False;self.seen={}
    def read_memory(self,where,size):
        self.seen[where,size]=self.seen.get((where,size),0)+1
        values={'22D0:F674':bytes([255,255]),'22D0:0DDC':bytes(2),
                '22D0:0DC4':struct.pack('<H',13),
                '22D0:0DE6':struct.pack('<4H',0xAF36,0x13F0,0xD244,0x13F0)}
        value=values[where];assert len(value)==size
        if self.bad or self.drift and self.seen[where,size]>1:value=bytes([123])*size
        return NS(data=value[:-1] if self.short else value)
def stop(phase):
    site,sp={'poll':((0x190F,0x0554),0xF674),'selected':((0x190F,0x0724),0xF676),
             'returned':((0x164C,0x050D),0x0DE6)}[phase]
    return {'reason':'breakpoint','cs_ip':address(loaded,*site),
            'registers':{'ds':'13F0','ss':'22D0','sp':f'{sp:04X}','bp':'0DE0','ax':'000D'}}
def reject(fn):
    try:fn()
    except ValueError:return
    raise AssertionError('unsupported notice input admitted')
for phase in ('poll','selected','returned'):
    m=Memory();s=stop(phase);r=observe_notice_input(m,s,loaded,entry,phase)
    assert r['ax']==13 and r['phase']==phase
    for attr in ('short','drift','bad'):
        m=Memory();setattr(m,attr,True);reject(lambda:observe_notice_input(m,s,loaded,entry,phase))
    for key,value in [('ds','0000'),('ss','0000'),('sp','0000')]:
        bad=copy.deepcopy(s);bad['registers'][key]=value
        reject(lambda:observe_notice_input(Memory(),bad,loaded,entry,phase))
    s['cs_ip']=address(loaded,0x190F,0x054B)
    reject(lambda:observe_notice_input(Memory(),s,loaded,entry,phase))
s=stop('poll');s['registers']['bp']='0DE2'
reject(lambda:observe_notice_input(Memory(),s,loaded,entry,'poll'))
s=stop('returned');s['registers']['bp']='0000'
assert observe_notice_input(Memory(),s,loaded,entry,'returned')['ax']==13
reject(lambda:observe_notice_input(Memory(),s,loaded,entry,'unknown'))
`, 'tools/research/rust-dos-runs');
});

test('next setup boundary admits only the notice guard or fixed chooser far caller', () => {
  check(`
import sys,struct,copy
from types import SimpleNamespace as NS
sys.path.insert(0,sys.argv[1])
from setup_parser import observe_notice_next,address
loaded=0x05BA
s={'reason':'breakpoint','cs_ip':address(loaded,0x19CF,0x009A),
   'registers':{'ds':'13F0','ss':'22D0','sp':'0DE2'}}
class Memory:
    def __init__(self):self.seen=0;self.short=False;self.drift=False;self.ip=0x008F;self.cs=0x0F89
    def read_memory(self,where,size):
        assert where=='22D0:0DE2' and size==4;self.seen+=1
        value=struct.pack('<HH',self.ip,self.cs)
        if self.drift and self.seen>1:value=bytes(4)
        return NS(data=value[:3] if self.short else value)
def reject(fn):
    try:fn()
    except ValueError:return
    raise AssertionError('unsupported next boundary admitted')
assert observe_notice_next(Memory(),s,loaded)['boundary']=='chooser'
for attr in ('short','drift'):
    m=Memory();setattr(m,attr,True);reject(lambda:observe_notice_next(m,s,loaded))
m=Memory();m.ip=0x0091;reject(lambda:observe_notice_next(m,s,loaded))
m=Memory();m.cs=0x19CF;reject(lambda:observe_notice_next(m,s,loaded))
for key,value in [('ds','0000'),('ss','A000'),('sp','FFFE')]:
    bad=copy.deepcopy(s);bad['registers'][key]=value
    reject(lambda:observe_notice_next(Memory(),bad,loaded))
bad=copy.deepcopy(s);bad['cs_ip']=address(loaded,0x164C,0x050D)
reject(lambda:observe_notice_next(Memory(),bad,loaded))
bad=copy.deepcopy(s);bad['reason']='time';reject(lambda:observe_notice_next(Memory(),bad,loaded))
notice=copy.deepcopy(s);notice['cs_ip']=address(loaded,0x190F,0)
m=Memory();m.ip=0x1234;m.cs=0x0C06
r=observe_notice_next(m,notice,loaded)
assert r['boundary']=='notice' and r['entry']['canonical_return_segment']==0x164C
`, 'tools/research/rust-dos-runs');
});

test('fixed chooser row observations bind both callers, stack text, vector and paused geometry', () => {
  check(`
import sys,struct
from types import SimpleNamespace as NS
sys.path.insert(0,sys.argv[1])
from setup_parser import observe_fields, observe_chooser, observe_rows, observe_label_display, address, PHASES
def reject(fn):
    try:fn()
    except ValueError:return
    raise AssertionError('unsupported chooser observation admitted')
loaded=0x1000;ds=0x1E36;ss=0x3000;bp=0xF000;sp=0xEBEC
counts={'main':2,'digital':1,'music_only':-1}
projection={'counts':counts,'payloads':{'main':b'Alpha'+bytes(249)+b'Beta'+bytes(250),
                                     'digital':b'Gamma'+bytes(249),'music_only':b''}}
entry_stop={'reason':'breakpoint','cs_ip':address(loaded,*PHASES['chooser']),
 'registers':{'ds':f'{ds:04X}','ss':f'{ss:04X}','bp':f'{bp:04X}','sp':f'{sp:04X}','ax':'0000'}}
row_bp=sp-2;row_sp=row_bp-0x1DB2
rows_stop={'reason':'breakpoint','cs_ip':address(loaded,*PHASES['rows']),
 'registers':{**entry_stop['registers'],'bp':f'{row_bp:04X}','sp':f'{row_sp:04X}','si':'0002','di':'0000'}}
class Memory:
 def __init__(self):
  self.data={f'{ds:04X}:EC74':struct.pack('<hhh',2,1,-1),
    f'{ds:04X}:188C':struct.pack('<6H',4,0x4000,4,0x4200,4,0x4100),
    f'{ds:04X}:1888':bytes(4),f'{ds:04X}:ECCC':struct.pack('<H',1),
    f'{ss:04X}:{sp:04X}':struct.pack('<7H',0x8F,0x19CF,0,0x20,ds,bp-0x400,ss),
    f'{ss:04X}:{bp+2:04X}':struct.pack('<8H',0x296,0x1429,2,4,0x5200,0x20,ds,0),
    f'{ss:04X}:{bp-0x400:04X}':b'Alpha'+bytes([10])+b'Beta'+bytes([10,0]),
    f'{ss:04X}:{row_bp-0x1C:04X}':struct.pack('<hh',2,2),
    f'{ds:04X}:DEB8':bytes([0,0,79,24,7]),
    f'{ds:04X}:DEBE':struct.pack('<5B3H',3,25,80,0,0,0,0xB800,1),
    '0000:0000':struct.pack('<HH',0x174,loaded)}
  self.text_reads=0;self.vector_reads=0;self.drift=False;self.short=None
 def read_memory(self,where,size):
  value=self.data[where];assert len(value)==size
  if where==f'{ss:04X}:{bp-0x400:04X}':self.text_reads+=1
  if where=='0000:0000':
   self.vector_reads+=1
   if self.drift and self.vector_reads>1:value=bytes(4)
  if where==self.short:value=value[:-1]
  return NS(data=value)
m=Memory();baseline=observe_fields(m,entry_stop,loaded,'chooser')
entry,pair=observe_chooser(m,entry_stop,loaded,projection,baseline,{'offset':4,'segment':0x5200})
assert entry['equal'] and pair[0]==pair[1] and m.text_reads==1
r=observe_rows(m,rows_stop,loaded,entry)
assert r['row_count']==2 and r['matches_source_count'] and r['index_matches_entry']
assert r['vector_zero']['matches_declared_handler'] and r['display']['columns']==80
label_stop={**entry_stop,'cs_ip':address(loaded,*PHASES['labels'])}
m=Memory();label_fields=observe_fields(m,label_stop,loaded,'labels')
assert observe_label_display(m,label_stop,loaded,label_fields)['display']['rows']==25
m=Memory();m.data['0000:0000']=bytes(4)
m.data[f'{ds:04X}:DEBE']=struct.pack('<5B3H',7,0,0,1,1,5,0xB000,0)
r=observe_label_display(m,label_stop,loaded,label_fields)
assert not r['vector_zero']['matches_declared_handler'] and r['display']['direct_enabled']==0
m=Memory();m.drift=True;reject(lambda:observe_label_display(m,label_stop,loaded,label_fields))
m=Memory();m.short=f'{ds:04X}:DEB8';reject(lambda:observe_label_display(m,label_stop,loaded,label_fields))
reject(lambda:observe_label_display(Memory(),entry_stop,loaded,label_fields))
reject(lambda:observe_label_display(Memory(),label_stop,loaded,{**label_fields,'mode':2}))
m=Memory();m.data['0000:0000']=bytes(4)
assert not observe_rows(m,rows_stop,loaded,entry)['vector_zero']['matches_declared_handler']
m=Memory();m.drift=True;reject(lambda:observe_rows(m,rows_stop,loaded,entry))
for key in ['0000:0000',f'{ds:04X}:DEBE',f'{ss:04X}:{row_bp-0x1C:04X}']:
 m=Memory();m.short=key;reject(lambda:observe_rows(m,rows_stop,loaded,entry))
for reg,value in [('bp','0001'),('ss','3001'),('sp','0100')]:
 bad={**rows_stop,'registers':{**rows_stop['registers'],reg:value}}
 reject(lambda:observe_rows(Memory(),bad,loaded,entry))
bad={**rows_stop,'registers':{**rows_stop['registers'],'si':'0000'}}
assert not observe_rows(Memory(),bad,loaded,entry)['matches_source_count']
for changed in [struct.pack('<7H',0x90,0x19CF,0,0x20,ds,bp-0x400,ss),
                struct.pack('<7H',0x8F,0x19CF,2,0x20,ds,bp-0x400,ss),
                struct.pack('<7H',0x8F,0x19CF,0,0x20,ds,bp-0x3FF,ss)]:
 m=Memory();m.data[f'{ss:04X}:{sp:04X}']=changed
 reject(lambda:observe_chooser(m,entry_stop,loaded,projection,baseline,{'offset':4,'segment':0x5200}))
 assert m.text_reads==0
m=Memory();m.data[f'{ss:04X}:{bp+2:04X}']=struct.pack('<8H',0x297,0x1429,2,4,0x5200,0x20,ds,0)
reject(lambda:observe_chooser(m,entry_stop,loaded,projection,baseline,{'offset':4,'segment':0x5200}))
m=Memory();m.data[f'{ss:04X}:{bp-0x400:04X}']=bytes(len(m.data[f'{ss:04X}:{bp-0x400:04X}']))
r,pair=observe_chooser(m,entry_stop,loaded,projection,baseline,{'offset':4,'segment':0x5200})
assert not r['equal'] and r['differing_bytes']>0
m=Memory();m.short=f'{ss:04X}:{bp-0x400:04X}'
reject(lambda:observe_chooser(m,entry_stop,loaded,projection,baseline,{'offset':4,'segment':0x5200}))
bad={**entry_stop,'cs_ip':address(loaded,*PHASES['labels'])}
reject(lambda:observe_chooser(Memory(),bad,loaded,projection,baseline,{'offset':4,'segment':0x5200}))
`, 'tools/research/rust-dos-runs');
});

test('fixed setup boundary candidates preserve far aliases, relative forms and wrapped frame fetch', () => {
  check(`
import sys, struct
sys.path.insert(0,sys.argv[1])
from setup_stub_transfers import FRAME, candidates
b=bytearray([0x90])*0x20000
def near(ip, opcode, target, width):
    length=len(opcode)+width
    raw=bytes(opcode)+((target-(ip+length))&((1<<(width*8))-1)).to_bytes(width,'little')
    for k,v in enumerate(raw):b[FRAME+((ip+k)&0xffff)]=v
near(0x10,[0xe8],0xc96,2)
near(0x100,[0xe9],0xc95,2)
near(0xc80,[0x74],0xc96,1)
near(0xc84,[0xe2],0xc95,1)
near(0xc88,[0xeb],0xc96,1)
near(0x200,[0x0f,0x82],0xc96,2)
near(0xffff,[0xe9],0xc95,2)
# Two segment:offset aliases of the same fixed physical load offset.
struct.pack_into('<BHH',b,0x100,0x9a,0xc96,0xc08)
struct.pack_into('<BHH',b,0x110,0xea,0xc86,0xc09)
struct.pack_into('<BHH',b,0x120,0x9a,0xc97,0xc08)
r=candidates(bytes(b))
assert r[0xcd16]['far']==[{'site':0x100,'kind':'call'},{'site':0x110,'kind':'jump'}]
assert {(x['site']-FRAME,x['kind']) for x in r[0xcd16]['relative']}=={(0x10,'call'),(0xc80,'conditional'),(0xc88,'jump'),(0x200,'conditional')}
assert {(x['site']-FRAME,x['kind']) for x in r[0xcd15]['relative']}=={(0x100,'jump'),(0xc84,'conditional'),(0xffff,'jump')}
for bad in (b'',bytes(FRAME+0xffff),bytes(0x20001),b):
    try:candidates(bad)
    except ValueError:pass
    else:raise AssertionError('invalid image admitted')
`, 'tools/research/exec-census');
});

test('setup source projection preserves documented words, strings, nested bits and duplicate stores', () => {
  check(`
import sys, struct
sys.path.insert(0,sys.argv[1])
from setup_record_projection import numeric_word, project_source
def reject(fn):
    try: fn()
    except ValueError: return
    raise AssertionError('accepted unsupported source projection')
def source(records,digital=()):
    rows=[]
    for group,items in [('MainGroup',records),('DigitalGroup',digital),('MusicOnlyGroup',())]:
        rows+=['['+group+']','[NumberOfRecords] '+str(-1 if group=='MusicOnlyGroup' else len(items))]
        for name,fields in items:
            rows+=['[StartRecord]','[CardName] ['+name+']',*fields,'[EndRecord]']
        rows+=['[EndOfGroup]']
    return chr(10).join(rows+['[EndOfFile]'])+chr(10)
fields=['[CardId] 7','[CardGroup] 4',
        '[NumberOfMidiAddresses] 3','[MidiAddress] 0x330','[MidiAltAddress] 0x3A8','[MidiAltAddress] 0x320',
        '[NumberOfDigitalAddresses] 2','[DigitalAltAddress] 0x380',
        '[NumberOfMidiIrqs] 2','[MidiIrq] 5','[MidiAltIrq] 7',
        '[NumberOfIrqs] 2','[Irq] 10','[AltIrq] 11',
        '[NumberOfDmas] 2','[Dma] 1','[AltDma] 3','[Dma] -1',
        '[RmDigitalDriverName] [D0]','[RmMidiDriverName] [M0]',
        '[PmDigitalDriverName] [D1]','[PmMidiDriverName] [M1]',
        '[DigitalDriverChunkNumber] 12','[MidiDriverChunkNumber] 13','[SamplePlaybackMode] 1',
        '[Flags] 3','[DspEnabled]','[DspNotEnabled]','[MidiNotEnabled]',
        '[Flags] 2','[CdEnabled]','[MultipleInstallDisabled]',
        '[Jumpers] 9','[MidiAddressJumper]','[MidiIrqJumper]','[DigitalAddressJumper]',
        '[DigitalIrqJumper]','[DigitalDmaJumper]','[AddressJumper]','[IrqJumper]',
        '[DmaJumper]','[MidiDmaJumper]']
r=project_source(source([('A',fields),('B',[])]))
assert r['counts']=={'main':2,'digital':0,'music_only':-1}
body=r['payloads']['main']; assert len(body)==508 and not r['payloads']['music_only']
assert body[:2]==bytes([65,0]) and body[254:256]==bytes([66,0])
assert body[256:]==bytes(252) and body[0xE0:254]==bytes(30)
assert struct.unpack_from('<5H',body,0x28)==(7,4,3,816,3)
assert struct.unpack_from('<H',body,0x32)[0]==800
assert struct.unpack_from('<3H',body,0x42)==(2,0,896)
assert struct.unpack_from('<3H',body,0x58)==(2,5,7)
assert struct.unpack_from('<3H',body,0x6E)==(2,10,11)
assert struct.unpack_from('<3H',body,0x84)==(2,65535,3)
assert [body[at:at+3] for at in (0x9A,0xA9,0xB8,0xC7)]==[b'D0'+bytes(1),b'M0'+bytes(1),b'D1'+bytes(1),b'M1'+bytes(1)]
assert struct.unpack_from('<5H',body,0xD6)==(12,13,1,9,255)
for spelling,word in [('00012',12),('-32768',32768),('65536',0),('0x3A8',3),
                      ('0xffff',0),('-0x330',0),('4294967295',65535),
                      ('-4294967295',1),('4294967296',65535),('-4294967296',0)]:
    assert numeric_word(spelling)==word
for spelling in ['-','--1','0x','+1','1 suffix','9'*20]: reject(lambda:numeric_word(spelling))
assert len(project_source(source([('A'*39,[])]))['payloads']['main'])==254
reject(lambda:project_source(source([('A'*40,[])])))
for tag in ['RmDigitalDriverName','RmMidiDriverName','PmDigitalDriverName','PmMidiDriverName']:
    assert len(project_source(source([('A',['['+tag+'] ['+'Q'*14+']'])]))['payloads']['main'])==254
    reject(lambda:project_source(source([('A',['['+tag+'] ['+'Q'*15+']'])])))
reject(lambda:project_source(source([('A',['[SamplePlaybackMode] 2'])])))
reject(lambda:project_source(source([('A',['[NumberOfDmas] 11'])])))
reject(lambda:project_source(source([('A',['[Jumpers] 1','[OpaqueJumper]'])])))
reject(lambda:project_source(source([('A',[])])[:-1]))
`, 'tools/research/exec-census');
});

test('setup record observations require disjoint matching headers before payload reads and retain bounded differences', () => {
  check(`
import sys, struct
from types import SimpleNamespace as NS
sys.path.insert(0,sys.argv[1])
from setup_parser import observe_records, address, PHASES
def reject(fn):
    try: fn()
    except ValueError: return
    raise AssertionError('accepted unsupported record observation')
loaded=0x1000; ds=0x1E36; ss=0x3000; sp=0x100
counts={'main':2,'digital':3,'music_only':-1}
expected={'main':bytes(i%251 for i in range(508)),
          'digital':bytes(i%239 for i in range(762)),'music_only':b''}
projection={'counts':counts.copy(),'payloads':expected.copy()}
stop={'reason':'breakpoint','cs_ip':address(loaded,*PHASES['returned']),
      'registers':{'ds':f'{ds:04X}','ss':f'{ss:04X}','sp':f'{sp:04X}','ax':'0001'}}
class Memory:
    def __init__(self):
        self.groups={'main':0x4000,'digital':0x4100,'music_only':0x4200}
        self.counts=counts.copy(); self.offset=4; self.headers=0; self.payloads=0
        self.bad_header=False; self.short_header=False; self.short_payload=False
        self.mismatch=False; self.drift=False
    def read_memory(self,where,size):
        values={f'{ds:04X}:EC74':struct.pack('<hhh',self.counts['main']+int(self.drift and self.payloads==2),self.counts['digital'],self.counts['music_only']),
                f'{ds:04X}:188C':struct.pack('<6H',self.offset,self.groups['main'],self.offset,self.groups['music_only'],self.offset,self.groups['digital']),
                f'{ds:04X}:1888':struct.pack('<HH',0,0), f'{ds:04X}:ECCC':struct.pack('<H',1)}
        if where in values:
            assert len(values[where])==size; return NS(data=values[where])
        for name,segment in self.groups.items():
            if where==f'{segment:04X}:0000':
                self.headers+=1
                value=((self.counts[name]*254&65535)+19)//16
                data=struct.pack('<H',value+int(self.bad_header and name=='music_only'))
                return NS(data=data[:1] if self.short_header else data)
            if where==f'{segment:04X}:0004':
                assert self.headers==3 and name!='music_only'
                self.payloads+=1
                data=bytearray(expected[name]); assert len(data)==size
                if self.mismatch and name=='main':
                    for i in range(40): data[i]^=1
                return NS(data=bytes(data[:-1] if self.short_payload else data))
        raise AssertionError('read outside named record fields')
m=Memory(); r,native=observe_records(m,stop,loaded,projection)
assert r['all_populated_equal'] and m.headers==3 and m.payloads==2
assert native=={name:expected[name] for name in ('main','digital')}
assert r['groups']['main']['compared_bytes']==508 and r['groups']['main']['header_paragraphs']==32
assert r['groups']['music_only']['equal'] is None and r['groups']['music_only']['compared_bytes']==0
assert r['groups']['music_only']['header_paragraphs']==4081
m=Memory(); m.mismatch=True; r,native=observe_records(m,stop,loaded,projection)
assert not r['all_populated_equal'] and r['groups']['main']['differing_bytes']==40
assert r['groups']['main']['first_differing_offsets']==list(range(32))
for attr in ['bad_header','short_header']:
    m=Memory(); setattr(m,attr,True); reject(lambda:observe_records(m,stop,loaded,projection))
    assert m.payloads==0
m=Memory(); m.short_payload=True; reject(lambda:observe_records(m,stop,loaded,projection))
m=Memory(); m.drift=True; reject(lambda:observe_records(m,stop,loaded,projection))
for mutate in [lambda m:setattr(m,'offset',5),
               lambda m:m.groups.update(main=0x2000),
               lambda m:m.groups.update(digital=0x4001),
               lambda m:m.groups.update(music_only=0xFFF0),
               lambda m:m.counts.update(main=1)]:
    m=Memory(); mutate(m); reject(lambda:observe_records(m,stop,loaded,projection)); assert m.headers==0 and m.payloads==0
bad={**stop,'registers':{**stop['registers'],'ax':'0000'}}
m=Memory(); reject(lambda:observe_records(m,bad,loaded,projection)); assert m.headers==0
bad={**stop,'registers':{**stop['registers'],'ss':'FFFF'}}
reject(lambda:observe_records(Memory(),bad,loaded,projection))
wrong={'counts':counts.copy(),'payloads':{**expected,'main':b''}}
reject(lambda:observe_records(Memory(),stop,loaded,wrong))
`, 'tools/research/rust-dos-runs');
});
