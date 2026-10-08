import json
from pathlib import Path
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
base=Path(GAME_DIR+'/analysis/reporter-audit')
for name,relative in [('root','issue5-poll-callers100/aliased-caller-root-controls.report.json'),('exit-case','issue5-poll-progress100/bx-0.report.json'),('repeat-case','issue5-poll-progress100/bx-1.report.json')]:
 r=json.loads((base/relative).read_text());rows=[]
 for p in r['paths']:
  calls=[{'site':e['site'],'target':e.get('target'),'kind':e['kind'],'conditionalModel':e.get('conditionalModel')} for e in p['events'] if e['kind'] in ['call','call-return']]
  polls=[e['site'] for e in p['events'] if e.get('entry')==241092 and e['kind']=='return']
  rows.append({'path':p.get('path'), 'stop':p.get('stop'),'site':p.get('stopSite'),'returned':p.get('returned'),'pollReturns':len(polls),'lastCalls':calls[-4:]})
 print(json.dumps({'case':name,'gaps':r.get('gaps'),'paths':rows}))