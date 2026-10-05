[CmdletBinding()]
param([switch] $NoRestore)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
$python = $env:EVIDENCE_PYTHON
if (-not $python) {
    $relative = if ($IsWindows -or $env:OS -eq 'Windows_NT') { 'Scripts/python.exe' } else { 'bin/python' }
    $python = Join-Path (Join-Path $root 'artifacts/evidence-python') $relative
}
if (-not (Test-Path -LiteralPath $python -PathType Leaf)) { throw 'Restore the locked evidence interpreter before the test-only emulator.' }
if (-not $NoRestore) {
    & $python -m pip install --require-hashes --only-binary=:all: -r (Join-Path $PSScriptRoot 'requirements.txt')
    if ($LASTEXITCODE -ne 0) { throw 'Hash-locked test-only Unicorn restore failed.' }
}
& $python -c 'import importlib.metadata; assert importlib.metadata.version("unicorn") == "2.1.4", "Expected test-only Unicorn 2.1.4"'
if ($LASTEXITCODE -ne 0) { throw 'Missing or mismatched test-only Unicorn; run tools/emu/Restore-Emulator.ps1.' }
