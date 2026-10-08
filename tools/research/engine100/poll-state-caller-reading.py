import json
from pathlib import Path
from capstone import Cs, CS_ARCH_X86, CS_MODE_16
from capstone.x86 import X86_OP_REG
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
root=Path(GAME_DIR+'/analysis/reporter-audit/issue5-poll-state-incoming100')
b=Path(GAME_DIR+'/DSUN.EXE').read_bytes()
md=Cs(CS_ARCH_X86,CS_MODE_16); md.detail=True
out=[]
for start,end,site in [(0x22212,0x222ea,0x222c8),(0x222ea,0x2247d,0x223ed)]:
 ins=list(md.disasm(b[start:end],start))
 assert sum(i.size for i in ins)==end-start
 k=next(k for k,i in enumerate(ins) if i.address==site)
 context=[{'site':i.address,'mnemonic':i.mnemonic,'operands':i.op_str} for i in ins[max(0,k-5):k+2]]
 ds_writes=[]
 for i in ins:
  _,writes=i.regs_access()
  if any(i.reg_name(r)=='ds' for r in writes): ds_writes.append(i.address)
 row={'entry':start,'callSite':site,'context':context,'ownDSWrites':ds_writes,'priorCalls':[i.address for i in ins[:k] if i.mnemonic in ['call','lcall']],'limits':['Own-instruction DS scan is not transitive callee preservation.','Relocation census excludes near, computed and unrelocated callers.','Argument offset alone does not prove initialized contents or valid DS storage.']}
 out.append(row)
(root/'caller-argument-reading.json').write_text(json.dumps(out,indent=2))
for row in out: print(json.dumps(row))
