from pathlib import Path
import json,xxhash
p=Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-mask100');b=Path('C:/GOG Games/Dark Sun 2/DSUN.EXE').read_bytes();assert xxhash.xxh3_128_hexdigest(b)=='e296af55ba2ecde7e77f555c90f33d0b'
rows=[]
for n in range(len(b)-1):
 if b[n:n+2]==bytes([0,0x14]):rows.append(n)
(p/'destination-handle-literal-candidates.json').write_text(json.dumps({'word':0x1400,'sites':rows,'negativeUsable':False,'limits':['Raw literal match is not instruction ownership, a write or current DS identity.','Aliased, computed and cross-segment writers are not enumerated.']},indent=2));print(json.dumps({'rawLiteralCandidates':rows,'negativeUsable':False}))
