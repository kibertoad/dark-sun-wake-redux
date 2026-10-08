from pathlib import Path
import json,struct,xxhash
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
from capstone.x86_const import X86_OP_MEM,X86_OP_IMM
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();assert xxhash.xxh3_128_hexdigest(b)=='e296af55ba2ecde7e77f555c90f33d0b'
d=Path(GAME_DIR+'/analysis/reporter-audit/issue5-transfer-field-incoming101')
m=Cs(CS_ARCH_X86,CS_MODE_16);m.detail=True
ins=list(m.disasm(b[0x81130:0x81457],0x81130))
rows=[i for i in ins if i.mnemonic=='jmp' and i.operands[0].type!=X86_OP_IMM]
rows=[i for i in rows if i.address<0x8126e]
assert rows
j=rows[-1];k=ins.index(j)
button=ins[k-6:k+1]
assert [i.mnemonic for i in button]==['mov','sub','cmp','jbe','jmp','shl','jmp']
assert button[1].operands[1].imm==19300 and button[2].operands[1].imm==3
assert button[5].operands[1].imm==1
assert button[-1].operands[0].mem.disp==0x327
first=rows[0];f=ins.index(first)
assert ins[f-8].mnemonic=='mov' and ins[f-8].operands[1].imm==19
assert ins[f-7].mnemonic=='mov' and ins[f-7].operands[1].imm==0x32f
assert [i.mnemonic for i in ins[f-6:f+1]]==['mov','cmp','je','add','loop','jmp','jmp']
assert ins[f-3].operands[1].imm==2 and first.operands[0].mem.disp==0x26
targets=[0x81130+struct.unpack_from('<H',b,0x81457+2*n)[0] for n in range(4)]
assert targets==[0x8126e,0x81324,0x81391,0x813ee]
r={'jumpSite':j.address,'tableStart':0x81457,'count':4,'stride':2,'targets':targets,'context':[{'site':i.address,'mnemonic':i.mnemonic,'operands':i.op_str} for i in ins[max(0,k-10):k+1]],'otherDispatches':[{'site':v.address,'context':[{'site':i.address,'mnemonic':i.mnemonic,'operands':i.op_str} for i in ins[max(0,ins.index(v)-10):ins.index(v)+1]]} for v in rows[:-1]]}
assert len(rows)==2
first_targets=[0x81130+struct.unpack_from('<H',b,0x81485+2*n)[0] for n in range(19)]
assert all(0x81130<=v<0x81457 for v in first_targets)
r['firstTable']={'jumpSite':rows[0].address,'selectorStart':0x8145f,'tableStart':0x81485,'count':19,'stride':2,'targets':first_targets}
(d/'start-dispatch-source-reading.json').write_text(json.dumps(r,indent=2))
print(json.dumps(r))
