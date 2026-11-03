import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtempSync,mkdirSync,copyFileSync,writeFileSync,rmSync} from 'node:fs';
import {tmpdir} from 'node:os';
import {join,resolve} from 'node:path';
import {spawnSync} from 'node:child_process';
const root=resolve(import.meta.dirname,'../..'),pwsh=process.env.PWSH||'pwsh';
const noPwsh=spawnSync(pwsh,['-NoProfile','-Command','exit 0']).error?'PowerShell 7 required':false;
const literal=s=>"'"+s.replaceAll("'","''")+"'";
function exercise(t,{offline=false,failBuild=false,mode='integration',research=false,explicit=false}={}) {
 const dir=mkdtempSync(join(tmpdir(),'validation-offline-'));
 t.after(()=>rmSync(dir,{recursive:true,force:true}));mkdirSync(join(dir,'tools'));
 copyFileSync(join(root,'tools/Invoke-Validation.ps1'),join(dir,'tools/Invoke-Validation.ps1'));
 writeFileSync(join(dir,'tools/Test.ps1'),`param($MinimumExpectedTests,$TestFilter,[switch]$NoRestore,$ChangedSince,[switch]$Research,[switch]$Integration)
$global:gate=@{offline=[bool]$NoRestore;filter=$TestFilter;research=[bool]$Research;integration=[bool]$Integration;base=$ChangedSince}
$global:LASTEXITCODE=0
`);
 const plan=mode==='invalid'?{mode:'bad',dotnet:false}:{mode,dotnet:mode==='integration',suites:[],tests:[]};
 const driver=join(dir,'driver.ps1');writeFileSync(driver,`
$ErrorActionPreference='Stop'
$global:commands=[Collections.Generic.List[object]]::new()
$global:nodes=[Collections.Generic.List[object]]::new()
$global:gate=$null
function node {$global:nodes.Add([object]@($args));$global:LASTEXITCODE=0;'${JSON.stringify(plan)}'}
function dotnet {$global:commands.Add([object]@($args));$global:LASTEXITCODE=if (${failBuild?'$true':'$false'} -and $args[0] -eq 'build') {7} else {0}}
$failure=$null
try { & ${literal(join(dir,'tools/Invoke-Validation.ps1'))} ${offline?'-NoRestore':''} ${mode==='selected'||research?'-ChangedSince HEAD':'-Integration'} ${research?'-Research':''} ${explicit?"-TestFilter SyntheticFilter -MinimumExpectedTests 3":''} }
catch {$failure=$_.Exception.Message}
'MOCK_RESULT:'+(@{commands=@($global:commands);nodes=@($global:nodes);gate=$global:gate;failure=$failure}|ConvertTo-Json -Depth 8 -Compress)
`);
 const r=spawnSync(pwsh,['-NoProfile','-File',driver],{encoding:'utf8',timeout:120000,
  env:{...process.env,CI:'',TMPDIR:dir,TMP:dir,TEMP:dir}});
 assert.equal(r.status,0,r.error?.message||r.stdout+r.stderr);
 const line=r.stdout.split(/\r?\n/).find(s=>s.startsWith('MOCK_RESULT:'));assert(line,r.stdout+r.stderr);
 return JSON.parse(line.slice('MOCK_RESULT:'.length));
}
test('integration restores normally and NoRestore forwards without restore fallback',{skip:noPwsh},t=>{
 const normal=exercise(t),offline=exercise(t,{offline:true});
 assert.equal(normal.failure,null);assert.equal(offline.failure,null);
 assert.equal(normal.commands[0][0],'restore');assert(!offline.commands.some(a=>a[0]==='restore'));
 assert.equal(normal.gate.offline,false);assert.equal(offline.gate.offline,true);assert.equal(offline.gate.integration,true);
 assert.deepEqual(offline.nodes[0].slice(1),['--integration']);
 assert(offline.commands.some(a=>a[0]==='build'));assert(offline.commands.some(a=>a[0]==='publish'));
 assert(offline.commands.filter(a=>['build','publish'].includes(a[0])).every(a=>a.includes('--no-restore')));
 assert.equal(exercise(t,{offline:true,failBuild:true}).failure,'dotnet build failed with exit code 7.');
});
test('selected research forwards its exact plan without restoring, building or publishing',{skip:noPwsh},t=>{
 const r=exercise(t,{offline:true,mode:'selected',research:true});
 assert.equal(r.failure,null);assert.deepEqual(r.commands,[]);
 assert.equal(r.gate.research,true);assert.equal(r.gate.base,'HEAD');
 assert.deepEqual(r.nodes[0].slice(1),['--research','HEAD']);
 const docs=exercise(t,{mode:'selected'});assert.equal(docs.failure,null);assert.deepEqual(docs.commands,[]);
 const explicit=exercise(t,{mode:'selected',explicit:true});assert.equal(explicit.failure,null);
 assert.equal(explicit.gate.filter,'SyntheticFilter');assert(explicit.commands.some(a=>a[0]==='build'));
});
test('integration cannot be narrowed and invalid or mixed research plans fail before execution',{skip:noPwsh},t=>{
 const full=exercise(t,{explicit:true});assert.equal(full.failure,null);assert(!full.gate.filter);
 assert.match(exercise(t,{mode:'invalid'}).failure,/Invalid validation suite plan/);
 assert.match(exercise(t,{mode:'selected',research:true,explicit:true}).failure,/Research needs/);
 assert.match(exercise(t,{research:true}).failure,/Research selection returned/);
});
