from pathlib import Path
import json,bisect
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
p=Path(GAME_DIR+'/analysis/reporter-audit/issue5-transfer-mask100');b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();starts=[int(l.split('\t')[0].split('+')[1],16) for l in Path('coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv').read_text().splitlines() if l.startswith('DSUN.EXE+')];m=Cs(CS_ARCH_X86,CS_MODE_16);out=[]
for candidate in [118630,139044,139327]:
 k=bisect.bisect_right(starts,candidate)-1;s,e=starts[k:k+2];ins=list(m.disasm(b[s:e],s));owned=[(n,i) for n,i in enumerate(ins) if i.address<=candidate<i.address+i.size]
 for n,i in owned:out.append({'literalSite':candidate,'inventoryWindow':[s,e],'context':[{'site':v.address,'mnemonic':v.mnemonic,'operands':v.op_str} for v in ins[max(0,n-4):n+4]],'qualification':'Linear inventory-window decode only; entry CFG ownership and native CS/DS remain unverified.'})
(p/'destination-handle-first-candidate-readings.json').write_text(json.dumps(out,indent=2));print(json.dumps(out))
