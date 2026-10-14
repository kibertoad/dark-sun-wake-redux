#!/usr/bin/env node
// Writes docs/PROGRESS.md: the progress measures of the work protocol's "Measuring progress"
// section, computed from the repository, each with its change since the previous snapshot.
// Usage: node tools/progress-report.mjs [--date YYYY-MM-DD] [--check]
// --check prints the figures and leaves the file alone.
import { execFileSync } from 'node:child_process';
import { existsSync, readdirSync, readFileSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { parseYaml } from '../node_modules/@scientific-method/standard-checker/dist/yaml.js';

const root = resolve(fileURLToPath(import.meta.url), '../..');
const out = join(root, 'docs/PROGRESS.md');
const args = process.argv.slice(2);
const date = args.includes('--date') ? args[args.indexOf('--date') + 1] : new Date().toISOString().slice(0, 10);
const read = path => readFileSync(path, 'utf8').replace(/^﻿/, '').replaceAll('\r\n', '\n');
const markdown = dir => existsSync(dir) ? readdirSync(dir, { withFileTypes: true }).flatMap(e =>
  e.isDirectory() ? markdown(join(dir, e.name)) : e.name.endsWith('.md') ? [join(dir, e.name)] : []) : [];
const ignore = () => {};
const git = (...a) => execFileSync('git', a, { cwd: root, encoding: 'utf8' }).trim();

// Parity totals, counted from the rows in parity/ by their Status and Code columns, since
// PARITY.md is regenerated on the main branch only and can lag the rows.
const parity = {
  status: Object.fromEntries(['unknown', 'sourced', 'supported', 'established', 'disputed', 'implemented', 'deviated', 'validated'].map(k => [k, 0])),
  code: { missing: 0, partial: 0, complete: 0 },
};
for (const path of markdown(join(root, 'parity'))) {
  const lines = read(path).split('\n');
  const head = lines.find(l => l.startsWith('| Spec ID |'))?.split('|').map(c => c.trim());
  if (!head) continue;
  for (const l of lines.filter(l => /^\| `[A-Z]+-[A-Z0-9]+-\d+` \|/.test(l))) {
    const cells = l.split('|').map(c => c.trim());
    const status = cells[head.indexOf('Status')], code = cells[head.indexOf('Code')];
    parity.status[status] = (parity.status[status] ?? 0) + 1;
    parity.code[code] = (parity.code[code] ?? 0) + 1;
  }
}

// Spec entries by kind and status, superseded ones included under their status.
const entries = {};
const metas = [];
for (const path of markdown(join(root, 'spec')).filter(p => !p.includes(join('spec', 'index')))) {
  const front = read(path).match(/^---\n([\s\S]*?)\n---(?:\n|$)/)?.[1];
  if (!front) continue;
  const meta = parseYaml(front, path, ignore);
  if (typeof meta.id !== 'string' || typeof meta.status !== 'string') continue;
  const kind = meta.id.split('-')[0];
  entries[kind] ??= {};
  entries[kind][meta.status] = (entries[kind][meta.status] ?? 0) + 1;
  metas.push(meta);
}
const completeReading = metas.filter(m => Array.isArray(m.complete_reading) && m.complete_reading.length).length;

// Citation coverage per inventory, from the checker's own measure.
const coverage = JSON.parse(execFileSync(process.execPath,
  [join(root, 'node_modules/@scientific-method/standard-checker/dist/coverage.js'), '--root', root, '--json'],
  { encoding: 'utf8' }));
// Mapping coverage, from the denominator audit beside each inventory.
const mapping = {};
for (const inv of coverage.inventories) {
  const regions = join(root, inv.path.replace(/\.tsv$/, '.regions.tsv'));
  if (!existsSync(regions)) continue;
  const [head, ...rows] = read(regions).trim().split('\n').map(l => l.split('\t'));
  const measured = rows.filter(r => r[head.indexOf('kind')] === 'region');
  const sum = name => measured.reduce((n, r) => n + Number(r[head.indexOf(name)]), 0);
  // A region's bytes in bodies are its size less the bytes of each class outside them.
  const outside = sum('instructions_outside') + sum('data_outside') + sum('undefined_outside');
  mapping[inv.path] = { regions: sum('size'), bodies: sum('size') - outside, instructionsOutside: sum('instructions_outside'),
    dataOutside: sum('data_outside'), undefinedOutside: sum('undefined_outside') };
}

// Data files of the manifests whose format entry is supported or higher. A pattern's * and ?
// match within one path segment, as the documentation check matches them.
const glob = p => new RegExp('^' + p.replace(/[.+^${}()|[\]\\]/g, '\\$&').replaceAll('*', '[^/]*').replaceAll('?', '[^/]') + '$');
const formats = metas.filter(m => m.id.startsWith('FMT-') && ['supported', 'established'].includes(m.status));
const dataFiles = {};
for (const name of readdirSync(join(root, 'spec/builds')).filter(n => n.endsWith('.files.yaml'))) {
  const build = name.slice(0, -'.files.yaml'.length);
  const paths = [...read(join(root, 'spec/builds', name)).matchAll(/^  - path: (.+)\n((?:    .*\n?)*)/gm)]
    .filter(m => /^    format: data$/m.test(m[2])).map(m => m[1].trim());
  const supported = paths.filter(p => formats.some(f => (f.builds ?? []).includes(build)
    && (f.files ?? []).some(pattern => glob(String(pattern)).test(p))));
  dataFiles[build] = { files: paths.length, supported: supported.length };
}

// Queue items by the section, the kind of evidence, they sit under.
const sections = ['Static', 'Emulated call', 'Agent run', 'Live session', 'Source', 'Blocked'];
const queue = Object.fromEntries(sections.map(s => [s, 0]));
for (const path of markdown(join(root, 'queue')).filter(p => !p.endsWith('README.md'))) {
  let section = null;
  for (const line of read(path).split('\n')) {
    if (line.startsWith('## ')) section = line.slice(3).trim();
    else if (/^- Q-[A-Z0-9]+-\d+\./.test(line) && section in queue) queue[section]++;
  }
}

const reports = existsSync(join(root, 'docs/reports'))
  ? readdirSync(join(root, 'docs/reports')).filter(n => /^R-\d+\.md$/.test(n)).length : 0;

const figures = { parity, entries, completeReading, coverage: coverage.inventories, mapping, dataFiles, queue, reports };
if (args.includes('--check')) {
  console.log(JSON.stringify(figures, null, 1));
  process.exit(0);
}

// The previous snapshot is kept in the file, so each change is against the last time it was written.
// A second run on the same date replaces that date's snapshot and compares against the one before it.
const stored = existsSync(out) ? JSON.parse(read(out).match(/<!-- snapshot (.*) -->/)?.[1] ?? 'null') : null;
const previous = stored?.date === date ? stored.prior ?? null : stored;
const history = existsSync(out) ? (read(out).split('## History\n')[1] ?? '').split('\n')
  .filter(l => /^\| \d{4}-/.test(l)) : [];
const commit = git('rev-parse', '--short=12', 'HEAD');
const change = (now, before) => !previous ? '' : before === undefined ? 'new' : now === before ? '' : `${now > before ? '+' : ''}${now - before}`;
const pct = (n, d) => d ? `${(100 * n / d).toFixed(1)}%` : 'n/a';
const lines = [];
const row = (...cells) => lines.push(`| ${cells.join(' | ')} |`);

lines.push('# Progress', '',
  '<!-- Generated by tools/progress-report.mjs. Do not edit; run `node tools/progress-report.mjs` and commit the result. -->', '',
  'The progress measures the [work protocol](../vendor/upstream/work-protocol.md#measuring-progress) (lines 483-536) lists, computed from',
  'the repository. Each Change column is against the previous snapshot, whose figures the file keeps in a comment at',
  'the end. Coverage counts what the function inventories list and is no share of the behaviour understood; a',
  'percentage of completion comes only from the parity matrix.', '',
  `## Snapshot of ${date}`, '', `Measured at commit \`${commit}\`${previous ? `; the previous snapshot is of ${previous.date}, commit \`${previous.commit}\`` : ''}.`, '');

lines.push('### Parity', '');
const rowsTotal = Object.values(parity.code).reduce((a, b) => a + b, 0);
row('Code', 'Rows', 'Share', 'Change'); row('---', '---:', '---:', '---:');
for (const [k, v] of Object.entries(parity.code)) row(k, v, pct(v, rowsTotal), change(v, previous?.parity.code[k]));
lines.push('');
row('Status', 'Rows', 'Change'); row('---', '---:', '---:');
for (const [k, v] of Object.entries(parity.status)) row(k, v, change(v, previous?.parity.status[k]));

lines.push('', '### Spec entries by status', '');
const statuses = [...new Set(Object.values(entries).flatMap(Object.keys))]
  .sort((a, b) => ['unknown', 'sourced', 'recorded', 'supported', 'established', 'disputed', 'superseded']
    .indexOf(a) - ['unknown', 'sourced', 'recorded', 'supported', 'established', 'disputed', 'superseded'].indexOf(b));
row('Kind', ...statuses, 'Total', 'Change'); row('---', ...statuses.map(() => '---:'), '---:', '---:');
for (const kind of Object.keys(entries).sort()) {
  const total = Object.values(entries[kind]).reduce((a, b) => a + b, 0);
  const before = previous?.entries[kind] ? Object.values(previous.entries[kind]).reduce((a, b) => a + b, 0) : undefined;
  row(kind, ...statuses.map(s => {
    const v = entries[kind][s] ?? 0, c = change(v, previous?.entries[kind]?.[s] ?? (previous ? 0 : undefined));
    return c && c !== 'new' ? `${v} (${c})` : v;
  }), total, change(total, before));
}
lines.push('', `Complete reading: ${completeReading ? `${completeReading} entries list findings in \`complete_reading\`; the function count they cover is not computed here` : 'no entry lists findings in `complete_reading`, so no function counts as completely read'}.`);

lines.push('', '### Citation coverage', '', 'From `npm exec -- standard-coverage`: functions and body bytes of each inventory that an entry\'s `locations` cite.',
  'A path under `@CD/` is the copy on the disc image; the build runs the installed copies (see the build entry in `spec/builds/`).', '');
row('Inventory', 'Functions cited', 'Share', 'Change', 'Bytes cited', 'Share', 'Change', 'Out of scope');
row('---', '---:', '---:', '---:', '---:', '---:', '---:', '---:');
for (const inv of coverage.inventories) {
  const before = previous?.coverage.find(p => p.path === inv.path);
  row(`\`${inv.path.replace(/^coverage\//, '')}\``, `${inv.cited} of ${inv.functions}`, pct(inv.cited, inv.functions - inv.outOfScope),
    change(inv.cited, before?.cited), `${inv.citedBytes} of ${inv.bytes}`, pct(inv.citedBytes, inv.bytes), change(inv.citedBytes, before?.citedBytes), inv.outOfScope);
}
if (coverage.unmeasured.length) lines.push('', `Not measured: ${coverage.unmeasured.map(u => `\`${u.path ?? u}\``).join(', ')}.`);

lines.push('', '### Mapping coverage', '', 'From each inventory\'s regions file: recognised bodies against the measured executable regions, and the bytes outside every body.', '');
row('Inventory', 'Region bytes', 'In bodies', 'Share', 'Instructions outside', 'Data outside', 'Undefined outside');
row('---', '---:', '---:', '---:', '---:', '---:', '---:');
for (const [path, m] of Object.entries(mapping))
  row(`\`${path.replace(/^coverage\//, '')}\``, m.regions, m.bodies, pct(m.bodies, m.regions), m.instructionsOutside, m.dataOutside, m.undefinedOutside);

