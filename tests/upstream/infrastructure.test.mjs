import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtempSync, mkdirSync, readFileSync, writeFileSync, rmSync, copyFileSync, existsSync} from 'node:fs';
import {tmpdir} from 'node:os';
import {resolve, join} from 'node:path';
import {spawnSync} from 'node:child_process';
import {copyWorkingTree} from './copy-working-tree.mjs';
import {readMz} from '../../tools/evidence/legacy-image.mjs';
const root = resolve(import.meta.dirname, '../..');
const pwsh = process.env.PWSH || 'pwsh';
const run = (file, args=[], options={}) => spawnSync(pwsh, ['-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', file, ...args], {encoding:'utf8', timeout:120000, ...options});
const output = result => result.error?.message || result.stdout + result.stderr;
function fixture(t) {
  const dir = mkdtempSync(join(tmpdir(), 'dark-sun-infrastructure-'));
  t.after(() => rmSync(dir, {recursive:true, force:true}));
  return dir;
}

test('mapped FBOV relocations reach rewritten segment words, preserving original sites', t => {
  const dir=fixture(t), source=join(dir,'source.exe'), mapped=join(dir,'mapped.exe');
  const b=Buffer.alloc(592), w=(p,n)=>b.writeUInt16LE(n,p), d=(p,n)=>b.writeUInt32LE(n,p);
  b.write('MZ'); w(4,1); w(8,4); w(6,2); w(24,28);
  w(28,19); w(32,35);
  b[80]=0x9a; w(81,32); w(83,12); b[96]=0x9a; w(97,16); w(99,13);
  b.write('FBOV',512); d(516,64); d(520,128); d(524,2);
  w(136,12); w(140,2); w(256,0x3fcd); d(260,0); w(264,32); w(266,2); w(268,1);
  w(288,0x3fcd); w(290,0); b[532]=0x9a; w(533,16); w(535,0); w(560,7);
  writeFileSync(source,b);
  const r=run(join(root,'tools/ghidra/New-FbovMappedImage.ps1'),['-SourcePath',source,'-OutputPath',mapped]);
  assert.equal(r.status,0,output(r));
  const result=readMz(readFileSync(mapped)), shifted=p=>result.header+p-64;
  assert.deepEqual([...result.relocations].sort((a,b)=>a-b),[83,99,291,535].map(shifted));
  assert.equal(result.resolveOperand(shifted(291),0).fileOffset,'0x'+shifted(528).toString(16).toUpperCase().padStart(8,'0'));
  assert.equal(result.resolveOperand(shifted(535),16).fileOffset,'0x'+shifted(80).toString(16).toUpperCase().padStart(8,'0'));
  assert.equal(readFileSync(source).equals(b),true);
});
function wrapper(t, code, options={}) {
  const dir=fixture(t), file=join(dir,'control.ps1');
  writeFileSync(file, "$ErrorActionPreference='Stop'\n"+code);
  return run(file, [], options);
}
const literal = value => "'"+value.replaceAll("'", "''")+"'";
const tool = name => literal(join(root,'tools',name));

test('bootstrap refuses missing/unestablished/mismatched facts and requires a recorded executable identity', t => {
  const dir=fixture(t), path=join(dir,'facts.json');
  const base=JSON.parse(readFileSync(join(root,'tools/project-config.json')));
  const valid=structuredClone(base);
  Object.assign(valid.original, {latestOfficialVersion:'1.1',analysisVersion:'1.1',patchStatusEstablished:true,
    patchStatusEvidence:'Synthetic patch provenance',analysisExecutable:{path:'synthetic.exe',byteLength:16,xxh3_128:'a'.repeat(32)}});
  const check=config=>{writeFileSync(path,JSON.stringify(config));return run(join(root,'tools/Bootstrap-Project.ps1'),['-ConfigPath',path,'-ValidateFactsOnly']);};
  assert.equal(check(valid).status,0);
  for(const [mutate,pattern] of [
    [x=>delete x.original.publisher,/Bootstrap facts are missing/],
    [x=>x.original.patchStatusEstablished=false,/Patch status is not established/],
    [x=>x.original.patchStatusEstablished='true',/Patch status is not established/],
    [x=>x.original.analysisVersion='1.0',/differs from latest/],
    [x=>x.original.analysisExecutable.xxh3_128='',/recorded xxh3_128/],
    [x=>x.original.analysisExecutable.xxh3_128='A'.repeat(32),/recorded xxh3_128/],
    [x=>x.original.analysisExecutable.byteLength=0,/recorded xxh3_128/]
  ]){const config=structuredClone(valid);mutate(config);const result=check(config);assert.notEqual(result.status,0);assert.match(output(result),pattern);}
  writeFileSync(path,JSON.stringify(valid));
  const before=readFileSync(path);
  const preview=run(join(root,'tools/Bootstrap-Project.ps1'),['-ConfigPath',path,'-WhatIf']);
  assert.equal(preview.status,0,output(preview));assert.deepEqual(readFileSync(path),before);
});

