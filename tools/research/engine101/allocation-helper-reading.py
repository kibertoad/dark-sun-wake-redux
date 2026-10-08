from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import json
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();c=Cs(CS_ARCH_X86,CS_MODE_16)
p=Path(GAME_DIR+'/analysis/reporter-audit/allocation-chain101')
r={str(a):[{'site':i.address,'size':i.size,'mnemonic':i.mnemonic,'operands':i.op_str} for i in c.disasm(b[a:z],a)] for a,z in [(0x6abc,0x6ad0),(0x5783,0x57c7)]}
(p/'helper-reading.json').write_text(json.dumps(r,indent=2),encoding='utf-8');print(json.dumps(r))
