import json
from pathlib import Path
from collections import deque
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
from capstone.x86 import X86_OP_IMM
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
p=Path(GAME_DIR+'/analysis/reporter-audit/issue5-poll-state-incoming100');b=Path(GAME_DIR+'/DSUN.EXE').read_bytes();md=Cs(CS_ARCH_X86,CS_MODE_16);md.detail=True
out=[]
for start,end,consumer,producers in [(139794,140010,139976,{139947}),(140010,140413,140269,{140107,140147,140197})]:
 ins=list(md.disasm(b[start:end],start));assert sum(i.size for i in ins)==end-start
 table={i.address:i for i in ins};edges={}
 for i in ins:
  successors=[];fall=i.address+i.size
  if i.mnemonic in ['ret','retf','iret']: pass
  elif i.mnemonic=='jmp': successors=[i.operands[0].imm]
  elif i.mnemonic.startswith('j') or i.mnemonic.startswith('loop'): successors=[i.operands[0].imm,fall]
  else: successors=[fall]
  edges[i.address]=[v for v in successors if v in table]
 def reachable(removed):
  seen=set();q=deque([start])
  while q:
   n=q.popleft()
   if n in removed or n in seen:continue
   seen.add(n);q.extend(edges[n])
  return consumer in seen
 assert reachable(set());assert not reachable(producers)
 individual={str(n):reachable({n}) for n in producers}
 if len(producers)==3:assert all(individual.values())
 rev={n:[] for n in table}
 for n,vs in edges.items():
  for v in vs:rev[v].append(n)
 q=deque([(consumer,())]);seen=set();last=set();intervening=set()
 while q:
  n,path=q.popleft()
  if n in seen:continue
  seen.add(n)
  if n in producers:last.add(n);continue
  if n!=consumer and table[n].mnemonic in ['call','lcall']:intervening.add(n)
  q.extend((v,()) for v in rev[n])
 assert last==producers
 row={'entry':start,'consumer':consumer,'formatterSites':sorted(producers),'consumerReachable':True,'removingAllFormattersBlocksConsumer':True,'removingIndividualFormatterStillReachesConsumer':individual,'possibleLastDeclaredFormatterCalls':sorted(last),'interveningCallSites':sorted(intervening),'assumptions':['CFG branches may be infeasible at runtime.','Every call continues to its next instruction.','Last declared formatter call is not last memory writer.','Same DS-relative offset does not establish same physical storage across calls.']}
 out.append(row);print(json.dumps(row))
(p/'buffer-producer-route-controls.json').write_text(json.dumps(out,indent=2))
