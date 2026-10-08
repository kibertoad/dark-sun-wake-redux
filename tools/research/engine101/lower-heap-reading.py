from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import json
b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();c=Cs(CS_ARCH_X86,CS_MODE_16)
p=Path('UserContent/analysis/reporter-audit/lower-heap101');p.mkdir(parents=True,exist_ok=True)
r={str(a):[{'site':i.address,'size':i.size,'mnemonic':i.mnemonic,'operands':i.op_str} for i in c.disasm(b[a:z],a)] for a,z in [(0x663b,0x6664),(0x66c4,0x6728),(0x6728,0x6782),(0x6782,0x67a5),(0x6a31,0x6abc),(0x5762,0x5783),(0x580b,0x583a),(0x584d,0x586b),(0x59f0,0x5a12),(0x697c,0x69f2),(0x7b8e,0x7baa)]}
(p/'bounded-reading.json').write_text(json.dumps(r,indent=2),encoding='utf-8');print(json.dumps({k:{'last':v[-4:],'calls':[i for i in v if i['mnemonic'] in ['call','lcall','int']]} for k,v in r.items()}))
