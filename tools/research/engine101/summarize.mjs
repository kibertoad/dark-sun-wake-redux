import {readFileSync} from 'node:fs';
const d='C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-transfer-mask101';
for(const n of ['actual-mask-prefix','without-index-mask','one-step']) {
 const r=JSON.parse(readFileSync(`${d}/${n}.report.json`));
 console.log(JSON.stringify({name:n,complete:r.completeWithinModel,controls:r.relationalControls.controls.map(c=>({name:c.name,whole:c.verdict,occurrences:c.occurrences,local:c.paths.flatMap(p=>p.occurrences.map(o=>o.verdict))}))}));
}
