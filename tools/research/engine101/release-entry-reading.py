from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import json,bisect
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();m=Cs(CS_ARCH_X86,CS_MODE_16)
starts=sorted(int(l.split('\t')[0].split('+')[1],16) for l in Path('coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv').read_text().splitlines() if l.startswith('DSUN.EXE+'))
k=bisect.bisect_right(starts,93834)-1;s,e=starts[k:k+2]
r={'ownerWindow':[s,e],'contexts':[]}
for start,end in [(91364,91380),(s,s+30),(93814,93844),(91540,91675)]:
 r['contexts'].append([{'site':i.address,'size':i.size,'mnemonic':i.mnemonic,'operands':i.op_str} for i in m.disasm(b[start:end],start)])
Path(GAME_DIR+'/analysis/reporter-audit/release-prefix101/entry-and-caller-reading.json').write_text(json.dumps(r,indent=2));print(json.dumps(r))
