[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
& (Join-Path $PSScriptRoot 'Verify-Repository.ps1') -RepositoryRoot $root
if ($LASTEXITCODE -ne 0) { throw 'Repository policy failed.' }
& (Join-Path $PSScriptRoot 'Verify-Configuration.ps1') -RepositoryRoot $root
if ($LASTEXITCODE -ne 0) { throw 'Project configuration is incomplete.' }
$physicalPatternFixture = Join-Path ([IO.Path]::GetTempPath()) (
    "dark-sun-wake-physical-pattern-{0}.bin" -f [Guid]::NewGuid().ToString('N'))
try {
    [IO.File]::WriteAllBytes($physicalPatternFixture, [byte[]]@(0, 0xaa, 0xbb, 0xaa, 0xbb, 0xcc))
    $physicalPatternReport = & (Join-Path $PSScriptRoot 'ghidra/ReportPhysicalBytePattern.ps1') `
        -SourcePath $physicalPatternFixture -Pattern 'aa bb' | ConvertFrom-Json
    if ($physicalPatternReport.SourceByteLength -ne 6 -or
        $physicalPatternReport.PatternByteLength -ne 2 -or
        $physicalPatternReport.MatchCount -ne 2 -or
        @($physicalPatternReport.Offsets).Count -ne 2 -or
        $physicalPatternReport.Offsets[0] -ne 1 -or $physicalPatternReport.Offsets[1] -ne 3) {
        throw 'Physical byte-pattern reporter synthetic contract failed.'
    }

    try {
        & (Join-Path $PSScriptRoot 'ghidra/ReportPhysicalBytePattern.ps1') -SourcePath $physicalPatternFixture -Pattern 'aa' -MaximumMatches 1 | Out-Null
        throw 'Physical byte-pattern reporter accepted more matches than its configured limit.'
    }
    catch {
        if ($_.Exception.Message -notmatch 'matches exceed the 1-result analysis limit') {
            throw
        }
    }
}
finally {
    if ([IO.File]::Exists($physicalPatternFixture)) { [IO.File]::Delete($physicalPatternFixture) }
}
# The documentation standard check, from the toolkit commit the CI workflow pins.
$documentationToolkitCommit = '6fe1e4133585d82458a83c5dac519720ae33adb5'
$documentationCheck = Join-Path $root "artifacts/check-documentation-$documentationToolkitCommit.mjs"
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    throw 'Node.js 20 or newer is required to run the documentation standard check.'
}
if (-not [IO.File]::Exists($documentationCheck)) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $documentationCheck) | Out-Null
    Invoke-WebRequest -UseBasicParsing -OutFile $documentationCheck `
        -Uri "https://raw.githubusercontent.com/kibertoad/refurbished-dinosaurs-toolkit/$documentationToolkitCommit/tools/check-documentation.mjs"
}
& node $documentationCheck --root $root --check
if ($LASTEXITCODE -ne 0) { throw 'Documentation standard check failed.' }
$artifacts = Join-Path $root 'artifacts/test'
dotnet test --project (Join-Path $root 'tests/DarkSunWakeRedux.Tests/DarkSunWakeRedux.Tests.csproj') `
  -p:UseSharedCompilation=false --artifacts-path $artifacts --no-progress -v minimal
if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
