from pathlib import Path
import json
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
p=Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-poll-state-incoming100')
b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();md=Cs(CS_ARCH_X86,CS_MODE_16);md.detail=True
rows=[{'site':i.address,'mnemonic':i.mnemonic,'operands':i.op_str} for i in md.disasm(b[0x3b487:0x3b521],0x3b487)]
(p/'state-consumer-instruction-reading.json').write_text(json.dumps(rows,indent=2))
for row in rows: print(json.dumps(row))