test('configured infrastructure catches missing locks, signers, exporter guards and version fields', t => {
  const dir=fixture(t);copyWorkingTree(root,dir);
  const gate=()=>run(join(dir,'tools/Test-TemplateInfrastructure.ps1'),['-RepositoryRoot',dir]);
  const initial=gate();assert.equal(initial.status,0,output(initial));
  for(const [relative,mutate,pattern] of [
    ['src/DarkSunWakeRedux.Extractor/packages.lock.json',()=>null,/packages.lock.json/],
    ['tools/Invoke-GpgSigner.ps1',()=>null,/Invoke-GpgSigner/],
    ['tools/ghidra/ExportEditionAnalysis.java',s=>s.replaceAll('requireLocalOutput','unsafeOutput'),/does not guard broad export/],
    ['tools/project-config.json',s=>{const c=JSON.parse(s);delete c.original.patchStatusEstablished;return JSON.stringify(c);},/patchStatusEstablished/],
    ['play.bat',s=>s.replaceAll('%*',''),/launcher is missing/]
  ]) {
    const path=join(dir,relative), saved=readFileSync(path);
    const changed=mutate(saved.toString());if(changed===null)rmSync(path);else writeFileSync(path,changed);
    const result=gate();assert.notEqual(result.status,0);assert.match(output(result),pattern);writeFileSync(path,saved);
  }
});

test('identity reconfiguration preserves recorded original facts and nested fingerprints', t => {
  const dir=fixture(t);copyWorkingTree(root,dir);
  const before=JSON.parse(readFileSync(join(dir,'tools/project-config.json'))).original;
  const result=run(join(dir,'tools/Configure-Project.ps1'),['-Force','-ProjectName','DarkSunWakeRedux','-DisplayName','Dark Sun: Wake of the Ravager Redux']);
  assert.equal(result.status,0,output(result));
  assert.deepEqual(JSON.parse(readFileSync(join(dir,'tools/project-config.json'))).original,before);
});

test('validation serializes a checkout and releases its lock even when a command fails', t => {
  const dir=fixture(t);mkdirSync(join(dir,'tools'));
  const source=readFileSync(join(root,'tools/Invoke-Validation.ps1'),'utf8');
  copyFileSync(join(root,'tools/Invoke-Validation.ps1'),join(dir,'tools/Invoke-Validation.ps1'));
  // Invoke a sibling script with production's unchanged initialization. Windows can expose
  // TEMP through an 8.3 alias while PSScriptRoot expands it; duplicating the hash uses another lock.
  const boundary=source.indexOf('$lock = $null');assert(boundary>0);
  writeFileSync(join(dir,'tools/GetValidationLock.ps1'),source.slice(0,boundary)+'\n$lockPath\n');
  const result=wrapper(t, `
$root=${literal(dir)}
$path=& (Join-Path $root 'tools/GetValidationLock.ps1')
$handle=[IO.File]::Open($path,'OpenOrCreate','ReadWrite','None')
try {
  try { & (Join-Path $root 'tools/Invoke-Validation.ps1'); throw 'accepted competing validation' }
  catch { if ($_.Exception.Message -notmatch 'Another validation run') { throw } }
} finally { $handle.Dispose() }
function dotnet { $global:LASTEXITCODE=7 }
try { & (Join-Path $root 'tools/Invoke-Validation.ps1'); throw 'accepted failed restore' }
catch { if ($_.Exception.Message -notmatch 'restore failed with exit code 7') { throw } }
$handle=[IO.File]::Open($path,'OpenOrCreate','ReadWrite','None');$handle.Dispose()
Remove-Item -LiteralPath $path
`);
  assert.equal(result.status,0,output(result));
});