lines.push('', '### Data file formats', '', 'Data files of each build\'s manifest that a `supported` or `established` format entry for that build names.', '');
row('Build', 'Data files', 'Supported', 'Share', 'Change'); row('---', '---:', '---:', '---:', '---:');
for (const [b, d] of Object.entries(dataFiles)) row(b, d.files, d.supported, pct(d.supported, d.files), change(d.supported, previous?.dataFiles[b]?.supported));

lines.push('', '### Queue and reports', '');
row('Items', 'Count', 'Change'); row('---', '---:', '---:');
for (const [k, v] of Object.entries(queue)) row(`Queue: ${k}`, v, change(v, previous?.queue[k]));
row('Open reports', reports, change(reports, previous?.reports));

// History keeps one line per snapshot with the headline figures.
const dsun = coverage.inventories.find(i => /^coverage\/[^/@]+\/DSUN\.EXE\.tsv$/.test(i.path));
const established = Object.values(entries).reduce((n, k) => n + (k.established ?? 0), 0);
const supportedEntries = Object.values(entries).reduce((n, k) => n + (k.supported ?? 0), 0);
const queued = Object.values(queue).reduce((a, b) => a + b, 0);
const line = `| ${date} | \`${commit}\` | ${parity.code.complete ?? 0} | ${parity.code.partial ?? 0} | ${parity.code.missing ?? 0} | ${established} | ${supportedEntries} | ${metas.filter(m => m.id.startsWith('FND-')).length} | ${dsun ? `${dsun.cited} of ${dsun.functions}` : 'n/a'} | ${queued} | ${reports} |`;
const kept = history.filter(l => !l.startsWith(`| ${date} |`));
lines.push('', '## History', '',
  '| Date | Commit | Parity complete | Partial | Missing | Established entries | Supported entries | Findings | Installed `DSUN.EXE` functions cited | Queue items | Open reports |',
  '| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |', ...kept, line, '');
const snapshot = { date, commit, ...figures, coverage: coverage.inventories.map(({ path, cited, citedBytes }) => ({ path, cited, citedBytes })) };
if (previous) snapshot.prior = { ...previous, prior: undefined };
lines.push(`<!-- snapshot ${JSON.stringify(snapshot)} -->`, '');
writeFileSync(out, lines.join('\n'));
console.log(`Wrote docs/PROGRESS.md for ${date} at ${commit}`);
