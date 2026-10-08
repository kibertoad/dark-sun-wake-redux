import json
from pathlib import Path
from collections import deque
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
from capstone.x86_const import X86_OP_IMM
import xxhash
base=Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-boundaries100')
c=json.loads((base/'documented-primitive.json').read_text());r=json.loads((base/'documented-primitive.report.json').read_text())
data=Path(c['source']).read_bytes();assert xxhash.xxh3_128_hexdigest(data)==c['xxh3']
start,end=0x154de,0x15853;ip=0x43ae;decoder=Cs(CS_ARCH_X86,CS_MODE_16);decoder.detail=True
ins=list(decoder.disasm(data[start:end],ip));assert sum(x.size for x in ins)==end-start
edges={};ports=[];returns=[]
for x in ins:
 s=start+x.address-ip;n=s+x.size;m=x.mnemonic
 if m.startswith('ret'): edges[s]=[];returns.append(s)
 elif m.startswith('j') or m.startswith('loop'):
  assert x.operands[0].type==X86_OP_IMM
  target=start+x.operands[0].imm-ip;edges[s]=[target] if m=='jmp' else [target,n]
 else:edges[s]=[n]
 if m in ('in','out'):ports.append(s)
assert ports==[x['site'] for x in r['hardwareBoundaries']]
assert r['complete'] and not r['gaps'] and not r['calls'] and not r['holes']
assert returns==[end-1]
assert all(t in edges for ts in edges.values() for t in ts)
def reachable(omit=None):
 seen=set();todo=[start]
 while todo:
  s=todo.pop()
  if s==omit or s in seen:continue
  seen.add(s);todo.extend(edges[s])
 return seen
seen=reachable();assert len(seen)==r['instructionCount']
placement=[{'site':s,'kind':next(x['boundary'] for x in r['hardwareBoundaries'] if x['site']==s),'returnWithoutSite':any(t in reachable(s) for t in returns)} for s in ports]
assert any(not x['returnWithoutSite'] for x in placement) and any(x['returnWithoutSite'] for x in placement)
for name in ['one-instruction','before-common-transfer']:
 n=json.loads((base/f'{name}.report.json').read_text());assert not n['complete'];assert not n['hardwareBoundaries']
summary={'sourceIdentity':r['sourceIdentity'],'completeLocalCFG':True,'continuationAssumptionsExplicit':len(r['assumedContinuations'])==len(ports),'hardware':placement,'commonSites':[x['site'] for x in placement if not x['returnWithoutSite']],'conditionalSites':[x['site'] for x in placement if x['returnWithoutSite']],'nativeReachability':'unconfirmed','pixels':'unconfirmed','memorySeeds':False,'scope':'local CFG return paths assuming each port continues; no data feasibility, hardware success or complete reading claim'}
(base/'placement-summary.json').write_text(json.dumps(summary))
print(json.dumps(summary))
print('Published hardware census matches independent bounded CFG; truncated and one-instruction controls cannot prove placement.')