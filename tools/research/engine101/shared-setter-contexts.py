from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import json
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();m=Cs(CS_ARCH_X86,CS_MODE_16)
p=Path(GAME_DIR+'/analysis/reporter-audit/shared-setter101');r=json.loads((p/'incoming.json').read_text());out=[]
for x in r['matches']:
 site=int(x['callSite'],16);out.append({'call':site,'context':[{'site':i.address,'size':i.size,'mnemonic':i.mnemonic,'operands':i.op_str} for i in m.disasm(b[site-36:site+10],site-36)]})
(p/'setter-candidate-contexts.json').write_text(json.dumps(out,indent=2));print(json.dumps(out))
