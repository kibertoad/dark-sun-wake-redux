from pathlib import Path
import json,bisect,xxhash
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();assert xxhash.xxh3_128_hexdigest(b)=='e296af55ba2ecde7e77f555c90f33d0b'
d=Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-release-callers101')
starts=sorted(int(l.split('\t')[0].split('+')[1],16) for l in Path('coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv').read_text().splitlines() if l.startswith('DSUN.EXE+'))
m=Cs(CS_ARCH_X86,CS_MODE_16);m.detail=True;out=[]
for r in json.loads((d/'independent-near-candidates.json').read_text())['candidates']:
 site=r['site'];k=bisect.bisect_right(starts,site)-1;s,e=starts[k:k+2]
 ins=list(m.disasm(b[s:e],s));matches=[i for i in ins if i.address==site and i.mnemonic in ['call','jmp']]
 out.append({'site':site,'start':s,'end':e,'linearAligned':bool(matches),'context':[{'site':i.address,'mnemonic':i.mnemonic,'operands':i.op_str} for i in ins if site-20<=i.address<=site+10],'qualification':'Bounded linear context is not CFG ownership or native handle admission.'})
(d/'near-caller-windows.json').write_text(json.dumps(out,indent=2));print(json.dumps([{k:v for k,v in r.items() if k not in ['context','qualification']} for r in out]))
