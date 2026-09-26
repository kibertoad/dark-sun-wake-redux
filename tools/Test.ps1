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
$overlayMapFixture = Join-Path ([IO.Path]::GetTempPath()) (
    "dark-sun-wake-overlay-map-{0}.bin" -f [Guid]::NewGuid().ToString('N'))
try {
    # A 144-byte MZ file: a 32-byte header, one overlay stub at segment 1, a one-entry FBOV segment
    # table at 0x60, and an FBOV pack at 0x70 whose 16 bytes of overlay code start at 0x80.
    $overlayMapBytes = New-Object byte[] 144
    $overlayMapBytes[0] = 0x4d; $overlayMapBytes[1] = 0x5a
    $overlayMapBytes[2] = 112; $overlayMapBytes[4] = 1; $overlayMapBytes[8] = 2; $overlayMapBytes[24] = 0x1c
    $overlayMapBytes[48] = 0xcd; $overlayMapBytes[49] = 0x3f; $overlayMapBytes[56] = 16
    $overlayMapBytes[96] = 1; $overlayMapBytes[100] = 2
    [Text.Encoding]::ASCII.GetBytes('FBOV').CopyTo($overlayMapBytes, 112)
    $overlayMapBytes[116] = 16; $overlayMapBytes[120] = 96; $overlayMapBytes[124] = 1
    [IO.File]::WriteAllBytes($overlayMapFixture, $overlayMapBytes)
    $overlayMap = & (Join-Path $PSScriptRoot 'ghidra/ReportFbovOverlayMap.ps1') `
        -SourcePath $overlayMapFixture -MappedAddress '1006:0004', '1001:0000' | ConvertFrom-Json
    if ($overlayMap.OverlayCount -ne 1 -or
        $overlayMap.FbovFileOffset -ne '0x00000070' -or
        $overlayMap.Overlays[0].HeaderSegment -ne '1001' -or
        $overlayMap.Overlays[0].CodeFileOffset -ne '0x00000080' -or
        $overlayMap.Overlays[0].MappedCodeStart -ne '1006:0000' -or
        $overlayMap.Conversions[0].FileOffset -ne '0x00000084' -or
        $overlayMap.Conversions[0].Region -ne 'overlay' -or
        $overlayMap.Conversions[0].OffsetInOverlayCode -ne '0x0004' -or
        $overlayMap.Conversions[1].FileOffset -ne '0x00000030' -or
        $overlayMap.Conversions[1].Region -ne 'resident') {
        throw 'FBOV overlay map reporter synthetic contract failed.'
    }
}
finally {
    if ([IO.File]::Exists($overlayMapFixture)) { [IO.File]::Delete($overlayMapFixture) }
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