test('eSigner refuses missing credentials and nonzero or silent-success failure output', t => {
  const env={...process.env};for(const name of ['ES_USERNAME','ES_PASSWORD','CREDENTIAL_ID','ES_TOTP_SECRET'])delete env[name];
  const result=run(join(root,'tools/Invoke-ESigner.ps1'),['-InputFile','synthetic.exe','-CodeSignToolPath',tmpdir()],{env});
  assert.notEqual(result.status,0);assert.match(output(result),/Missing eSigner environment/);
  const dir=fixture(t), fake=join(dir,'java.ps1');
  writeFileSync(fake,`param([Parameter(ValueFromRemainingArguments=$true)] $Rest)\nWrite-Output $env:SYNTHETIC_SIGNER_OUTPUT\nexit ([int]$env:SYNTHETIC_SIGNER_EXIT)\n`);
  // Execute the production native-output/exit contract with a constructed process double.
  const code=`
$source=[Management.Automation.Language.Parser]::ParseFile(${tool('Invoke-ESigner.ps1')},[ref]$null,[ref]$null)
$function=$source.Find({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Invoke-CodeSignTool'},$true)
Invoke-Expression $function.Extent.Text
$toolRoot=${literal(dir)};$java=${literal(fake)};$jar='synthetic.jar'
Invoke-CodeSignTool -Arguments @('sign')
`;
  for(const [text,exit,ok] of [['0 errors; signed',0,true],['ERROR: refused',0,false],['java.lang.Exception: refused',0,false],['Missing required option',0,false],['signed',9,false]]){
    const r=wrapper(t,code,{env:{...env,SYNTHETIC_SIGNER_OUTPUT:text,SYNTHETIC_SIGNER_EXIT:String(exit)}});
    assert.equal(r.status===0,ok,output(r));
  }
});

test('Authenticode acceptance rejects missing timestamps and unexpected certificates', t => {
  for(const [status,stamp,thumb,ok] of [['Valid',true,'a'.repeat(40),true],['NotSigned',true,'a'.repeat(40),false],['Valid',false,'a'.repeat(40),false],['Valid',true,'b'.repeat(40),false]]){
    const r=wrapper(t,`
function Get-AuthenticodeSignature {
  [pscustomobject]@{Status=${literal(status)};TimeStamperCertificate=${stamp?'[pscustomobject]@{}':'$null'};SignerCertificate=[pscustomobject]@{Thumbprint=${literal(thumb)}}}
}
& ${tool('Assert-WindowsSignature.ps1')} -Path 'synthetic.exe' -ExpectedThumbprint '${'a'.repeat(40)}'
`);
    assert.equal(r.status===0,ok,output(r));
  }
});

