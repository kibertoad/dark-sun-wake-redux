from pathlib import Path
import json,bisect
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
p=Path(GAME_DIR+'/analysis/reporter-audit/issue5-callback-reference-forms100');c=json.loads((p/'reference-form-census.json').read_text());b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();starts=[int(l.split('\t')[0].split('+')[1],16) for l in Path('coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv').read_text().splitlines() if l.startswith('DSUN.EXE+')];m=Cs(CS_ARCH_X86,CS_MODE_16);rows=[]
for site in c['writerOffsetWordCandidates']:
 k=bisect.bisect_right(starts,site)-1
 if k<0 or k+1>=len(starts):continue
 s,e=starts[k:k+2];ins=list(m.disasm(b[s:e],s));matches=[(n,i) for n,i in enumerate(ins) if i.address<=site<i.address+i.size]
 row={'site':site,'inventoryWindow':[s,e],'decoded':False,'qualification':'Linear window context only; no function-boundary or native reference claim'}
 if matches:
  n,i=matches[0];row.update(decoded=True,context=[{'site':v.address,'mnemonic':v.mnemonic,'operands':v.op_str} for v in ins[max(0,n-3):n+3]])
 rows.append(row)
(p/'offset-word-candidate-contexts.json').write_text(json.dumps(rows,indent=2));print(json.dumps(rows))
