from pathlib import Path
import json,xxhash
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
from capstone.x86_const import X86_OP_IMM
d=Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-release-callers101')
r=json.loads((d/'direct-release-producer-reading.json').read_text());b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();assert xxhash.xxh3_128_hexdigest(b)=='e296af55ba2ecde7e77f555c90f33d0b'
m=Cs(CS_ARCH_X86,CS_MODE_16);m.detail=True;ins=list(m.disasm(b[91236:92484],91236));assert sum(i.size for i in ins)==1248
edges={}
for i in ins:
 n=i.address+i.size
 if i.mnemonic.startswith('ret'):v=[]
 elif i.mnemonic=='jmp':assert i.operands[0].type==X86_OP_IMM;v=[i.operands[0].imm]
 elif i.mnemonic.startswith('j') or i.mnemonic.startswith('loop'):assert i.operands[0].type==X86_OP_IMM;v=[i.operands[0].imm,n]
 else:v=[n]
 edges[i.address]=v
assert all(t in edges for v in edges.values() for t in v)
def reachable(start,removed=None):
 q=[start];seen=set()
 while q:
  x=q.pop()
  if x==removed or x in seen:continue
  seen.add(x);q.extend(edges[x])
 return seen
selection=91371;restore=92481
sites=[x['site'] for x in r['ownFieldWriters']]+[x['pushSite'] for x in r['argumentFields']]
assert all(x in reachable(91236) and x not in reachable(91236,selection) for x in sites)
assert not any(x in reachable(restore) for x in sites)
assert [x['site'] for x in r['ownDSWrites']]==[selection,restore]
out={'ownDSSelectionDominatesWritesAndArgumentLoads':True,'ownDSRestoreDoesNotReachThem':True,'conditionalOn':['All local callees return; this CFG does not establish their DS, stack or memory effects.','Dynamic aliases and current native table/handle state remain unverified.']};(d/'direct-release-producer-placement.json').write_text(json.dumps(out,indent=2));print(json.dumps(out))
