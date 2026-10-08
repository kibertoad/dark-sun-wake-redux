import json
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
from capstone.x86_const import X86_OP_MEM
import xxhash
p=Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-poll-sound-producers100');c=json.loads((p/'receiver-trace.json').read_text());data=Path(c['source']).read_bytes();assert xxhash.xxh3_128_hexdigest(data)==c['xxh3'];needle=(0x3434).to_bytes(2,'little');matches=[];at=0
while True:
 at=data.find(needle,at)
 if at<0:break
 matches.append(at);at+=1
md=Cs(CS_ARCH_X86,CS_MODE_16);md.detail=True
windows=[('FND-CONFIG-166-status',0x3b361,0x3b487,0x51),('FND-CONFIG-166-clear',0x3b6b5,0x3b717,0x3a5),('FND-CONFIG-020-receiver',0x3c5f0,0x3c650,0xb0),('FND-CONFIG-020-initializer-prefix',0x3c673,0x3c710,0x133)]
owned=[]
for name,start,end,ip in windows:
 for i in md.disasm(data[start:end],ip):
  for o in i.operands:
   if o.type==X86_OP_MEM and o.mem.disp==0x3434:owned.append({'window':name,'site':start+i.address-ip,'width':o.size,'access':o.access,'segment':i.reg_name(o.mem.segment) or 'default DS'})
summary={'sourceIdentity':{'xxh3':c['xxh3'],'size':len(data)},'field':0x3434,'literalOccurrences':matches,'instructionOperandsInDocumentedWindows':owned,'scope':'raw literal scan covers shipped file, not all instruction starts, runtime segments or aliased writes; listed source windows only','negativeUsable':False}
(p/'state-word-literal-summary.json').write_text(json.dumps(summary));print(json.dumps(summary))