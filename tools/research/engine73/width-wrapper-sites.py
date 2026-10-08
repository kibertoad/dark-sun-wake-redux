from pathlib import Path
p=Path('docs/IMPLEMENTATION-PLAN.md');s=p.read_text(encoding='utf-8');a=s.index('## Output cardinality');z=s.index('This file says',a);s=s[:a]+'''## Overlapping byte/word and normalized-wrapper tooling acceptance

- **Outcome.** Retain complete word-guard intervals and neighboring-byte provenance through the actual replacement wrapper's normalized result, without treating cleared byte flags or AX zero as performed service work.
- **Evidence.** FND-CONFIG-187/188/189 are read-only acceptance inputs. Published reader 2.1.0/engine 7.3.0 retain read/write intervals, byte producers and conditional call scopes.
- **Acceptance.** Trace the complete replacement wrapper and both actual service bodies with explicit conditional call returns and register/frame/field scopes. Unknown input bytes, indirect targets, stopped callees and capped paths remain unknown; scope preservation is a hypothesis. No seeded entry memory, selected branch outcomes, stitched windows, spec/parity changes or original execution.
- **Tests and Exit.** Verify both two-byte guards and their missing byte producers remain visible on normalized returning paths; retain modeled failure results and wrapper's own zero. Compare scoped/unscoped calls and unread/capped controls. Full Gap 36 also needs real byte/high-byte producers and complete fixture acceptance. Run the canonical gate before commit.

'''+s[z:];p.write_text(s,encoding='utf-8')
import hashlib,json
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();assert hashlib.sha256(b).hexdigest()=='ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c'
c=Cs(CS_ARCH_X86,CS_MODE_16);out=[]
for name,start,end,seg,ip in [('replacement',0x33c0d,0x33c85,0x3d72,0x12ed),('before-service',0x334a4,0x336a3,0x3d72,0xb84),('after-service',0x33262,0x3347f,0x3d72,0x942)]:
 ins=list(c.disasm(b[start:end],start));assert sum(i.size for i in ins)==end-start,name
 out.append({'name':name,'start':start,'end':end,'segment':seg,'ip':ip,'entries':[start],'calls':[{'site':i.address,'target':i.op_str} for i in ins if i.mnemonic in ('call','lcall')],'guards':[i.address for i in ins if '0x332c' in i.op_str or '0x332e' in i.op_str]})
Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/result-origin730/width-wrapper-sites.json').write_text(json.dumps(out));print(json.dumps([{'name':r['name'],'calls':r['calls'],'guards':r['guards']} for r in out]))
