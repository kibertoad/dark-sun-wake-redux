from pathlib import Path
import json
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
p=Path(GAME_DIR+'/analysis/reporter-audit/issue5-poll-state-incoming100')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();md=Cs(CS_ARCH_X86,CS_MODE_16);md.detail=True
rows=[{'site':i.address,'mnemonic':i.mnemonic,'operands':i.op_str} for i in md.disasm(b[0x3b487:0x3b521],0x3b487)]
(p/'state-consumer-instruction-reading.json').write_text(json.dumps(rows,indent=2))
for row in rows: print(json.dumps(row))
