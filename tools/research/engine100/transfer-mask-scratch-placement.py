from pathlib import Path
from collections import deque
import json,xxhash
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
p=Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-mask100');b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();assert xxhash.xxh3_128_hexdigest(b)=='e296af55ba2ecde7e77f555c90f33d0b';m=Cs(CS_ARCH_X86,CS_MODE_16);m.detail=True;ins=list(m.disasm(b[87262:88147],87262));table={i.address:i for i in ins};assert sum(i.size for i in ins)==885
edges={}
for i in ins:
 f=i.address+i.size
 if i.mnemonic in ['ret','retf','iret']:v=[]
 elif i.mnemonic=='jmp':v=[i.operands[0].imm]
 elif i.mnemonic.startswith('j') or i.mnemonic.startswith('loop'):v=[i.operands[0].imm,f]
 else:v=[f]
 edges[i.address]=[n for n in v if n in table]
def reach(start,target,removed=set()):
 q=deque([start]);seen=set()
 while q:
  n=q.popleft()
  if n in removed or n in seen:continue
  if n==target:return True
  seen.add(n);q.extend(edges[n])
 return False
writers=[87269,87433,87452,87456,87682,87689,87985,87990]
assert reach(87262,87993)
required={str(n):not reach(87262,87993,{n}) for n in writers};assert all(required.values())
DSwriters=[]
for i in ins:
 _,ws=i.regs_access()
 if any(m.reg_name(r)=='ds' for r in ws):DSwriters.append(i.address)
DS_before_scratch=[n for n in DSwriters if reach(87262,n) and reach(n,87689)]
assert DS_before_scratch==[87269]
assert table[87268].mnemonic=='push' and table[87268].op_str=='cs'
assert table[87269].mnemonic=='pop' and table[87269].op_str=='ds'
assert not any(i.mnemonic in ['call','lcall','int'] for i in ins)
wrong_source=table[87433].op_str=='bx, word ptr [bp + 6]';assert not wrong_source
out={'maskReadReachable':True,'requiredSourceInstructions':required,'ownDSAssignmentsBeforeScratchStore':DS_before_scratch,'wrongFirstHandleReadingRejected':not wrong_source,'conditions':['Full-entry CFG with port and repeat-instruction continuation assumptions.','No native handle, slot or memory values supplied.','Dominating own stores are not complete last-writer coverage: transfer ranges may alias shared metadata.','Stack/frame and dynamic slot-index validity remain unverified.','Hardware output and source mask-table contents remain unverified.']}
(p/'mask-scratch-placement-controls.json').write_text(json.dumps(out,indent=2));print(json.dumps(out))
