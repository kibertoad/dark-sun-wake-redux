from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
import hashlib,json
b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();assert hashlib.sha256(b).hexdigest()=='ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c'
c=Cs(CS_ARCH_X86,CS_MODE_16);ins=list(c.disasm(b[0xcc7b:0xcd0e],0xcc7b));assert sum(i.size for i in ins)==0x93
# Retain instruction context locally only, emit selectors and widths.
Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/result-origin730/eviction-tail-local.txt').write_text('\n'.join(f'{i.address:x} {i.mnemonic} {i.op_str}' for i in ins[-30:]))
print(json.dumps({'tail_start':ins[-30].address,'writes':[(i.address,i.op_str.split(',')[0]) for i in ins if i.mnemonic=='mov' and '[' in i.op_str.split(',')[0]],'returns':[i.address for i in ins if i.mnemonic.startswith('ret')]}))
