from pathlib import Path
import json
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
from capstone.x86_const import X86_OP_MEM
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
p=Path(GAME_DIR+'/analysis/reporter-audit/issue5-transfer-mask100');b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();m=Cs(CS_ARCH_X86,CS_MODE_16);m.detail=True;ins=list(m.disasm(b[87262:88147],87262));rows=[]
for k,i in enumerate(ins):
 _,writes=i.regs_access()
 if any(m.reg_name(r)=='ds' for r in writes) or any(o.type==X86_OP_MEM and o.mem.disp==0xe2c for o in i.operands):
  rows.append({'site':i.address,'DSWriter':any(m.reg_name(r)=='ds' for r in writes),'context':[{'site':v.address,'mnemonic':v.mnemonic,'operands':v.op_str} for v in ins[max(0,k-3):k+3]]})
(p/'mask-scratch-segment-reading.json').write_text(json.dumps(rows,indent=2));print(json.dumps(rows))
