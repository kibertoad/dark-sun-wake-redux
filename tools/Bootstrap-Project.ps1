# One-time original-version readiness gate, adapted from the pinned template.
[CmdletBinding(SupportsShouldProcess = $true)]
param([string] $ConfigPath, [switch] $ValidateFactsOnly)
$ErrorActionPreference = 'Stop'
if (-not $ConfigPath) { $ConfigPath = Join-Path $PSScriptRoot 'project-config.json' }
$config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
$required = [ordered]@{
    projectName = $config.projectName
    displayName = $config.displayName
    'original.title' = $config.original.title
    'original.developer' = $config.original.developer
    'original.publisher' = $config.original.publisher
    'original.releaseYear' = $config.original.releaseYear
    'original.genre' = $config.original.genre
    'original.latestOfficialVersion' = $config.original.latestOfficialVersion
    'original.analysisVersion' = $config.original.analysisVersion
    'original.patchStatusEvidence' = $config.original.patchStatusEvidence
}
$missing = @($required.GetEnumerator() | Where-Object { [string]::IsNullOrWhiteSpace([string] $_.Value) } | ForEach-Object Key)
if ($missing.Count) { throw "Bootstrap facts are missing: $($missing -join ', '). See docs/SOURCE-EDITIONS.md." }
# A nonempty string such as 'false' is truthy in PowerShell; require a JSON boolean.
if ($config.original.patchStatusEstablished -isnot [bool] -or -not $config.original.patchStatusEstablished) {
    throw 'Patch status is not established. Resolve the provenance in docs/SOURCE-EDITIONS.md before executable analysis. Original-free validation remains available via tools/Invoke-Validation.ps1.'
}
if ($config.original.latestOfficialVersion -cne $config.original.analysisVersion) {
    throw "The analysis version '$($config.original.analysisVersion)' differs from latest official version '$($config.original.latestOfficialVersion)'. Patch the legally owned analysis copy before analysis."
}
$identity = $config.original.analysisExecutable
if (-not $identity -or ($identity.byteLength -isnot [long] -and $identity.byteLength -isnot [int]) -or
    $identity.byteLength -le 0 -or $identity.sha256 -notmatch '^[a-fA-F0-9]{64}$' -or
    [string]::IsNullOrWhiteSpace($identity.path)) {
    throw 'Established patch status requires original.analysisExecutable path, positive byteLength and a recorded SHA-256. Do not invent a fingerprint.'
}
if ($ValidateFactsOnly) { Write-Host 'Bootstrap version and identity facts verified; no files changed.'; return }
if ($config.configured) {
    # A completed identity must never be configured again as a side effect of checking readiness.
    if ($WhatIfPreference) { Write-Host 'Configured project: would verify configuration; no files changed.'; return }
} elseif ($PSCmdlet.ShouldProcess((Split-Path -Parent $PSScriptRoot), 'Configure restoration identity')) {
    & (Join-Path $PSScriptRoot 'Configure-Project.ps1') -ConfigPath $ConfigPath
}
& (Join-Path $PSScriptRoot 'Verify-Configuration.ps1') -Strict
if ($LASTEXITCODE -ne 0) { throw 'Project configuration is incomplete.' }
Write-Host 'Bootstrap readiness verified.'
