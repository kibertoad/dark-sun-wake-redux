#Requires -Version 7.0
[CmdletBinding()]
param([Parameter(Mandatory)][string]$GameDirectory,[Parameter(Mandatory)][string]$OutputDirectory,
 [Parameter(Mandatory)][string]$GhidraHome,[Parameter(Mandatory)][string]$JavaHome)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out=[IO.Path]::GetFullPath($OutputDirectory)
if($out.StartsWith([IO.Path]::GetFullPath($root),[StringComparison]::OrdinalIgnoreCase)){throw 'Baseline analysis output must be outside the repository.'}
New-Item -ItemType Directory -Force -Path $out,(Join-Path $out 'snapshots') | Out-Null
$env:JAVA_HOME=$JavaHome
$headless=Join-Path $GhidraHome 'support/analyzeHeadless.bat'
$scripts='"'+(& (Join-Path $root 'tools/Get-GhidraScriptPath.ps1'))+'"'
$revision=(Get-FileHash (Join-Path $PSScriptRoot 'ExportResearchBaseline.java') -Algorithm SHA256).Hash.ToLower()
$runId=Get-Date -Format 'yyyyMMdd-HHmmss-ffff'
$records=[Collections.Generic.List[object]]::new()
function Measure-View($Key,$Source,$Manifest,$Mode,$Regions,$ExistingProject,$ExpectedHash) {
 $hash=(& node (Join-Path $root 'tools/evidence/xxh3.mjs') $Source).Trim().Split(' ')[0]
 if($LASTEXITCODE -ne 0 -or $hash -notmatch '^[0-9a-f]{32}$'){throw "Hash failed for $Key"}
 if($ExpectedHash -and $hash -ne $ExpectedHash){throw "Licensed source identity differs for $Key"}
 $md5=(Get-FileHash -LiteralPath $Source -Algorithm MD5).Hash.ToLower()
 $projectRoot=Join-Path $out 'snapshots';$project=$Key;$program=Split-Path -Leaf $Source
 if($ExistingProject){$projectRoot=Split-Path -Parent $ExistingProject;$project=[IO.Path]::GetFileNameWithoutExtension($ExistingProject)}
 elseif(!(Test-Path -LiteralPath (Join-Path $projectRoot ($project+'.gpr')))){
   & $headless $projectRoot $project -import $Source *> (Join-Path $out ($Key+'-import.log'))
   if($LASTEXITCODE -ne 0 -or !(Test-Path -LiteralPath (Join-Path $projectRoot ($project+'.gpr')))){throw "Import failed for $Key"}
 }
 $baseA=Join-Path $out ($Key+'-'+$runId+'-a');$baseB=Join-Path $out ($Key+'-'+$runId+'-b')
 $arguments=@($projectRoot,$project,'-process',$program,'-noanalysis','-readOnly','-scriptPath',$scripts)
 foreach($stem in @($baseA,$baseB)){
   $arguments+=@('-postScript','ExportResearchBaseline.java',$stem,$hash,($Key+'-'+$runId),$revision,$Mode,$md5)+@($Regions)
 }
 $log=Join-Path $out ($Key+'-export.log')
 & $headless @arguments *> $log
 if($LASTEXITCODE -ne 0 -or @((Select-String -LiteralPath $log -Pattern 'BASELINE_COMPLETE')).Count -ne 2){throw "Export did not complete twice for $Key; see $log"}
 foreach($suffix in @('.tsv','.provenance.tsv','.regions.tsv')){
   if(!(Test-Path -LiteralPath ($baseA+$suffix)) -or !(Test-Path -LiteralPath ($baseB+$suffix)) -or
      (Get-FileHash ($baseA+$suffix)).Hash -ne (Get-FileHash ($baseB+$suffix)).Hash){throw "Duplicate exports differ for $Key $suffix"}
 }
 $records.Add([pscustomobject]@{key=$Key;manifest=$Manifest;source=$Source;xxh3=$hash;md5=$md5;mode=$Mode;regions=@($Regions);stem=$baseA;project=(Join-Path $projectRoot ($project+'.gpr'));exporter=$revision})
 $records | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $out 'views.json') -Encoding utf8
 Write-Host "Measured $Key; duplicate exports agree."
}
# Shipped hashes are verified against the manifest, rather than embedded in tooling.
$manifest=Get-Content (Join-Path $root 'spec/builds/BLD-GOG-EN-1.1.files.yaml') -Raw
function Manifest-Hash($Name){
 $escaped=[regex]::Escape($Name)
 $match=[regex]::Match($manifest,'(?m)^  - path: '+$escaped+'\r?\n(?:(?!  - path:)[\s\S])*?    xxh3: ([0-9a-f]{32})')
 if(!$match.Success){throw "No manifest hash for $Name"};$match.Groups[1].Value
}
Measure-View 'dosbox-v2' (Join-Path $GameDirectory 'DOSBOX/DOSBox.exe') 'DOSBOX/DOSBox.exe' 'executable' @() (Join-Path $GameDirectory 'analysis/exe-batches/dosbox-pe-ghidra/DOSBoxPeReading.gpr') (Manifest-Hash 'DOSBOX/DOSBox.exe')
foreach($name in @('SOUND_DS.EXE','SVIEW.EXE','PATCH.EXE')){Measure-View ([IO.Path]::GetFileNameWithoutExtension($name)) (Join-Path $GameDirectory $name) $name 'initialized' @() $null (Manifest-Hash $name)}
Measure-View 'CHARTRAN-unpacked' (Join-Path $out 'sources/CHARTRAN-UNPACKED.EXE') 'CHARTRAN.EXE' 'initialized' @() $null 'a2804715759141397dca547934213843' # FND-PARTY-010
foreach($edition in @('INST','CD')){
 $source=if($edition -eq 'INST'){Join-Path $GameDirectory 'DSUN.EXE'}else{Join-Path $out 'sources/CD-DSUN.EXE'}
 $name=if($edition -eq 'INST'){'DSUN.EXE'}else{'CD:DSUN.EXE'}
 Measure-View ($edition+'-resident') $source $name 'initialized' @() $null (Manifest-Hash $name)
 $map=& (Join-Path $PSScriptRoot 'ReportFbovOverlayMap.ps1') -SourcePath $source | ConvertFrom-Json
 if($LASTEXITCODE -ne 0){throw 'Overlay mapping failed.'}
 $regions=@($map.Overlays | ForEach-Object {$_.MappedCodeStart+'..'+$_.MappedCodeEnd})
 $map | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $out ($edition+'-mapping.json')) -Encoding utf8
 Measure-View ($edition+'-overlay') (Join-Path $out ('sources/'+$edition+'-MAPPED.EXE')) $name 'initialized' $regions $null $null
}
