#Requires -Version 7.0
[CmdletBinding()]
param([Parameter(Mandatory)][string]$GhidraHome, [Parameter(Mandatory)][string]$JavaHome)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out = Join-Path $root ('artifacts/entry-data-controls/' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($out) | Out-Null
$fixture = Join-Path $out 'invented-entry-data.bin'
# Invented MOV immediate, NOP fill, two RETs; no licensed bytes are used.
[byte[]]$bytes = @(0x90) * 64
$bytes[0] = 0xB8; $bytes[1] = 0x11; $bytes[2] = 0x22; $bytes[3] = 0x33; $bytes[4] = 0x44
$bytes[31] = 0xC3; $bytes[32] = 0xC3
[IO.File]::WriteAllBytes($fixture, $bytes)
$headless = Join-Path $GhidraHome 'support/analyzeHeadless.bat'
if (!(Test-Path -LiteralPath $headless -PathType Leaf)) { throw 'Ghidra launcher not found.' }
$scripts = '"' + (& (Join-Path $root 'tools/Get-GhidraScriptPath.ps1')) + '"'
$previousJava = $env:JAVA_HOME
try {
    $env:JAVA_HOME = $JavaHome
    $log = Join-Path $out 'controls.log'
    & $headless $out EntryDataSynthetic -import $fixture -processor x86:LE:32:default `
        -loader BinaryLoader -loader-baseAddr 0x1000 -noanalysis -scriptPath $scripts `
        -postScript TestEntryDataBoundary.java -deleteProject *> $log
    $result = $LASTEXITCODE
    $text = [IO.File]::ReadAllText($log)
    if ($result -ne 0 -or $text -notmatch 'ENTRY_DATA_SYNTHETIC_COMPLETE\s+guards=9 rollback=true bytesUnchanged=true untouched=true idempotent=true' -or
        ([regex]::Matches($text, 'BOUNDARY_REJECTED\s+').Count -ne 9)) {
        throw "Entry/data synthetic controls failed; see $log"
    }
    Write-Host "Entry/data synthetic controls passed: $log"
}
finally { $env:JAVA_HOME = $previousJava }
