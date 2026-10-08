from pathlib import Path
import hashlib,json
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();assert hashlib.sha256(b).hexdigest()=='ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c'
c=Cs(CS_ARCH_X86,CS_MODE_16);ins=list(c.disasm(b[0x263c2:0x26434],0x263c2));assert sum(i.size for i in ins)==0x72
calls=[i.address for i in ins if i.mnemonic in ('call','lcall')];si=[i for i in ins if i.mnemonic=='mov' and i.op_str.startswith('si,')][0];entry=si.address+si.size
out={'calls':calls,'entry':entry,'stores':[(i.address,i.op_str.split(',')[0]) for i in ins if i.mnemonic=='mov' and '[' in i.op_str.split(',')[0]],'branches':[(i.address,i.mnemonic) for i in ins if i.mnemonic.startswith('j')]}
Path(GAME_DIR+'/analysis/reporter-audit/result-origin730/path-append-sites.json').write_text(json.dumps(out));print(json.dumps(out))
