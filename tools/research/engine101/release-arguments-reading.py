from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import json
b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();m=Cs(CS_ARCH_X86,CS_MODE_16)
r={}
for start,end in [(93532,93710),(93710,94018)]:
 r[str(start)]=[{'site':i.address,'size':i.size,'mnemonic':i.mnemonic,'operands':i.op_str} for i in m.disasm(b[start:end],start)]
d=Path('UserContent/analysis/reporter-audit/release-arguments101');d.mkdir(parents=True,exist_ok=True);(d/'caller-contexts.json').write_text(json.dumps(r,indent=2));print(json.dumps(r))
