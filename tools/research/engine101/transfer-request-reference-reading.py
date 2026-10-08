from pathlib import Path
import json,xxhash
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();assert xxhash.xxh3_128_hexdigest(b)=='e296af55ba2ecde7e77f555c90f33d0b'
m=Cs(CS_ARCH_X86,CS_MODE_16);ins=list(m.disasm(b[0x1395d:0x139d9],0x1395d));out=[]
for k,i in enumerate(ins):
 if i.mnemonic=='shl' and i.op_str=='bx, 1':out.append({'shift':i.address,'after':i.address+i.size,'context':[{'site':v.address,'mnemonic':v.mnemonic,'operands':v.op_str} for v in ins[max(0,k-3):k+4]]})
d=Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-consumer-producer101');out=[{'site':v.address,'mnemonic':v.mnemonic,'operands':v.op_str} for v in ins[:28]];(d/'request-reference-source-reading.json').write_text(json.dumps(out,indent=2));print(json.dumps(out))
