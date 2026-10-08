#Requires -Version 7.0
[CmdletBinding()]
param([Parameter(Mandatory)][string]$GhidraHome, [Parameter(Mandatory)][string]$JavaHome)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out = Join-Path $root ('artifacts/segment-effect-controls/' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($out) | Out-Null
$fixture = Join-Path $out 'segment-effects-synthetic.bin'
# Entirely synthetic: read DS, write DS, write SS, pop DS, LSS, LDS, INT3, NOP, WAIT, return.
[IO.File]::WriteAllBytes($fixture, [byte[]]@(0x8c,0xd8,0x8e,0xd8,0x8e,0xd0,0x1f,0x0f,0xb2,0,0xc5,0,0xcc,0x90,0x9b,0xc3))
$headless = Join-Path $GhidraHome 'support/analyzeHeadless.bat'
if (!(Test-Path -LiteralPath $headless -PathType Leaf)) { throw 'Ghidra launcher not found.' }
$scripts = '"' + (& (Join-Path $root 'tools/Get-GhidraScriptPath.ps1')) + '"'
$previousJava = $env:JAVA_HOME
try {
    $env:JAVA_HOME = $JavaHome
    $common = @('-import', $fixture, '-processor', 'x86:LE:32:default', '-loader', 'BinaryLoader',
        '-loader-baseAddr', '0x1000', '-noanalysis', '-scriptPath', $scripts,
        '-postScript', 'TestSegmentWrites.java')
    $validLog = Join-Path $out 'valid.log'
    & $headless $out SegmentEffectsValid @common -postScript ReportSegmentWrites.java 'DS+SS' 10 `
        -postScript ReportSegmentWrites.java 'DS+SS' 1 -deleteProject *> $validLog
    $validExit = $LASTEXITCODE
    $valid = [IO.File]::ReadAllText($validLog)
    if ($validExit -ne 0 -or $valid -notmatch 'Synthetic segment-output controls passed\.' -or
        $valid -notmatch 'scanned=10 matches=5 emitted=5 truncated=false opaqueInstructions=1 noPcodeInstructions=2' -or
        $valid -notmatch 'scanned=10 matches=5 emitted=1 truncated=true opaqueInstructions=1 noPcodeInstructions=2' -or
        $valid -notmatch 'opaqueAddress=0000100c' -or
        $valid -notmatch 'opaqueSitesEmitted=1 opaqueSitesTruncated=false' -or
        $valid -notmatch 'emptyMnemonic=NOP count=1 firstAddress=0000100d' -or
        $valid -notmatch 'emptyMnemonic=WAIT count=1 firstAddress=0000100e' -or
        $valid -notmatch 'emptyClasses=2 emptyClassesEmitted=2 emptyClassesTruncated=false' -or
        $valid -notmatch 'emptyClasses=2 emptyClassesEmitted=1 emptyClassesTruncated=true' -or
        $valid -match 'address=00001000 writes=') {
        throw "Synthetic valid segment controls failed; see $validLog"
    }
    $invalidLog = Join-Path $out 'invalid.log'
    & $headless $out SegmentEffectsInvalid @common -postScript ReportSegmentWrites.java 'DS+DS' 10 `
        -postScript ReportSegmentWrites.java 'DS+SS' 0 -deleteProject *> $invalidLog
    $invalidExit = $LASTEXITCODE
    $invalid = [IO.File]::ReadAllText($invalidLog)
    if ($invalidExit -ne 0 -or $invalid -notmatch 'Supply unique supported segment names' -or
        $invalid -notmatch 'Output limit must be 1\.\.10000' -or
        $invalid -match 'Completed decoded segment-output audit' -or
        $invalid -match 'ReportSegmentWrites.java> address=') {
        throw "Synthetic invalid segment controls failed; see $invalidLog"
    }
    Write-Host "Synthetic segment-output controls passed; logs: $out"
} finally { $env:JAVA_HOME = $previousJava }
