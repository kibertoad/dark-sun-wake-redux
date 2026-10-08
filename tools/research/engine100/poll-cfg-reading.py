import json
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
from capstone.x86_const import X86_OP_MEM,X86_OP_IMM
import xxhash
p=Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-poll-cfg100')
md=Cs(CS_ARCH_X86,CS_MODE_16);md.detail=True
for root in [558273,558369,241092]:
 c=json.loads((p/f'{root}.json').read_text());r=json.loads((p/f'{root}.report.json').read_text());data=Path(c['source']).read_bytes();assert xxhash.xxh3_128_hexdigest(data)==c['xxh3'];region=c['regions'][0]
 code=list(md.disasm(data[region['start']:region['end']],region['ip']))
 print(json.dumps({'entry':root,'complete':r['complete'],'gaps':r['gaps'],'calls':r['calls']}))
 for i in code:
  at=root+i.address-region['ip']
  if root==241092 or at<root+14 or (root==558273 and at>558320) or (root==558369 and at>558665):print(hex(at),i.mnemonic,i.op_str)