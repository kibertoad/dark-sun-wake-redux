from pathlib import Path
import json,xxhash
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
from capstone.x86_const import X86_OP_MEM
p=Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-mask100');b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();assert xxhash.xxh3_128_hexdigest(b)=='e296af55ba2ecde7e77f555c90f33d0b';m=Cs(CS_ARCH_X86,CS_MODE_16);m.detail=True;ins=list(m.disasm(b[87262:88147],87262));rows=[]
for k,i in enumerate(ins):
 if any(o.type==X86_OP_MEM and o.mem.disp==0x108c for o in i.operands):
  rows.append({'site':i.address,'accesses':[{'access':o.access,'width':o.size,'segment':m.reg_name(o.mem.segment),'base':m.reg_name(o.mem.base)} for o in i.operands if o.type==X86_OP_MEM and o.mem.disp==0x108c],'context':[{'site':v.address,'mnemonic':v.mnemonic,'operands':v.op_str} for v in ins[max(0,k-3):k+3]]})
(p/'mask-scratch-source-reading.json').write_text(json.dumps(rows,indent=2));print(json.dumps(rows))
