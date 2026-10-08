import { readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { run } from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
process.env.EVIDENCE_PYTHON = resolve('artifacts/evidence-python/Scripts/python.exe');
const out = resolve('UserContent/analysis/reporter-audit/lower-heap120');
const c = JSON.parse(readFileSync(`${out}/steps128.json`));
const ctl = c.relationalControls.find((x) => x.name === 'request-shift-count');
const f = `${out}/wrong-shift-count-steps128.json`;
writeFileSync(f, JSON.stringify({ ...c, relationalControls: [{ ...ctl, right: 16 }] }));
try { run(['trace', f]); console.log('wrong-shift-count: ACCEPTED (unexpected)'); }
catch (e) { const m = String(e.message).trim().split('\n').pop(); writeFileSync(`${out}/wrong-shift-count-steps128.error.txt`, m); console.log('wrong-shift-count: ' + m); }
