import json
from pathlib import Path
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
p=Path(GAME_DIR+'/analysis/reporter-audit/issue5-poll-sound-producers100')
r=json.loads((p/'receiver-trace.report.json').read_text())
for idx,path in enumerate(r['paths']):
 rows=[]
 for e in path['events']:
  if e['kind']=='write' and e.get('entry')==247280 and e.get('width') in [2,4]:
   rows.append({k:e.get(k) for k in ['site','width','segment','offset','location','value']})
 print(json.dumps({'path':idx,'writes':rows}))