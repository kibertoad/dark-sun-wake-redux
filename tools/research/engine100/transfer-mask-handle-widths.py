import json
from pathlib import Path
p=Path('C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-mask100');rows=json.loads((p/'wrapper-validator-slot-reading.json').read_text());t={r['site']:r for r in rows}
assert t[155960]['operands']=='di, word ptr [bp + 8]'
assert t[155993]['operands']=='di' and t[155994]['operands']=='si'
assert t[155963]['operands']=='si, si' and t[155967]['operands']=='di, di'
assert t[155978]['operands']=='al, al' and t[155989]['operands']=='al, al'
assert not(t[155978]['operands']=='ax, ax' or t[155989]['operands']=='ax, ax')
out={'secondArgumentRoute':'wrapper second word -> DI -> primitive second stacked word','handleChecks':'signed word','validatorPredicate':'AL only; full-AX alternative rejected','conditions':['Intervening callee and saved-frame preservation remain conditions.','Coordinate acceptance does not bound the handle index or validate slot flags/storage.']};(p/'wrapper-mask-handle-width-controls.json').write_text(json.dumps(out,indent=2));print(json.dumps(out))
