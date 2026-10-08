import {readFileSync} from 'node:fs';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
const d=GAME_DIR+'/analysis/reporter-audit/issue5-transfer-mask101';
for(const n of ['actual-mask-prefix','without-index-mask','one-step']) {
 const r=JSON.parse(readFileSync(`${d}/${n}.report.json`));
 console.log(JSON.stringify({name:n,complete:r.completeWithinModel,controls:r.relationalControls.controls.map(c=>({name:c.name,whole:c.verdict,occurrences:c.occurrences,local:c.paths.flatMap(p=>p.occurrences.map(o=>o.verdict))}))}));
}
