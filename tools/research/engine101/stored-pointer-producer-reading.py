from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import json
b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();m=Cs(CS_ARCH_X86,CS_MODE_16)
r={}
for s,e in [(0x2f4f1,0x2f519),(0x18bdb,0x18be5),(0x336a3,0x33705)]:r[str(s)]=[{'site':i.address,'size':i.size,'mnemonic':i.mnemonic,'operands':i.op_str} for i in m.disasm(b[s:e],s)]
p=Path('UserContent/analysis/reporter-audit/stored-pointer-producer101');p.mkdir(parents=True,exist_ok=True);(p/'producer-contexts.json').write_text(json.dumps(r,indent=2));print(json.dumps(r))
