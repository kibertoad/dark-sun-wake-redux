[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$SourcePath,
    [Parameter(Mandatory = $true)][string]$ResidentInventoryPath,
    [Parameter(Mandatory = $true)][string]$MappedInventoryPath,
    [Parameter(Mandatory = $true)][string]$OutputPath,
    [Parameter(Mandatory = $false)][string]$ManifestPath = 'DSUN.EXE'
)

$ErrorActionPreference = 'Stop'
if ($ManifestPath -notmatch '^[A-Za-z0-9_./:-]+$') {
    throw "Invalid manifest path for inventory address: $ManifestPath"
}
$map = & (Join-Path $PSScriptRoot 'ReportFbovOverlayMap.ps1') -SourcePath $SourcePath |
    ConvertFrom-Json
$headerBytes = [long]$map.HeaderBytes
$residentEnd = [Convert]::ToInt64($map.FbovFileOffset, 16)
$overlayRanges = @($map.Overlays | ForEach-Object {
    $start = [Convert]::ToInt64($_.CodeFileOffset, 16)
    [pscustomobject]@{ Start = $start; End = $start + [long]$_.CodeBytes }
})

function Read-Inventory([string]$Path) {
    if (-not [IO.File]::Exists($Path)) { throw "Inventory not found: $Path" }
    $rows = @(Import-Csv -LiteralPath $Path -Delimiter "`t")
    if ($rows.Count -eq 0) { throw "Inventory is empty: $Path" }
    foreach ($row in $rows) {
        if ($row.PSObject.Properties.Name -notcontains 'start' -or
            $row.PSObject.Properties.Name -notcontains 'size' -or
            $row.start -notmatch '^[0-9a-fA-F]{4}:[0-9a-fA-F]{4}$' -or
            $row.size -notmatch '^[1-9][0-9]*$') {
            throw "Invalid function inventory row in $Path"
        }
    }
    return $rows
}

function Get-FileOffset([string]$Address) {
    $parts = $Address.Split(':')
    return $headerBytes + ([Convert]::ToInt64($parts[0], 16) - 0x1000) * 16 +
        [Convert]::ToInt64($parts[1], 16)
}

$functions = @{}
$residentCount = 0
foreach ($row in (Read-Inventory $ResidentInventoryPath)) {
    $offset = Get-FileOffset $row.start
    if ($offset -lt $headerBytes -or $offset -ge $residentEnd) {
        throw "Resident function start is outside the resident image: $($row.start)"
    }
    if ($functions.ContainsKey($offset)) { throw "Duplicate resident file offset: $offset" }
    $functions[$offset] = [long]$row.size
    $residentCount++
}

$overlayCount = 0
foreach ($row in (Read-Inventory $MappedInventoryPath)) {
    $offset = Get-FileOffset $row.start
    $inOverlayCode = @($overlayRanges | Where-Object {
        $offset -ge $_.Start -and $offset -lt $_.End
    }).Count -eq 1
    if (-not $inOverlayCode) { continue }
    if ($functions.ContainsKey($offset)) { throw "Duplicate overlay file offset: $offset" }
    $functions[$offset] = [long]$row.size
    $overlayCount++
}

$outputFullPath = [IO.Path]::GetFullPath($OutputPath)
if ([IO.File]::Exists($outputFullPath)) { throw "Output already exists: $outputFullPath" }
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($outputFullPath)) | Out-Null
$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("start`tsize")
foreach ($offset in ($functions.Keys | Sort-Object)) {
    $lines.Add(($ManifestPath + ('+0x{0:X8}' -f [long]$offset)) + "`t" + $functions[$offset])
}
[IO.File]::WriteAllLines($outputFullPath, $lines, [Text.UTF8Encoding]::new($false))
Write-Output "Resident functions: $residentCount; overlay functions: $overlayCount; total: $($functions.Count)"
