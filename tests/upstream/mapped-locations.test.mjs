import { checkerScript } from "../../tools/tool-dependencies.mjs";
import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtempSync,mkdirSync,writeFileSync,rmSync} from 'node:fs';
import {tmpdir} from 'node:os';
import {resolve,dirname} from 'node:path';
import {fileURLToPath} from 'node:url';
import {spawnSync} from 'node:child_process';
const root=resolve(dirname(fileURLToPath(import.meta.url)),'../..');
test('pinned checker accepts mapped endpoints and rejects holes and crossing overlay bounds',t=>{
 const dir=mkdtempSync(resolve(tmpdir(),'mapped-location-'));
 t.after(()=>rmSync(dir,{recursive:true,force:true}));
 const build='BLD-'+'SYNTHETIC',finding='FND-'+'SYNTHETIC-001';
 function put(path,text){mkdirSync(dirname(resolve(dir,path)),{recursive:true});writeFileSync(resolve(dir,path),text);}
 for (const path of ['spec/glossary','deviations','parity']) mkdirSync(resolve(dir,path),{recursive:true});
 put('spec/LICENSE','Synthetic test fixture.');
 put('spec/README.md','# Synthetic spec\n\n## Scope\n\nSynthetic checker input.\n\n## Standard version\n\nThis spec follows version 1 of the documentation standard.\n\n## Areas\n\n| Area | Covers |\n|---|---|\n| SYNTHETIC | Synthetic code ranges |\n');
 put(`spec/builds/${build}.md`,`---\nid: ${build}\ntitle: Synthetic executable\nsuperseded_by: []\ndeveloper: Synthetic\npublisher: Synthetic\npublisher_version: "1"\ndistribution: Synthetic\nlanguages: [en]\nint_width: 16\nmanifest: ${build}.files.yaml\n---\n\n## Obtaining\n\nSynthetic fixture.\n\n## Compared with other builds\n\nNone.\n\n## Other files\n\nNone.\n\n## Code ranges\n\n| File | Range | Overlay | Finding |\n|---|---|---|---|\n| GAME.EXE | 0x20..0x40 | 1 | ${finding} |\n| GAME.EXE | 0x50..0x80 | 2 | ${finding} |\n`);
 put(`spec/builds/${build}.files.yaml`,`files:\n  - path: GAME.EXE\n    format: MZ\n    size: 160\n    xxh3: ${'a'.repeat(32)}\n`);
 function run(offset){
 put(`spec/findings/${finding}.md`,`---\nid: ${finding}\ntitle: Synthetic ranges\nstatus: recorded\nbuilds: [${build}]\nsuperseded_by: []\nrecorded_by: Synthetic\nreproduced_by: []\nmethod: static\nlocations:\n  - build: ${build}\n    file: GAME.EXE\n    offset: ${offset}\ntool: synthetic fixture\nenvironment: null\n---\n\n## Observation\n\nSynthetic code ranges.\n\n## Interpretation\n\nFixture only.\n\n## Alternatives\n\nNone.\n\n## How to reproduce\n\nRun the synthetic checker test.\n`);
 return spawnSync(process.execPath,[checkerScript(),'--root',dir,'--no-ksy'],{encoding:'utf8'});
 }
 for(const offset of ['0x20..0x40','0x50..0x80']){const result=run(offset);assert.equal(result.status,0,result.stdout+result.stderr);}
 for(const offset of ['0x40','0x3F..0x51','0x7F..0x81']){const result=run(offset);assert.notEqual(result.status,0);assert.match(result.stdout+result.stderr,/does not lie wholly inside/);}
});
