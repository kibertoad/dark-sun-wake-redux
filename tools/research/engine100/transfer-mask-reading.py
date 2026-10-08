import json
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
from capstone.x86_const import X86_OP_MEM
import xxhash
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
p=Path(GAME_DIR+'/analysis/reporter-audit/issue5-transfer-boundaries100')
c=json.loads((p/'documented-primitive.json').read_text());data=Path(c['source']).read_bytes();assert xxhash.xxh3_128_hexdigest(data)==c['xxh3']
md=Cs(CS_ARCH_X86,CS_MODE_16);md.detail=True
matches=[]
for i in md.disasm(data[0x154de:0x15853],0x43ae):
 for oi,o in enumerate(i.operands):
  if o.type==X86_OP_MEM and o.mem.disp==0x26dc:
   matches.append({'site':0x154de+i.address-0x43ae,'width':o.size,'access':o.access,'segment':md.reg_name(o.mem.segment),'base':md.reg_name(o.mem.base),'index':md.reg_name(o.mem.index),'scale':o.mem.scale})
print(json.dumps({'maskOperands':matches}))
r=json.loads((p/'wrapper-callees.report.json').read_text())
node=next(n for n in r['nodes'] if n['entry']==0x154de)
print(json.dumps({'observations':[o for o in node['memoryObservations'] if o.get('site') in [x['site'] for x in matches]],'keys':list(node['memoryObservations'][0])}))
for i in md.disasm(data[87970:88010],0x43ae+87970-0x154de): print(hex(0x154de+i.address-0x43ae),i.mnemonic,i.op_str)