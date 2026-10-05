// needs: GAME_DIR
// FND-CONFIG-193: resident call-free initializer and its fixed word/byte stores.
// Read-only observation bindings; no original memory field is seeded here.
import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve,join} from 'node:path';
import {randomUUID} from 'node:crypto';
import assert from 'node:assert/strict';
import {run} from './resident-call.mjs';

export function checkInitializer(gameDir) {
  if (!gameDir) return {skipped:'GAME_DIR is absent'};
  const directory=join(gameDir,'analysis/reporter-audit/emulated-initializer',randomUUID());
  mkdirSync(directory,{recursive:true});
  const identity=JSON.parse(readFileSync(new URL('../../src/DarkSunWakeRedux.Extractor/source-manifests/gog-en-52095422060333615.json',import.meta.url),'utf8').replace(/^\uFEFF/,'' )).files.find(f=>f.path==='DSUN.EXE');
  const regions=[{name:'graphics-pool-initializer',start:0x1384b,end:0x138d8,segment:0x1bf3,ip:0x271b,entries:[0x1384b],evidence:'FND-CONFIG-193 bounded complete resident call-free body'}];
  const fields=[['root_zero_segment',4,2],['root_one_segment',6,2],['next_paragraph',0xe4e,2],['upper_paragraph',0xe4c,2],['free_slot_flags',0xc08,508]];
  const config={build:'BLD-GOG-EN-1.1',source:'DSUN.EXE',sourceKind:'mz',xxh3:identity.xxh3,entry:0x1384b,returnBytes:4,regions,maxInstructions:2048,maxWrites:512,observations:fields.map(([name,offset,length])=>({name:'graphics_pool.'+name,segment:0x1bf3,offset,length}))};
  function call(name,c) {const file=join(directory,name+'.json');writeFileSync(file,JSON.stringify({...c,report:name+'.report.json'}));run(file,{...process.env,GAME_DIR:resolve(gameDir)});return JSON.parse(readFileSync(join(directory,name+'.report.json'),'utf8'));}
  const cases=[];
  for (const [name,registers] of [['loaded-context',{}],['register-restoration',{ds:0x57e0,esi:0x13579bdf,edi:0x2468ace0}]]) {
    const r=call(name,{...config,registers});assert(r.returned);assert.equal(r.hardware.length,0);
    const observations=Object.fromEntries(r.observations.map(o=>[o.name,o]));
    for (const [field,expected] of [['root_zero_segment',0xa000],['root_one_segment',0xa400],['next_paragraph',0xa7e8],['upper_paragraph',0xaffb]]) assert.equal(Buffer.from(observations['graphics_pool.'+field].after,'hex').readUInt16LE(),expected,field);
    const flags=observations['graphics_pool.free_slot_flags'];const before=Buffer.from(flags.before,'hex'),after=Buffer.from(flags.after,'hex');
    for (let i=0;i<508;i+=2) {assert.equal(after[i],128);assert.equal(after[i+1],before[i+1]);}
    const flagBase=0x1bf3*16+0xc08;const flagWrites=r.writes.filter(w=>w.address>=flagBase&&w.address<flagBase+508);
    assert.equal(flagWrites.length,254);assert(flagWrites.every(w=>w.width===1&&w.value===128&&(w.address-flagBase)%2===0));
    for (const reg of ['ds','esi','edi']) assert.equal(r.registers[reg],r.setup.entryRegisters[reg],reg);
    assert.equal(r.branches.length,1);assert.deepEqual(r.branches[0].outcomes,['fallthrough','taken']);
    assert.notEqual(Buffer.from(observations['graphics_pool.root_zero_segment'].after,'hex').readUInt16LE(),0xa001,'wrong root answer must differ');
    cases.push({name,returned:true,branchDirectionsCovered:true});
  }
  assert.throws(()=>call('instruction-cap',{...config,maxInstructions:1}),/instruction limit/);
  assert.throws(()=>call('write-cap',{...config,maxWrites:1}),/write limit/);
  assert.throws(()=>call('omitted-return',{...config,regions:regions.map(r=>({...r,end:0x138d7}))}),/undeclared resident execution/);
  return {cases,negativeControls:'passed',directory,nativeReachability:'unconfirmed'};
}
