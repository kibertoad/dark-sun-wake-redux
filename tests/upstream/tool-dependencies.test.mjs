import test from "node:test";
import assert from "node:assert/strict";
import { mkdtempSync, mkdirSync, readFileSync, writeFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { resolve, join } from "node:path";
import { sourceXxh3 } from "@scientific-method/executable-reader";
import { spawnSync } from "node:child_process";
import { verifyNpmPackages, verifyEngine, enginePython, withEngine, checkerScript } from "../../tools/tool-dependencies.mjs";
import { run } from "../../tools/evidence/report.mjs";
const root = resolve(import.meta.dirname, "../..");
function scratch(t) {
  const dir = mkdtempSync(join(tmpdir(), "published-tooling-"));
  t.after(() => rmSync(dir, {recursive:true,force:true}));
  return dir;
}
test("published versions, missing engine and version mismatch fail actionably", t => {
  verifyNpmPackages(); verifyEngine(); assert(checkerScript().endsWith("standard-checker.js"));
  assert.throws(() => verifyEngine(root, {...process.env,EVIDENCE_PYTHON:join(scratch(t),"missing-python")}), /Restore-ToolDependencies/);
  const dir = scratch(t); mkdirSync(join(dir,"tools/evidence"),{recursive:true});
  writeFileSync(join(dir,"tools/evidence/requirements.txt"),readFileSync(join(root,"tools/evidence/requirements.txt"),"utf8").replace(/scientific-method-engine==\d+\.\d+\.\d+/,"scientific-method-engine==99.0.0"));
  assert.throws(() => verifyEngine(dir,{...process.env,EVIDENCE_PYTHON:enginePython()}), /expected 99.0.0/);
});
test("exact npm lock and installed-version mismatch are rejected", t => {
  const dir=scratch(t), name="@scientific-method/executable-reader";
  writeFileSync(join(dir,"package.json"),JSON.stringify({devDependencies:{[name]:"0.0.0"}}));
  writeFileSync(join(dir,"package-lock.json"),JSON.stringify({packages:{["node_modules/"+name]:{version:"0.0.1",integrity:"sha512-synthetic"}}}));
  assert.throws(() => verifyNpmPackages(dir), /exact npm lock/);
});
test("wrapper routes a synthetic MZ through the installed engine and restores environment", t => {
  const dir=scratch(t), data=Buffer.alloc(512); data.write("MZ");data.writeUInt16LE(1,4);data.writeUInt16LE(4,8);data[64]=0xc3;
  writeFileSync(join(dir,"source.bin"),data);
  writeFileSync(join(dir,"config.json"),JSON.stringify({source:"source.bin",sourceKind:"mz",xxh3:sourceXxh3(data),entry:64,
    regions:[{name:"synthetic",start:64,end:65,ip:0,segment:4096,entries:[64],evidence:"synthetic MZ"}]}));
  const previous=process.env.EVIDENCE_PYTHON;
  assert.equal(run(["x86-effects",join(dir,"config.json")]).completeWithinModel,true);
  assert.equal(process.env.EVIDENCE_PYTHON,previous);
  assert.throws(() => withEngine(() => {throw Error("synthetic failure")}),/synthetic failure/);
  assert.equal(process.env.EVIDENCE_PYTHON,previous);
});
test("installed engine refuses incompatible prepared protocol before source access", () => {
  for (const protocol of [2,99]) {
    const r=spawnSync(enginePython(),["-m","scientific_method_engine","trace","-"],{input:JSON.stringify({preparedProtocol:protocol}),encoding:"utf8"});
    assert.notEqual(r.status,0);assert.match(r.stderr,new RegExp('protocol '+protocol));assert.match(r.stderr,/Install matching/);
  }
});


test("pypcode is required and its installed version must match the exact runtime lock", t => {
  const dir=scratch(t); mkdirSync(join(dir,"tools/evidence"),{recursive:true});
  const path=join(dir,"tools/evidence/requirements.txt"), lock=readFileSync(join(root,"tools/evidence/requirements.txt"),"utf8");
  writeFileSync(path,lock.replace(/^pypcode==.*$/m,""));
  assert.throws(() => verifyEngine(dir,{...process.env,EVIDENCE_PYTHON:enginePython()}), /exactly pin engine, Capstone, pypcode and xxhash/);
  writeFileSync(path,lock.replace(/pypcode==\d+\.\d+\.\d+/,"pypcode==99.0.0"));
  assert.throws(() => verifyEngine(dir,{...process.env,EVIDENCE_PYTHON:enginePython()}), /pypcode.*expected 99.0.0/);
});


test("installed table continuations keep prefix writes beside the ordinary stopped path", t => {
  const dir=scratch(t), data=Buffer.alloc(512);data.write("MZ");data.writeUInt16LE(1,4);data.writeUInt16LE(4,8);
  data.set([0xc7,0x06,0x20,0x00,0x01,0x00,0xff,0xe3],64);
  data.set([0xb8,0xff,0xff,0xc3],80);data.writeUInt16LE(16,112);
  writeFileSync(join(dir,"source.bin"),data);
  const config={source:"source.bin",sourceKind:"mz",xxh3:sourceXxh3(data),entry:64,
    registers:{ds:8192,ss:12288,sp:65280},regions:[{name:"synthetic",start:64,end:84,ip:0,segment:4096,entries:[64],evidence:"synthetic MZ"}],
    indirectJumps:[{site:70,exhaustive:false,evidence:"synthetic conditional route; selector unknown",table:{start:112,count:1,stride:2,evidence:"synthetic near-word table"}}]};
  const path=join(dir,"config.json");writeFileSync(path,JSON.stringify(config));
  const r=run(["x86-effects",path]);
  assert.equal(r.completeWithinModel,false);assert(r.paths.some(p=>!p.returned&&p.stopSite===70));
  assert(r.declaredContinuationPaths.some(p=>p.returned&&p.registers.ax.value===65535&&p.events.some(e=>e.kind==="write"&&e.site===64&&e.width===2&&e.value.value===1)));
  assert(r.effectOrdering.declaredContinuationPaths.every(p=>!p.effectCompleteWithinModel));
  writeFileSync(path,JSON.stringify({...config,maxPaths:1}));
  const capped=run(["x86-effects",path]);assert.equal(capped.completeWithinModel,false);
  assert(!capped.declaredContinuationPaths.some(p=>p.returned));
});
