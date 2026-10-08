from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import json
b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();c=Cs(CS_ARCH_X86,CS_MODE_16)
p=Path('UserContent/analysis/reporter-audit/allocation-chain101');p.mkdir(parents=True,exist_ok=True)
r={str(a):[{'site':i.address,'size':i.size,'mnemonic':i.mnemonic,'operands':i.op_str} for i in c.disasm(b[a:z],a)] for a,z in [(0x396c8,0x39752),(0x561a,0x5631),(0x6ad0,0x6b59),(0x59d9,0x59f0),(0x67a5,0x6822)]}
(p/'bounded-reading.json').write_text(json.dumps(r,indent=2),encoding='utf-8');print(json.dumps(r))
