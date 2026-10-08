import json
from pathlib import Path
from capstone import Cs,CS_ARCH_X86,CS_MODE_16
from capstone.x86_const import X86_OP_MEM,X86_OP_IMM,X86_OP_REG
import xxhash
p=Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-poll-cfg100');md=Cs(CS_ARCH_X86,CS_MODE_16);md.detail=True
cfgs={};reports={}
for root in [558273,558369,241092]:
 c=json.loads((p/f'{root}.json').read_text());r=json.loads((p/f'{root}.report.json').read_text());data=Path(c['source']).read_bytes();assert xxhash.xxh3_128_hexdigest(data)==c['xxh3'];a=c['regions'][0]
 code=list(md.disasm(data[a['start']:a['end']],a['ip']));assert sum(i.size for i in code)==a['end']-a['start'];assert r['complete'] and not r['gaps'] and not r['holes'];cfgs[root]=(a,code);reports[root]=r
checks=[]
def reg(i,o,name):return i.operands[o].type==X86_OP_REG and i.reg_name(i.operands[o].reg)==name
def imm(i,o,value):return i.operands[o].type==X86_OP_IMM and i.operands[o].imm==value
for root,call in [(558273,558333),(558369,558689)]:
 a,code=cfgs[root];sites=[root+i.address-a['ip'] for i in code];k=sites.index(call);v=code[k-5:k]
 assert [i.mnemonic for i in v]==['push','lea','push','push','push'];assert reg(v[0],0,'ss') and reg(v[3],0,'ss')
 assert reg(v[1],0,'ax') and v[1].operands[1].type==X86_OP_MEM and v[1].reg_name(v[1].operands[1].mem.base)=='bp' and v[1].operands[1].mem.disp==-2
 assert reg(v[2],0,'ax') and reg(v[4],0,'ax')
 assert code[0].mnemonic=='push' and reg(code[0],0,'bp');assert code[1].mnemonic=='mov' and reg(code[1],0,'bp') and reg(code[1],1,'sp')
 assert code[2].mnemonic=='sub' and reg(code[2],0,'sp') and code[2].operands[1].imm>=2
 n=code[k+1:k+4];assert [i.mnemonic for i in n]==['add','test','jne'];assert reg(n[0],0,'sp') and imm(n[0],1,8);assert reg(n[1],0,'ax') and imm(n[1],1,1)
 target=root+n[2].operands[0].imm-a['ip'];assert target==sites[k-5]
 assert any(x['site']==call and x['target']==241092 for x in reports[root]['calls'])
 checks.append({'root':root,'call':call,'scratchAllocatedAtEntry':True,'outputSegments':'same current SS','outputOffsets':'same current BP minus two','backEdgeReformsBothArguments':True,'predicate':'AX bit zero, not scratch memory','cleanupBytes':8,'calleeFramePreservation':'conditional, including prior callees and interrupt'})
_,w=cfgs[241092];sites=[241092+i.address-cfgs[241092][0]['ip'] for i in w]
assert len([i for i in w if i.mnemonic=='int'])==1
loads=[(j,i) for j,i in enumerate(w) if i.mnemonic=='les'];assert len(loads)==2
for (j,i),displacement,producer in zip(loads,[6,10],['cx','dx']):
 assert reg(i,0,'di') and i.operands[1].type==X86_OP_MEM and i.reg_name(i.operands[1].mem.base)=='bp' and i.operands[1].mem.disp==displacement
 n=w[j+1];assert n.mnemonic=='mov' and n.operands[0].type==X86_OP_MEM and n.operands[0].size==2 and n.reg_name(n.operands[0].mem.segment)=='es' and n.reg_name(n.operands[0].mem.base)=='di' and reg(n,1,producer)
assert any(i.mnemonic=='mov' and reg(i,0,'ax') and reg(i,1,'bx') for i in w)
assert not any(i.mnemonic.startswith('j') or i.mnemonic.startswith('loop') or i.mnemonic in ['call','lcall'] for i in w)
summary={'sourceIdentity':reports[241092]['sourceIdentity'],'callers':checks,'wrapper':{'orderedOutputProducers':['cx','dx'],'registerReturnProducer':'bx','callerBPStoredAndRestored':w[0].mnemonic=='push' and w[-2].mnemonic=='pop' and reg(w[-2],0,'bp'),'interrupt':'external condition','ownBranches':False},'scope':'complete local instruction/argument/back-edge structure; prior callee and interrupt preservation, valid storage and native result sequences remain unverified'}
(p/'alias-backedge-summary.json').write_text(json.dumps(summary));print(json.dumps(summary))
print('Both documented caller argument/back-edge structures and actual wrapper ordering verified; no finite driver sequence or whole symbolic pass claimed.')