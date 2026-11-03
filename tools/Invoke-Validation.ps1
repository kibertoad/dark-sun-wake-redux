[CmdletBinding()]
param(
    [ValidateRange(1, 16)][int] $MaxCpuCount = 2,
    [string] $TestFilter,
    [ValidateRange(0, 1000000)][int] $MinimumExpectedTests = 0,
    [switch] $NoRestore,
    [string] $ChangedSince,
    [switch] $Research,
    [switch] $Integration
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$hash = [Security.Cryptography.SHA256]::Create()
try { $id = [BitConverter]::ToString($hash.ComputeHash([Text.Encoding]::UTF8.GetBytes($root.ToUpperInvariant()))).Replace('-', '') }
finally { $hash.Dispose() }
$lockPath = Join-Path ([IO.Path]::GetTempPath()) "restoration-validation-$($id.Substring(0, 16)).lock"
$lock = $null
function Invoke-CheckedDotnet([string[]] $Arguments) {
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($Arguments[0]) failed with exit code $LASTEXITCODE." }
}
try {
    try { $lock = [IO.File]::Open($lockPath, 'OpenOrCreate', 'ReadWrite', 'None') }
    catch [IO.IOException] { throw "Another validation run is already active for this checkout ($lockPath)." }
    if ($Research -and (-not $ChangedSince -or $Integration -or $env:CI -or $TestFilter -or $MinimumExpectedTests)) {
        throw 'Research needs -ChangedSince and cannot request rebuild tests or integration; run integration separately.'
    }
    $suiteBase = if ($Integration -or $env:CI -or -not $ChangedSince) { '--integration' } else { $ChangedSince }
    [string[]] $suiteArguments = @($suiteBase)
    if ($Research) { $suiteArguments = @('--research') + $suiteArguments }
    $selectionJson = & node (Join-Path $PSScriptRoot 'Select-ValidationSuites.mjs') @suiteArguments
    if ($LASTEXITCODE -ne 0) { throw 'Could not select validation suites.' }
    try { $selection = $selectionJson | ConvertFrom-Json -ErrorAction Stop }
    catch { throw 'Could not parse validation suite plan.' }
    if ($selection.mode -notin @('selected', 'integration') -or $selection.dotnet -isnot [bool]) {
        throw 'Invalid validation suite plan.'
    }
    if ($Research -and ($selection.dotnet -or $selection.mode -eq 'integration')) {
        throw 'Research selection returned rebuild tests; run integration separately.'
    }
    $runDotnet = $selection.dotnet -or $TestFilter -or $MinimumExpectedTests
    if ($selection.mode -eq 'integration' -and $TestFilter) {
        Write-Host 'Integration is required; manual filters cannot narrow acceptance.'
        $TestFilter = $null
    }
    if ($NoRestore) {
        Write-Host 'NoRestore: using existing restore state for this checkout and test artifact paths; no restore fallback.'
    }
    elseif ($runDotnet) {
        Invoke-CheckedDotnet -Arguments @('restore', (Join-Path $root 'DarkSunWakeRedux.slnx'), '--locked-mode', "-maxCpuCount:$MaxCpuCount")
    }
    $testParameters = @{ MinimumExpectedTests = $MinimumExpectedTests }
    if ($ChangedSince) { $testParameters.ChangedSince = $ChangedSince }
    if ($Research) { $testParameters.Research = $true }
    if ($Integration) { $testParameters.Integration = $true }
    if ($NoRestore) { $testParameters.NoRestore = $true }
    if ($TestFilter) { $testParameters.TestFilter = $TestFilter; Write-Warning 'Filtered .NET validation is partial acceptance; all tooling/policy checks still run.' }
    & (Join-Path $PSScriptRoot 'Test.ps1') @testParameters
    if ($LASTEXITCODE -ne 0) { throw 'Canonical validation failed.' }
    if (-not $runDotnet) { return }
    Invoke-CheckedDotnet -Arguments @('build', (Join-Path $root 'DarkSunWakeRedux.slnx'), '-c', 'Release', '--no-restore', "-maxCpuCount:$MaxCpuCount", '-p:UseSharedCompilation=false', '-v', 'minimal')
    $output = Join-Path $root 'artifacts/validation/assetless-game'
    Invoke-CheckedDotnet -Arguments @('publish', (Join-Path $root 'src/DarkSunWakeRedux.Game/DarkSunWakeRedux.Game.csproj'), '-c', 'Release', '--no-restore', '--output', $output, '-p:UseSharedCompilation=false', '-v', 'minimal')
    if (Test-Path -LiteralPath (Join-Path $output 'UserContent')) { throw 'Assetless publish contains UserContent.' }
    Invoke-CheckedDotnet -Arguments @((Join-Path $output 'DarkSunWakeRedux.Game.dll'), '--smoke-test')
    Write-Host 'Validation passed: locked dependencies, canonical checks, Release build and assetless publish/smoke.'
} finally { if ($lock) { $lock.Dispose() } }
