// Narrower bounds of the recorded acquisition query; bounds only shrink. Prints neutral summaries.
import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { resolve } from 'node:path';
import { run } from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');

process.env.EVIDENCE_PYTHON = resolve('artifacts/evidence-python/Scripts/python.exe');
const src = resolve(GAME_DIR+'/analysis/reporter-audit/lower-heap101');
const out = resolve(GAME_DIR+'/analysis/reporter-audit/lower-heap120');
mkdirSync(out, { recursive: true });
const c = JSON.parse(readFileSync(`${src}/connected-controls.json`));
const SHIFT_RETF = 22393;
const noSplit = (q) => ({ ...q, regions: q.regions.filter((r) => r.name !== 'tail-split') });

const variants = [
  ['steps128', { ...c, maxSteps: 128 }],
  ['steps96', { ...c, maxSteps: 96 }],
  ['steps64', { ...c, maxSteps: 64 }],
  ['omit-tail-split', noSplit(c)],
  ['omit-tail-split-steps128', noSplit({ ...c, maxSteps: 128 })],
  ['omit-tail-split-omit-shift', { ...noSplit(c), regions: noSplit(c).regions.filter((r) => r.name !== 'segment-shift') }],
];
for (const [name, q] of variants) {
  const f = `${out}/${name}.json`;
  writeFileSync(f, JSON.stringify(q));
  let r;
  try { r = run(['trace', f]); } catch (e) { console.log(JSON.stringify({ name, error: String(e.message).trim().split('\n').pop() })); continue; }
  writeFileSync(`${out}/${name}.report.json`, JSON.stringify(r));
  const retf = r.paths.flatMap((p) => p.events.filter((e) => e.kind === 'return' && e.site === SHIFT_RETF).map((e) => e.returnCheck));
  console.log(JSON.stringify({
    name, complete: r.completeWithinModel, paths: r.paths.length, returned: r.paths.filter((p) => p.returned).length,
    shiftRetfFollowed: retf.length,
    shiftRetfChecks: [...new Set(retf.map((x) => JSON.stringify(x)))].map((s) => JSON.parse(s)),
    stillStoppedAtShiftRetf: r.paths.filter((p) => p.stopSite === SHIFT_RETF).length,
    controls: r.relationalControls.controls.map((x) => ({ name: x.name, whole: x.verdict, occ: x.occurrences,
      local: [...new Set(x.paths.flatMap((p) => p.occurrences).map((o) => o.verdict))] })),
    stops: [...new Set(r.paths.filter((p) => !p.returned).map((p) => `${p.stop} at ${p.stopSite}`))],
  }));
}
