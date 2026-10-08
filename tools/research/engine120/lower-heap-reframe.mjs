// Rerun the recorded lower-heap acquisition/growth query (#302 consumer case) on engine 12.0.0.
// Reads and writes only private reports under UserContent; prints neutral summaries.
import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { resolve } from 'node:path';
import { run } from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';

process.env.EVIDENCE_PYTHON = resolve('artifacts/evidence-python/Scripts/python.exe');
const src = resolve('UserContent/analysis/reporter-audit/lower-heap101');
const out = resolve('UserContent/analysis/reporter-audit/lower-heap120');
mkdirSync(out, { recursive: true });
const c = JSON.parse(readFileSync(`${src}/connected-controls.json`));
const SHIFT_RETF = 22393;

const variants = [
  ['connected-controls', c],
  ['omit-shift-helper', { ...c, regions: c.regions.filter((r) => r.name !== 'segment-shift') }],
  ['one-step', { ...c, maxSteps: 1 }],
];
for (const [name, q] of variants) {
  const f = `${out}/${name}.json`;
  writeFileSync(f, JSON.stringify(q));
  const r = run(['trace', f]);
  writeFileSync(`${out}/${name}.report.json`, JSON.stringify(r));
  const retf = r.paths.flatMap((p) => p.events.filter((e) => e.kind === 'return' && e.site === SHIFT_RETF).map((e) => e.returnCheck));
  console.log(JSON.stringify({
    name,
    complete: r.completeWithinModel,
    paths: r.paths.length,
    returned: r.paths.filter((p) => p.returned).length,
    shiftRetfFollowed: retf.length,
    shiftRetfChecks: [...new Set(retf.map((x) => JSON.stringify(x)))].map((s) => JSON.parse(s)),
    stillStoppedAtShiftRetf: r.paths.filter((p) => p.stopSite === SHIFT_RETF).length,
    controls: r.relationalControls.controls.map((x) => ({ name: x.name, whole: x.verdict, occurrences: x.occurrences,
      local: [...new Set(x.paths.flatMap((p) => p.occurrences).map((o) => o.verdict))] })),
    stops: [...new Set(r.paths.filter((p) => !p.returned).map((p) => `${p.stop} at ${p.stopSite}`))],
    gaps: [...new Set(r.gaps.map((g) => g.reason))],
  }));
}
