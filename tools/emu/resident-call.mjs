#!/usr/bin/env node
// Research tooling. Original-derived configurations and reports remain in GAME_DIR.
import { readFileSync, statSync, realpathSync, writeFileSync } from 'node:fs';
import { resolve, dirname, relative, isAbsolute } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { spawnSync } from 'node:child_process';
import { sourceXxh3 } from '@scientific-method/executable-reader';
import { readMz } from '@scientific-method/executable-reader/legacy-image';
import { enginePython } from '../tool-dependencies.mjs';

const ROOT = resolve(dirname(fileURLToPath(import.meta.url)), '../..');
function integer(n, min, max, name) {
  if (!Number.isSafeInteger(n) || n < min || n > max) throw new Error(`Invalid ${name}`);
  return n;
}
function inside(root, file) {
  const p = relative(root, file);
  return p !== '' && !p.startsWith('..') && !isAbsolute(p);
}

export function loadResident(bytes, config) {
  if (config.sourceKind !== 'mz') throw new Error('Only resident MZ calls are supported');
  const digest = sourceXxh3(bytes);
  if (digest !== config.xxh3) throw new Error('Source xxh3 differs from the build baseline');
  const mz = readMz(bytes, config.loadSegment);
  const base = mz.loadSegment * 16;
  if (base + mz.end - mz.header > 0x100000) throw new Error('Resident image crosses the real-mode address limit');
  if (!Array.isArray(config.regions) || !config.regions.length || config.regions.length > 128) throw new Error('Declare 1..128 code regions');
  const regions = config.regions.map(r => {
    integer(r.start, mz.header, mz.end - 1, 'region start');
    integer(r.end, r.start + 1, mz.end, 'region end');
    integer(r.segment, 0, 65535, 'region segment');
    integer(r.ip, 0, 65535, 'region offset');
    if (typeof r.name !== 'string' || !r.name || typeof r.evidence !== 'string' || !r.evidence) throw new Error('Name and cite each code region');
    if (mz.address(r.segment, r.ip) !== r.start || r.ip + r.end - r.start > 65536) throw new Error('Region mapping differs from the resident source');
    return { ...r, linearStart: base + r.start - mz.header, linearEnd: base + r.end - mz.header };
  });
  for (let i = 0; i < regions.length; i++) for (let j = i + 1; j < regions.length; j++) {
    if (regions[i].start < regions[j].end && regions[j].start < regions[i].end) throw new Error('Overlapping code regions');
  }
  const owner = regions.find(r => Array.isArray(r.entries) && r.entries.includes(config.entry));
  if (!owner) throw new Error('Call entry must be a declared region entry');
  integer(config.entry, owner.start, owner.end - 1, 'call entry');
  const resident = Buffer.from(bytes.subarray(mz.header, mz.end));
  for (const file of mz.relocations) {
    const at = file - mz.header;
    resident.writeUInt16LE((resident.readUInt16LE(at) + mz.loadSegment) & 65535, at);
  }
  if ('memory' in config || 'patches' in config || 'arguments' in config) throw new Error('Memory/parameter seeding needs supported layout bindings; this harness does not accept raw patches');
  return {
    ...config, protocol: 1, image: resident.toString('base64'), imageBase: base,
    sourceIdentity: { size: bytes.length, xxh3: digest }, regions,
    entrySegment: owner.segment, entryOffset: owner.ip + config.entry - owner.start,
  };
}

export function emulate(packet, python = enginePython()) {
  const child = spawnSync(python, [resolve(ROOT, 'tools/emu/resident_call.py')], {
    input: JSON.stringify(packet), encoding: 'utf8', timeout: 30000, maxBuffer: 16 * 1024 * 1024,
  });
  if (child.error) throw new Error(`Emulated call worker failed: ${child.error.message}`);
  if (child.status !== 0) throw new Error(child.stderr.trim() || 'Emulated call worker failed');
  return JSON.parse(child.stdout);
}

export function run(configFile, env = process.env) {
  if (!env.GAME_DIR) throw new Error('Set GAME_DIR to the licensed installation; no original call was made');
  const game = realpathSync(env.GAME_DIR);
  if (inside(ROOT, game) || game === ROOT) throw new Error('GAME_DIR must be outside the checkout');
  const file = realpathSync(resolve(configFile));
  if (!inside(game, file) || statSync(file).size > 1024 * 1024) throw new Error('Keep bounded original call configs inside GAME_DIR');
  const config = JSON.parse(readFileSync(file, 'utf8').replace(/^\uFEFF/, ''));
  const manifest = JSON.parse(readFileSync(resolve(ROOT, 'src/DarkSunWakeRedux.Extractor/source-manifests/gog-en-52095422060333615.json'), 'utf8').replace(/^\uFEFF/, ''));
  const identity = manifest.files.find(f => f.path === config.source);
  if (config.build !== 'BLD-GOG-EN-1.1' || config.source !== 'DSUN.EXE' || !identity || identity.xxh3 !== config.xxh3) throw new Error('Call must name the supported build and its manifest baseline');
  const source = realpathSync(resolve(game, config.source));
  if (!inside(game, source) || statSync(source).size !== identity.size) throw new Error('Licensed source size or location differs from the manifest');
  const output = resolve(dirname(file), config.report);
  if (!inside(game, realpathSync(dirname(output))) || inside(ROOT, output)) throw new Error('Original reports must remain inside GAME_DIR and outside the checkout');
  const result = emulate(loadResident(readFileSync(source), config), enginePython(ROOT, env));
  // Exclusive creation also refuses an existing report or a link to another destination.
  writeFileSync(output, JSON.stringify(result, null, 2) + '\n', { encoding: 'utf8', flag: 'wx' });
  return { returned: result.returned, steps: result.steps, report: output };
}

if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
  try {
    if (process.argv.length !== 3) throw new Error('Usage: node tools/emu/resident-call.mjs <GAME_DIR call config>');
    console.log(JSON.stringify(run(process.argv[2])));
  } catch (error) { console.error(error.message); process.exitCode = 1; }
}
