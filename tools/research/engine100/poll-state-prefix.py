import json
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
p=Path(GAME_DIR+'/analysis/reporter-audit/issue5-poll-state-inputs100');c=json.loads((p/'243381.json').read_text());data=Path(c['source']).read_bytes();m=Cs(CS_ARCH_X86,CS_MODE_16)
for root in [243381,242529]:
 region=next(r for r in c['regions'] if r['start']==root)
 for i in m.disasm(data[root:region['end']],region['ip']):
  site=root+i.address-region['ip']
  if site>=root+43:break
  print(root,site,i.mnemonic,i.op_str)