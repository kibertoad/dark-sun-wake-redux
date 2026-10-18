[CmdletBinding()]
param(
    [ValidateRange(1, 16)][int] $MaxCpuCount = 2,
    [string] $TestFilter,
    [ValidateRange(0, 1000000)][int] $MinimumExpectedTests = 0,
    [switch] $NoRestore,
    [string] $ChangedSince
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
    if ($ChangedSince) {
        $selection = & node (Join-Path $PSScriptRoot 'Select-Validation.mjs') $ChangedSince
        if ($LASTEXITCODE -ne 0 -or $selection -notin @('documentation', 'full')) {
            throw 'Could not classify changes; provide a valid base commit.'
        }
        $documentationOnly = $selection -eq 'documentation' -and -not ($TestFilter -or $MinimumExpectedTests)
        if ($documentationOnly) {
            & (Join-Path $PSScriptRoot 'Restore-ToolDependencies.ps1') -NoRestore:$NoRestore
            if ($LASTEXITCODE -ne 0) { throw 'Tool dependency verification failed.' }
            foreach ($check in @('Verify-Repository', 'Verify-Configuration', 'Test-TemplateInfrastructure')) {
                & (Join-Path $PSScriptRoot "$check.ps1") -RepositoryRoot $root
                if ($LASTEXITCODE -ne 0) { throw "$check failed." }
            }
            & node (Join-Path $PSScriptRoot 'upstream.mjs') links
            if ($LASTEXITCODE -ne 0) { throw 'Section link checks failed.' }
            & node (Join-Path $PSScriptRoot 'Invoke-NodeChecks.mjs')
            if ($LASTEXITCODE -ne 0) { throw 'Documentation or queue checks failed.' }
            Write-Host 'Documentation-only checks passed; full Test.ps1 remains required before the batch commit.'
            return
        }
        if (-not $TestFilter) { throw 'The change requires -TestFilter naming the tests that cover it.' }
    }
    if ($NoRestore) {
        Write-Host 'NoRestore: using existing restore state for this checkout and test artifact paths; no restore fallback.'
    }
    else {
        Invoke-CheckedDotnet -Arguments @('restore', (Join-Path $root 'DarkSunWakeRedux.slnx'), '--locked-mode', "-maxCpuCount:$MaxCpuCount")
    }
    $testParameters = @{ MinimumExpectedTests = $MinimumExpectedTests }
    if ($NoRestore) { $testParameters.NoRestore = $true }
    if ($TestFilter) { $testParameters.TestFilter = $TestFilter; Write-Warning 'Filtered .NET validation is partial acceptance; all tooling/policy checks still run.' }
    & (Join-Path $PSScriptRoot 'Test.ps1') @testParameters
    if ($LASTEXITCODE -ne 0) { throw 'Canonical validation failed.' }
    Invoke-CheckedDotnet -Arguments @('build', (Join-Path $root 'DarkSunWakeRedux.slnx'), '-c', 'Release', '--no-restore', "-maxCpuCount:$MaxCpuCount", '-p:UseSharedCompilation=false', '-v', 'minimal')
    $output = Join-Path $root 'artifacts/validation/assetless-game'
    Invoke-CheckedDotnet -Arguments @('publish', (Join-Path $root 'src/DarkSunWakeRedux.Game/DarkSunWakeRedux.Game.csproj'), '-c', 'Release', '--no-restore', '--output', $output, '-p:UseSharedCompilation=false', '-v', 'minimal')
    if (Test-Path -LiteralPath (Join-Path $output 'UserContent')) { throw 'Assetless publish contains UserContent.' }
    Invoke-CheckedDotnet -Arguments @((Join-Path $output 'DarkSunWakeRedux.Game.dll'), '--smoke-test')
    Write-Host 'Validation passed: locked dependencies, canonical checks, Release build and assetless publish/smoke.'
} finally { if ($lock) { $lock.Dispose() } }
