from pathlib import Path
import bisect,json,xxhash
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes()
assert xxhash.xxh3_128_hexdigest(b)=='e296af55ba2ecde7e77f555c90f33d0b'
starts=sorted(int(l.split('\t')[0].split('+')[1],16) for l in Path('coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv').read_text().splitlines() if l.startswith('DSUN.EXE+'))
d=Path(GAME_DIR+'/analysis/reporter-audit/issue5-transfer-field-incoming101')
m=Cs(CS_ARCH_X86,CS_MODE_16);m.detail=True
out=[]
for site in [0x7df32,0x812e5,0x1cd08,0x283c1,0x2874f,0x2c0a7]:
 k=bisect.bisect_right(starts,site)-1;s,e=starts[k:k+2]
 ins=list(m.disasm(b[s:e],s));matched=[i for i in ins if i.address==site]
 out.append({'site':site,'start':s,'end':e,'linearCallCandidate':bool(matched and matched[0].mnemonic=='lcall'),'context':[{'site':i.address,'mnemonic':i.mnemonic,'operands':i.op_str} for i in ins if site-20<=i.address<=site+20]})
(d/'caller-window-contexts.json').write_text(json.dumps(out,indent=2))
print(json.dumps([{k:v for k,v in r.items() if k!='context'} for r in out]))
