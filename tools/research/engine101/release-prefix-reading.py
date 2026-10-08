from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_16
import json
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes()
m=Cs(CS_ARCH_X86,CS_MODE_16);m.detail=True
r=[{'site':i.address,'size':i.size,'mnemonic':i.mnemonic,'operands':i.op_str} for i in m.disasm(b[91236:91540],91236)]
d=Path(GAME_DIR+'/analysis/reporter-audit/release-prefix101');d.mkdir(parents=True,exist_ok=True)
(d/'prefix-reading.json').write_text(json.dumps(r,indent=2))
print(json.dumps(r))
