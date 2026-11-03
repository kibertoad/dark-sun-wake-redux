import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtempSync,mkdirSync,copyFileSync,writeFileSync,rmSync} from 'node:fs';
import {tmpdir} from 'node:os';
import {join,resolve} from 'node:path';
import {spawnSync} from 'node:child_process';
const root=resolve(import.meta.dirname,'../..'),pwsh=process.env.PWSH||'pwsh';
function exercise(t,{mode='selected',research=true,explicit=false,failSuite=false}={}) {
 const dir=mkdtempSync(join(tmpdir(),'configured-test-gate-'));
 t.after(()=>rmSync(dir,{recursive:true,force:true}));
 mkdirSync(join(dir,'tools/ghidra'),{recursive:true});
 for(const name of ['Test.ps1','ghidra/ReportPhysicalBytePattern.ps1','ghidra/ReportFbovOverlayMap.ps1'])
  copyFileSync(join(root,'tools',name),join(dir,'tools',name));
 for(const name of ['Restore-ToolDependencies','Verify-Repository','Verify-Configuration','Test-TemplateInfrastructure','Verify-WorkingFiles'])
  writeFileSync(join(dir,'tools',name+'.ps1'),`param($RepositoryRoot,[switch]$NoRestore)
$global:checks.Add('${name}');$global:LASTEXITCODE=0
`);
 const plan={mode,dotnet:mode==='integration',suites:[],tests:[]};
 const driver=join(dir,'driver.ps1');writeFileSync(driver,`
$ErrorActionPreference='Stop'
$global:checks=[Collections.Generic.List[string]]::new()
$global:nodes=[Collections.Generic.List[object]]::new()
$global:commands=[Collections.Generic.List[object]]::new()
function node {
 $global:nodes.Add([object]@($args));$global:LASTEXITCODE=0
 if($args[0] -like '*Select-ValidationSuites.mjs') {'${JSON.stringify(plan)}'}
 if($args[0] -like '*Run-ValidationSuites.mjs' -and ${failSuite?'$true':'$false'}) {$global:LASTEXITCODE=9}
}
function dotnet {$global:commands.Add([object]@($args));$global:LASTEXITCODE=0}
$failure=$null
try { & '${join(dir,'tools/Test.ps1').replaceAll("'","''")}' -NoRestore -ChangedSince HEAD ${research?'-Research':''} ${explicit?'-TestFilter SyntheticFilter':''} }
catch {$failure=$_.Exception.Message}
'RESULT:'+(@{checks=@($global:checks);nodes=@($global:nodes);commands=@($global:commands);failure=$failure}|ConvertTo-Json -Depth 8 -Compress)
`);
 const result=spawnSync(pwsh,['-NoProfile','-File',driver],{encoding:'utf8',env:{...process.env,CI:''}});
 assert.equal(result.status,0,result.error?.message||result.stdout+result.stderr);
 const line=result.stdout.split(/\r?\n/).find(s=>s.startsWith('RESULT:'));assert(line,result.stdout+result.stderr);
 return JSON.parse(line.slice(7));
}
test('actual configured research gate retains fresh checks and exact dispatch without any dotnet command',t=>{
 const r=exercise(t);assert.equal(r.failure,null);assert.deepEqual(r.commands,[]);
 for(const name of ['Restore-ToolDependencies','Verify-Repository','Verify-Configuration','Test-TemplateInfrastructure','Verify-WorkingFiles'])
  assert(r.checks.includes(name),name);
 assert.deepEqual(r.nodes.find(a=>a[0].endsWith('Run-ValidationSuites.mjs')).slice(1),['--research','HEAD']);
 assert(r.nodes.some(a=>a[0].endsWith('Invoke-NodeChecks.mjs')));
});
test('actual configured gate rejects mixed research and propagates tooling failures',t=>{
 assert.match(exercise(t,{explicit:true}).failure,/Research needs/);
 assert.match(exercise(t,{mode:'integration'}).failure,/Research selection returned/);
 assert.match(exercise(t,{failSuite:true}).failure,/Synthetic evidence/);
});
test('actual configured integration cannot be narrowed by a manual filter',t=>{
 const r=exercise(t,{mode:'integration',research:false,explicit:true});assert.equal(r.failure,null);
 const command=r.commands.find(a=>a[0]==='test');assert(command);assert(!command.includes('--filter'));
 assert(command.includes('--no-restore'));
});
