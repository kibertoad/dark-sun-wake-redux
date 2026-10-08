from pathlib import Path
import json,xxhash
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
from capstone.x86_const import X86_OP_REG,X86_OP_MEM
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();assert xxhash.xxh3_128_hexdigest(b)=='e296af55ba2ecde7e77f555c90f33d0b'
d=Path(GAME_DIR+'/analysis/reporter-audit/issue5-transfer-field-incoming101')
m=Cs(CS_ARCH_X86,CS_MODE_16);m.detail=True
out=[]
for s,e,site in [(164492,164984,164801),(164984,165896,165711),(179076,180627,180391)]:
 ins=list(m.disasm(b[s:e],s));rows=[]
 for k,i in enumerate(ins):
  if i.mnemonic in ['call','lcall'] or any(m.reg_name(r)=='ds' for r in i.regs_access()[1]):
   rows.append({'site':i.address,'mnemonic':i.mnemonic,'operands':i.op_str,'writesDS':any(m.reg_name(r)=='ds' for r in i.regs_access()[1]),'context':[{'site':v.address,'mnemonic':v.mnemonic,'operands':v.op_str} for v in ins[max(0,k-4):k+3]]})
 out.append({'start':s,'end':e,'consumerSite':site,'rows':rows,'qualification':'Linear source facts only; published entry-path ownership is checked separately.'})
(d/'consumer-parent-source-effects.json').write_text(json.dumps(out,indent=2))
print(json.dumps([{'start':r['start'],'consumerSite':r['consumerSite'],'DSwrites':[i['site'] for i in r['rows'] if i['writesDS']],'callsBeforeConsumer':[i['site'] for i in r['rows'] if i['mnemonic'] in ['call','lcall'] and i['site']<r['consumerSite']]} for r in out]))
