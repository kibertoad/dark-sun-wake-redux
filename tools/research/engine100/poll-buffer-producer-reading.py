from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import json
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
p=Path(GAME_DIR+'/analysis/reporter-audit/issue5-poll-state-incoming100')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();md=Cs(CS_ARCH_X86,CS_MODE_16)
for start,end in [(0x22212,0x222ea),(0x222ea,0x2247d)]:
 rows=[{'site':i.address,'mnemonic':i.mnemonic,'operands':i.op_str} for i in md.disasm(b[start:end],start)]
 (p/f'buffer-producer-{start}.json').write_text(json.dumps(rows,indent=2))
 print(json.dumps({'entry':start,'rows':[r for r in rows if r['site']>= (139900 if start==139794 else 140073) and r['site']< (139976 if start==139794 else 140269)]}))
