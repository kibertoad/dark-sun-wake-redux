from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import json
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();m=Cs(CS_ARCH_X86,CS_MODE_16)
r=[{'site':i.address,'size':i.size,'mnemonic':i.mnemonic,'operands':i.op_str} for i in m.disasm(b[0x16a75:0x16a86],0x16a75)]
Path(GAME_DIR+'/analysis/reporter-audit/shared-setter101/setter-body.json').write_text(json.dumps(r,indent=2));print(json.dumps(r))
