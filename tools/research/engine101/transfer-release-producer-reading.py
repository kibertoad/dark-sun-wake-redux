from pathlib import Path
import json,xxhash
from capstone import Cs,CS_ARCH_X86,CS_MODE_16,CS_AC_WRITE
from capstone.x86_const import X86_OP_MEM,X86_OP_IMM,X86_OP_REG
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();assert xxhash.xxh3_128_hexdigest(b)=='e296af55ba2ecde7e77f555c90f33d0b'
d=Path(GAME_DIR+'/analysis/reporter-audit/issue5-transfer-release-callers101')
m=Cs(CS_ARCH_X86,CS_MODE_16);m.detail=True
ins=list(m.disasm(b[91236:92484],91236));args=[]
for k,i in enumerate(ins):
 if i.mnemonic=='call' and i.operands[0].type==X86_OP_IMM and i.operands[0].imm==80373:
  a=ins[k-2];assert ins[k-1].mnemonic=='push' and ins[k-1].op_str=='cs'
  assert a.mnemonic=='push' and a.operands[0].type==X86_OP_MEM
  args.append({'callSite':i.address,'pushSite':a.address,'field':a.operands[0].mem.disp,'width':a.operands[0].size})
fields=sorted({r['field'] for r in args});writers=[];ds=[]
for k,i in enumerate(ins):
 if any(m.reg_name(r)=='ds' for r in i.regs_access()[1]):ds.append({'site':i.address,'context':[{'site':v.address,'mnemonic':v.mnemonic,'operands':v.op_str} for v in ins[max(0,k-2):k+2]]})
 for o in i.operands:
  if o.type==X86_OP_MEM and o.mem.disp in fields and o.access&CS_AC_WRITE:
   source=i.operands[1]
   if source.type==X86_OP_IMM:
    assert source.imm==0xffff;role='sentinel'
   else:
    assert source.type==X86_OP_REG and m.reg_name(source.reg)=='ax'
    assert ins[k-1].mnemonic=='add' and ins[k-1].op_str.startswith('sp,')
    assert ins[k-2].mnemonic=='call' and ins[k-2].operands[0].imm in [80088,80221]
    role='allocator-return' if ins[k-2].operands[0].imm==80088 else 'request-return'
   writers.append({'site':i.address,'field':o.mem.disp,'width':o.size,'role':role,'context':[{'site':v.address,'mnemonic':v.mnemonic,'operands':v.op_str} for v in ins[max(0,k-4):k+2]]})
r={'argumentFields':args,'ownFieldWriters':writers,'ownDSWrites':ds,'qualification':'Source local producer leads only; callee effects, current DS and native fields remain unverified.'};(d/'direct-release-producer-reading.json').write_text(json.dumps(r,indent=2));print(json.dumps(r))
