// Wholly synthetic #302 controls against the installed engine 12.0.0 / reader 2.2.0.
import { writeFileSync, mkdirSync } from 'node:fs';
import { resolve } from 'node:path';
import assert from 'node:assert/strict';
import { run, sourceXxh3 } from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';

process.env.EVIDENCE_PYTHON = resolve('artifacts/evidence-python/Scripts/python.exe');
const d = resolve('artifacts/engine120/near-far-reframe');
mkdirSync(d, { recursive: true });

const cases = [
  // name, bytes, helper entry, expected stop (null = returns)
  ['converted-near', [0xe8, 1, 0, 0xcb, 0x58, 0x0e, 0x50, 0xcb], 4, null],
  ['wrong-segment-conversion', [0xe8, 1, 0, 0xcb, 0x58, 0x6a, 0, 0x50, 0xcb], 4, 'far return segment changed'],
  ['incomplete-conversion', [0xe8, 1, 0, 0xcb, 0x58, 0x0e, 0xcb], 4, 'stop'],
  ['ordinary-near', [0xe8, 1, 0, 0xcb, 0xc3], 4, null],
  ['caller-far-frame', [0x0e, 0xe8, 1, 0, 0xcb, 0xcb], 5, null],
];

for (const [name, bytes, helper, expected] of cases) {
  const b = Buffer.from(bytes);
  const source = `${d}/${name}.bin`;
  writeFileSync(source, b);
  const c = {
    source, sourceKind: 'synthetic-raw', xxh3: sourceXxh3(b), entry: 0, returnBytes: 4,
    regions: [{ name: 'synthetic', start: 0, end: b.length, segment: 0x1000, ip: 0, entries: [0, helper],
      evidence: 'wholly synthetic near-call frame conversion versus ordinary and caller-built controls' }],
    maxSteps: 12, maxPaths: 2,
  };
  const f = `${d}/${name}.json`;
  writeFileSync(f, JSON.stringify(c));
  const r = run(['returns', f]);
  writeFileSync(`${d}/${name}.report.json`, JSON.stringify(r, null, 1));
  const p = r.paths[0];
  const checks = p.events.filter((e) => e.kind === 'return').map((e) => ({ site: e.site, check: e.returnCheck }));
  console.log(JSON.stringify({ name, complete: r.completeWithinModel, returned: p.returned, stop: p.stop, ax: p.registers?.AX ?? p.state?.AX, checks }));
  if (expected === null) assert(r.completeWithinModel && p.returned, name);
  else {
    assert(!r.completeWithinModel && !p.returned, name);
    if (expected !== 'stop') assert.equal(p.stop, expected, name);
  }
}
console.log('all synthetic #302 controls behave as expected');
