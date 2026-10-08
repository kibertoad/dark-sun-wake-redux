#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$GhidraHome,
    [Parameter(Mandatory)][string]$JavaHome
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$out = Join-Path $root ('artifacts/citation-boundary-controls/' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($out) | Out-Null
$fixture = Join-Path $out 'citation-boundaries-synthetic.bin'
# Synthetic NOP, six-byte conditional branch to RET, then undecoded mapped bytes.
[byte[]]$bytes = @(0x90, 0x0f, 0x85, 0, 0, 0, 0, 0xc3, 0, 0, 0, 0, 0, 0, 0, 0)
[IO.File]::WriteAllBytes($fixture, $bytes)
$headless = Join-Path $GhidraHome 'support/analyzeHeadless.bat'
if (!(Test-Path -LiteralPath $headless -PathType Leaf)) { throw 'Windows Ghidra headless launcher was not found.' }
$scripts = '"' + (& (Join-Path $root 'tools/Get-GhidraScriptPath.ps1')) + '"'
$previousJava = $env:JAVA_HOME
try {
    $env:JAVA_HOME = $JavaHome
    $common = @('-import', $fixture, '-processor', 'x86:LE:32:default', '-loader', 'BinaryLoader',
        '-loader-baseAddr', '0x1000', '-noanalysis', '-scriptPath', $scripts,
        '-postScript', 'TestCitationBoundaries.java')
    $validLog = Join-Path $out 'valid.log'
    & $headless $out CitationBoundaryValid @common -postScript ReportCitationBoundaries.java `
        1000..1008:return 1000..1006:return 1000..1007:return -deleteProject *> $validLog
    $validExit = $LASTEXITCODE
    $valid = [IO.File]::ReadAllText($validLog)
    if ($validExit -ne 0 -or $valid -notmatch 'Synthetic citation endpoint controls passed\.' -or
        $valid -notmatch 'end=00001008 startState=aligned endState=aligned returnState=present' -or
        $valid -notmatch 'end=00001006 startState=aligned endState=interior returnState=unknown' -or
        $valid -notmatch 'end=00001007 startState=aligned endState=aligned returnState=missing' -or
        $valid -notmatch 'Completed 3 endpoint queries') {
        throw "Synthetic valid endpoint controls failed; see $validLog"
    }
    $invalidLog = Join-Path $out 'invalid.log'
    $excessive = @('1000..1008') * 33
    & $headless $out CitationBoundaryInvalid @common -postScript ReportCitationBoundaries.java `
        1000..1008:return 1008..1000 -postScript ReportCitationBoundaries.java @excessive `
        -deleteProject *> $invalidLog
    $invalidExit = $LASTEXITCODE
    $invalid = [IO.File]::ReadAllText($invalidLog)
    # Headless may exit successfully after a script throws, so require the exact
    # intended rejections and forbid partial reporter results instead of trusting exit alone.
    if ($invalidExit -ne 0 -or $invalid -notmatch 'Synthetic citation endpoint controls passed\.' -or
        $invalid -notmatch 'Range must be nonempty' -or
        $invalid -notmatch 'Supply 1\.\.32' -or
        $invalid -match 'ReportCitationBoundaries.java> start=' -or
        $invalid -match 'Completed \d+ endpoint queries') {
        throw "Synthetic invalid endpoint controls failed; see $invalidLog"
    }
    Write-Host "Synthetic citation endpoint controls passed; logs: $out"
} finally {
    $env:JAVA_HOME = $previousJava
}
