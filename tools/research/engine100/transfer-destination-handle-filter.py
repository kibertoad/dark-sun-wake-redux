from pathlib import Path
import json,bisect
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
from capstone.x86_const import X86_OP_MEM
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
p=Path(GAME_DIR+'/analysis/reporter-audit/issue5-transfer-mask100');b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();starts=[int(l.split('\t')[0].split('+')[1],16) for l in Path('coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv').read_text().splitlines() if l.startswith('DSUN.EXE+')];sites=json.loads((p/'destination-handle-literal-candidates.json').read_text())['sites'];m=Cs(CS_ARCH_X86,CS_MODE_16);m.detail=True;out=[]
for c in sites:
 k=bisect.bisect_right(starts,c)-1
 if k<0 or k+1>=len(starts):continue
 s,e=starts[k:k+2];ins=list(m.disasm(b[s:e],s))
 for n,i in enumerate(ins):
  if i.address<=c<i.address+i.size:
   matches=[o for o in i.operands if o.type==X86_OP_MEM and o.mem.disp==0x1400]
   if matches:out.append({'literalSite':c,'instructionSite':i.address,'window':[s,e],'operands':i.op_str,'access':[o.access for o in matches],'width':[o.size for o in matches],'qualification':'Linear candidate only; no native DS or entry-CFG ownership claim.'})
(p/'destination-handle-decoded-candidates.json').write_text(json.dumps(out,indent=2));print(json.dumps(out))