test('OpenPGP signs only with the expected good key, rejects expired/revoked statuses and cleans its home', t => {
  const dir=fixture(t), file=join(dir,'synthetic.deb');writeFileSync(file,'synthetic package');
  const fingerprint='A'.repeat(40);
  for(const [status,valid,ok] of [['GOODSIG',fingerprint,true],['REVKEYSIG',fingerprint,false],['EXPKEYSIG',fingerprint,false],['EXPSIG',fingerprint,false],['NEWSIG',fingerprint,false],['GOODSIG','B'.repeat(40),false]]){
    const r=wrapper(t,`
$env:GPG_PRIVATE_KEY='synthetic private-key stand-in';$env:GPG_PASSPHRASE='synthetic';$env:GPG_FINGERPRINT='${fingerprint}'
$env:GNUPGHOME='synthetic-previous-home';$global:syntheticCreatedHome=$null
function gpg {
  $global:syntheticCreatedHome=$env:GNUPGHOME;$global:LASTEXITCODE=0
  if ($args -contains '--list-secret-keys') { 'fpr:::::::::${fingerprint}:' }
  elseif ($args -contains '--detach-sign') { [IO.File]::WriteAllText($args[[Array]::IndexOf($args,'--output')+1],'synthetic signature') }
  elseif ($args -contains '--verify') { '[GNUPG:] ${status} synthetic';'[GNUPG:] VALIDSIG ${valid} synthetic ${valid}' }
}
function gpgconf { $global:LASTEXITCODE=0 }
$failed=$false
try { & ${tool('Invoke-GpgSigner.ps1')} -InputFile ${literal(file)} }
catch { $failed=$true; Write-Host $_.Exception.Message }
if ($failed -eq ${ok?'$true':'$false'}) { throw 'unexpected signing acceptance' }
if ($env:GNUPGHOME -ne 'synthetic-previous-home' -or (Test-Path -LiteralPath $global:syntheticCreatedHome)) { throw 'key-home cleanup failed' }
`);
    assert.equal(r.status,0,output(r));
  }
  const env={...process.env};for(const name of ['GPG_PRIVATE_KEY','GPG_PASSPHRASE','GPG_FINGERPRINT'])delete env[name];
  const missing=run(join(root,'tools/Invoke-GpgSigner.ps1'),['-TestConfiguration'],{env});
  assert.notEqual(missing.status,0);assert.match(output(missing),/Missing OpenPGP environment/);
});

// A player's cmd.exe finds play.bat in the current directory; agent shells set this variable, which stops that.
const withoutCwdLookupBlock=env=>Object.fromEntries(Object.entries(env).filter(([name])=>name.toLowerCase()!=='nodefaultcurrentdirectoryinexepath'));
test('play launcher forwards quoted arguments, bypasses source for smoke and preserves failures', {skip:process.platform!=='win32'}, t => {
  const dir=fixture(t), commands=join(dir,'commands');mkdirSync(commands);
  copyFileSync(join(root,'play.bat'),join(dir,'play.bat'));
  const fake=join(commands,'dotnet.cmd'), log=join(dir,'calls.log');
  writeFileSync(fake,`@echo off\r\necho %*>>"%SYNTHETIC_DOTNET_LOG%"\r\nif "%1"=="build" exit /b %SYNTHETIC_BUILD_EXIT%\r\nif "%1"=="run" if "%4"=="src\\DarkSunWakeRedux.Extractor" exit /b 13\r\nexit /b %SYNTHETIC_GAME_EXIT%\r\n`);
  const invoke=(args,build=0,game=0)=>{
    const entry=join(dir,'invoke.cmd');writeFileSync(entry,`@echo off\r\ncall play.bat ${args}\r\nexit /b %errorlevel%\r\n`);
    return spawnSync('cmd.exe',['/d','/c','invoke.cmd'],{cwd:dir,encoding:'utf8',env:{...withoutCwdLookupBlock(process.env),PATH:commands+';'+process.env.PATH,DARK_SUN_WAKE_PATH:join(dir,'never-present-source'),SYNTHETIC_DOTNET_LOG:log,SYNTHETIC_BUILD_EXIT:String(build),SYNTHETIC_GAME_EXIT:String(game)}});
  };
  for(const flag of ['--smoke-test','--platform-smoke-test']){
    rmSync(log,{force:true});const r=invoke(`${flag} --asset-pack "a path with spaces"`);assert.equal(r.status,0,output(r));
    const calls=readFileSync(log,'utf8');assert.doesNotMatch(calls,/Extractor/);assert.match(calls,/--asset-pack "a path with spaces"/);
  }
  assert.equal(invoke('--smoke-test',0,23).status,23);
  assert.equal(invoke('--smoke-test',5).status,1);
  const ordinary=invoke('');assert.notEqual(ordinary.status,0,output(ordinary)+'\n'+readFileSync(log,'utf8'));assert.match(output(ordinary),/Original Dark Sun assets were not found/);
});
