#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$AnalysisRoot,
    [Parameter(Mandatory)][string]$CanonicalProjectRoot,
    [string]$InventoryPath = (Join-Path $PSScriptRoot '../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv')
)
$ErrorActionPreference = 'Stop'
function Read-Rows([string]$Path) {
    if (!(Test-Path -LiteralPath $Path -PathType Leaf) -or (Get-Item -LiteralPath $Path).Length -gt 16MB) {
        throw 'Missing or excessive metadata input'
    }
    @(Import-Csv -LiteralPath $Path -Delimiter "`t")
}
function Inventory-Map([object[]]$Rows) {
    $map = @{}
    foreach ($row in $Rows) {
        $key = $row.start.ToUpperInvariant()
        if ($map.ContainsKey($key)) { throw 'Duplicate inventory entry' }
        $map[$key] = "$key`t$($row.size)`t$($row.ranges.ToUpperInvariant())"
    }
    $map
}
foreach ($region in @('main','boundary')) {
    foreach ($suffix in @('.tsv','.regions.tsv','.provenance.tsv')) {
        $a = Join-Path $AnalysisRoot ('after-'+$region+'-a'+$suffix)
        $b = Join-Path $AnalysisRoot ('after-'+$region+'-b'+$suffix)
        if ((Get-FileHash -LiteralPath $a).Hash -ne (Get-FileHash -LiteralPath $b).Hash) { throw 'Duplicate exports differ' }
    }
}
$before = Inventory-Map (Read-Rows (Join-Path $AnalysisRoot 'before-main.tsv'))
$after = Inventory-Map (Read-Rows (Join-Path $AnalysisRoot 'after-main-a.tsv'))
if ($before.Count -ne 212 -or $after.Count -ne 213 -or $before.ContainsKey('1000:02DF') -or
    $before['1000:02AD'] -ne "1000:02AD`t80`t1000:02AD..1000:02FD" -or
    $after['1000:02AD'] -ne "1000:02AD`t23`t1000:02AD..1000:02C4" -or
    $after['1000:02DF'] -ne "1000:02DF`t30`t1000:02DF..1000:02FD") { throw 'Unexpected boundary row delta' }
foreach ($entry in $before.Keys) {
    if ($entry -ne '1000:02AD' -and $before[$entry] -ne $after[$entry]) { throw 'Unrelated exported row changed' }
}
foreach ($stem in @('before-main','before-boundary','after-main-a','after-boundary-a')) {
    $p = @{}; foreach ($row in (Read-Rows (Join-Path $AnalysisRoot ($stem+'.provenance.tsv')))) { $p[$row.key]=$row.value }
    if ($p.source_md5 -ne '90b09ee26f2bc3a8d20063712636241c' -or
        $p.xxh3 -ne 'e296af55ba2ecde7e77f555c90f33d0b' -or
        $p.body_bytes_outside_regions -ne '0' -or $p.entries_without_instruction -ne '0') { throw 'Export provenance or anomaly check failed' }
}
$priorAudit = Read-Rows (Join-Path $AnalysisRoot 'before-main.regions.tsv')
$nextAudit = Read-Rows (Join-Path $AnalysisRoot 'after-main-a.regions.tsv')
if ($priorAudit.Count -ne 1 -or $nextAudit.Count -ne 1) { throw 'Unexpected main region census' }
foreach ($column in @('size','instructions_outside','undefined','undefined_outside')) {
    if ($priorAudit[0].$column -ne $nextAudit[0].$column) { throw 'Unexpected region-class delta' }
}
if ([long]$nextAudit[0].instructions -ne [long]$priorAudit[0].instructions-27 -or
    [long]$nextAudit[0].data -ne [long]$priorAudit[0].data+27 -or
    [long]$nextAudit[0].data_outside -ne [long]$priorAudit[0].data_outside+27) { throw 'Instruction/data delta disagrees' }
$narrow = Read-Rows (Join-Path $AnalysisRoot 'after-boundary-a.regions.tsv')
if ($narrow.Count -ne 1 -or $narrow[0].size -ne '80' -or $narrow[0].instructions -ne '53' -or
    $narrow[0].data -ne '27' -or $narrow[0].undefined -ne '0') { throw 'Narrow partition is incomplete' }
$joined = Inventory-Map (Read-Rows $InventoryPath)
$reconciled = $joined['1000:02AD'] -eq $after['1000:02AD'] -and $joined['1000:02DF'] -eq $after['1000:02DF']
if (!$reconciled -and ($joined['1000:02AD'] -ne $before['1000:02AD'] -or $joined.ContainsKey('1000:02DF'))) { throw 'Joined boundary matches neither measured state' }
foreach ($entry in $before.Keys) {
    if ($entry -ne '1000:02AD' -and $joined[$entry] -ne $before[$entry]) { throw 'Joined inventory has an unrelated main-region difference' }
}
$manifestPath = Join-Path $AnalysisRoot 'canonical-before.json'
if ((Get-Item -LiteralPath $manifestPath).Length -gt 65536) { throw 'Excessive canonical manifest' }
$canonical = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$canonicalRoot = [IO.Path]::GetFullPath($CanonicalProjectRoot).TrimEnd('\','/')+[IO.Path]::DirectorySeparatorChar
foreach ($row in $canonical) {
    if ([IO.Path]::IsPathRooted($row.relative)) { throw 'Canonical member must be relative' }
    $path = [IO.Path]::GetFullPath((Join-Path $canonicalRoot $row.relative))
    if (!$path.StartsWith($canonicalRoot,[StringComparison]::OrdinalIgnoreCase) -or
        (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $row.sha256) { throw 'Canonical project changed' }
}
Write-Host "RUNTIME_ERROR_EXPORTS_VERIFIED priorFunctions=212 newFunctions=213 untouched=211 reconciled=$reconciled canonicalUnchanged=true"
