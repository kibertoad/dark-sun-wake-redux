[CmdletBinding()]
param([string] $RepositoryRoot)
$ErrorActionPreference = 'Stop'
if (-not $RepositoryRoot) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }
$RepositoryRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path
# The work protocol's size limits on its working files: 1,000 lines each, and 200 for the handover.
$limits = [ordered]@{}
Get-ChildItem -Path (Join-Path $RepositoryRoot 'queue'), (Join-Path $RepositoryRoot 'docs/goals'),
    (Join-Path $RepositoryRoot 'docs/live-sessions'), (Join-Path $RepositoryRoot 'docs/reports'),
    (Join-Path $RepositoryRoot 'docs/decisions') -Filter '*.md' -Recurse -File -ErrorAction SilentlyContinue |
    ForEach-Object { $limits[$_.FullName] = 1000 }
foreach ($path in 'docs/IMPLEMENTATION-PLAN.md', 'docs/DECISIONS.md', 'docs/RUNTIME.md') {
    $limits[(Join-Path $RepositoryRoot $path)] = 1000
}
$limits[(Join-Path $RepositoryRoot 'docs/HANDOVER.md')] = 200
foreach ($workingFile in $limits.Keys) {
    $lineCount = [IO.File]::ReadAllLines($workingFile).Length
    if ($lineCount -gt $limits[$workingFile]) {
        throw "$workingFile has $lineCount lines; the work protocol allows $($limits[$workingFile])."
    }
}
