import hashlib,json
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();assert hashlib.sha256(b).hexdigest()=='ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c'
cs=Cs(CS_ARCH_X86,CS_MODE_16)
regions=[('pair',0x381fa,0x382af,0x4237,0xc8a),('init',0x37573,0x37597,0x4237,3),('fill-wrapper',0x91a2,0x91c1,0x1000,0x3fa2),('fill',0x917e,0x91a2,0x1000,0x3f7e),('append',0x37616,0x3765d,0x4237,0xa6),('copy-eight',0x35998,0x359b8,0x4072,0x78),('copy',0x5652,0x566e,0x1000,0x452),('intersection',0x392ba,0x39339,0x4400,0xba),('normalize',0x3935c,0x39408,0x4400,0x15c),('bounds',0x39339,0x3935c,0x4400,0x139),('copy-region',0x375e6,0x37616,0x4237,0x76)]
out=[]
for name,start,end,seg,ip in regions:
 ins=list(cs.disasm(b[start:end],start));assert sum(i.size for i in ins)==end-start,name
 calls=[{'site':i.address,'target':i.op_str} for i in ins if i.mnemonic in ('call','lcall')]
 out.append({'name':name,'start':start,'end':end,'segment':seg,'ip':ip,'entries':[start],'calls':calls})
p=Path(GAME_DIR+'/analysis/reporter-audit/result-origin730/pair-sites.json');p.write_text(json.dumps(out));print(json.dumps([{'name':r['name'],'calls':r['calls']} for r in out]))
