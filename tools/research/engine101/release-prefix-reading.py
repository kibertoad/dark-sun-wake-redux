from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_16
import json
b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes()
m=Cs(CS_ARCH_X86,CS_MODE_16);m.detail=True
r=[{'site':i.address,'size':i.size,'mnemonic':i.mnemonic,'operands':i.op_str} for i in m.disasm(b[91236:91540],91236)]
d=Path('UserContent/analysis/reporter-audit/release-prefix101');d.mkdir(parents=True,exist_ok=True)
(d/'prefix-reading.json').write_text(json.dumps(r,indent=2))
print(json.dumps(r))
