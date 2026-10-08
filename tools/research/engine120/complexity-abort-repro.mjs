// Wholly synthetic: one path grows an unknown sum past the engine's term cap; its sibling returns at once.
import { writeFileSync, mkdirSync } from 'node:fs';
import { resolve } from 'node:path';
import { run, sourceXxh3 } from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';

process.env.EVIDENCE_PYTHON = resolve('artifacts/evidence-python/Scripts/python.exe');
const d = resolve('artifacts/engine120/complexity-abort');
mkdirSync(d, { recursive: true });

function source(adds) {
  const body = [];
  for (let i = 0; i < adds; i++) body.push(0x01, 0xd8); // add ax, bx
  const skip = body.length;
  return Buffer.from([
    0xa1, 0x00, 0x01,       // mov ax, [0100h]   unknown
    0x8b, 0x1e, 0x02, 0x01, // mov bx, [0102h]   unknown
    0x85, 0xc0,             // test ax, ax
    0x0f, 0x84, skip & 0xff, skip >> 8, // jz done (near, 16-bit displacement)
    ...body,
    0xcb,                   // done: retf
  ]);
}

for (const adds of [8, 600]) {
  const b = source(adds);
  const f = `${d}/adds-${adds}.bin`;
  writeFileSync(f, b);
  const c = {
    source: f, sourceKind: 'synthetic-raw', xxh3: sourceXxh3(b), entry: 0, returnBytes: 4,
    registers: { ds: 0x3000, ss: 0x9000, sp: 0xe000 },
    regions: [{ name: 'synthetic', start: 0, end: b.length, segment: 0x1000, ip: 0, entries: [0],
      evidence: 'wholly synthetic unknown sum versus an early-return sibling path' }],
    maxSteps: adds + 16, maxPaths: 2,
  };
  const q = `${d}/adds-${adds}.json`;
  writeFileSync(q, JSON.stringify(c));
  try {
    const r = run(['trace', q]);
    console.log(JSON.stringify({ adds, complete: r.completeWithinModel,
      paths: r.paths.map((p) => ({ returned: p.returned, stop: p.stop, stopSite: p.stopSite })) }));
  } catch (e) {
    console.log(JSON.stringify({ adds, error: String(e.message).trim().split('\n').pop() }));
  }
}
