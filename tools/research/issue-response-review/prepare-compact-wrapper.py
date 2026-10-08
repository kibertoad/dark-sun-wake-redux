from pathlib import Path
folder=Path(GAME_DIR+'/analysis/reporter-audit/compact-wrapper-controls')
folder.mkdir(exist_ok=True)
script=r'''
import {readFileSync,writeFileSync} from 'node:fs';
import {spawnSync} from 'node:child_process';
import cp from 'node:child_process';
import {syncBuiltinESMExports} from 'node:module';
import assert from 'node:assert/strict';
import {run as oldRun} from 'file:///C:/sources/dark-sun-wake-redux/artifacts/protocol3-delivery/node_modules/@scientific-method/executable-reader/dist/src/report.js';
import {run as newRun} from 'file:///C:/sources/dark-sun-wake-redux/artifacts/issue-response-review/reader/node_modules/@scientific-method/executable-reader/dist/src/report.js';
import os,sys
GAME_DIR=os.environ.get('GAME_DIR') or sys.exit('Set GAME_DIR to the supported Dark Sun build.')
assert(!process.env.PYTHONPATH);
const rawSpawn=cp.spawnSync,wire=[];
cp.spawnSync=(...args)=>{const r=rawSpawn(...args);wire.push({python:args[0],command:args[1][3],bytes:Buffer.byteLength(r.stdout??''),limit:args[2]?.maxBuffer,error:r.error?.code,status:r.status});return r;};
syncBuiltinESMExports();
const sourceRoot=GAME_DIR+'/analysis/reporter-audit/coordinate-gates/';
const source=JSON.parse(readFileSync(sourceRoot+'wrapper-rejected-unread-services.json','utf8'));
const complete=JSON.parse(readFileSync(sourceRoot+'validator-traced-getters.json','utf8'));
assert.equal(source.entry,0x26130);assert(source.callModels.length===0);
const config={...source,regions:complete.regions};
assert.deepEqual([config.maxSteps,config.maxPaths,config.totalSteps,config.visitLimit],[200,64,20000,4]);
assert.equal(config.sha256,'ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c');
delete config.sha256;config.xxh3='e296af55ba2ecde7e77f555c90f33d0b';
const root=new URL('./',import.meta.url);
function query(name,c,run,python,command='effects'){
 const f=new URL(name+'.json',root);writeFileSync(f,JSON.stringify(c));process.env.EVIDENCE_PYTHON=python;
 try{const r=run([command,f.pathname.startsWith('/')?decodeURIComponent(f.pathname.slice(1)):f.pathname]);writeFileSync(new URL(name+'.report.json',root),JSON.stringify(r));console.log(JSON.stringify({name,command,wire:wire.at(-1),bytes:JSON.stringify(r).length,prettyBytes:JSON.stringify(r,null,2).length,paths:r.paths?.length,steps:r.stepsUsed,gaps:r.gaps,stops:[...new Set(r.paths.map(p=>p.stop))],complete:r.completeWithinModel}));return r;}
 catch(e){console.log(JSON.stringify({name,command,error:e.message,wire:wire.at(-1)}));return {error:e.message};}
}
const adopted='C:/sources/dark-sun-wake-redux/artifacts/evidence-python/Scripts/python.exe';
const candidate='C:/sources/dark-sun-wake-redux/artifacts/issue-response-review/python/Scripts/python.exe';
const old=query('wrapper-engine400',config,oldRun,adopted);
const r=query('wrapper-engine611',config,newRun,candidate);
assert(!r.error,'Published compact bridge must produce a report');assert(!r.completeWithinModel);
assert(wire.at(-1).bytes<=32*1024*1024);assert.equal(wire.at(-1).limit,32*1024*1024);
console.log(JSON.stringify({oldCapReproduced:old.error?.includes('32 MiB')??false,newReportProduced:true,sourceModels:config.callModels.length,native:r.nativeReachability,limits:r.limits}));
'''
(folder/'verify-compact-wrapper.mjs').write_text(script)
print('Prepared equal-input old/new published wrapper bridge probe; all original bounds retained')
