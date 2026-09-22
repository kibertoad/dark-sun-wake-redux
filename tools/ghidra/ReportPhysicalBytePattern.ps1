[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$SourcePath,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Pattern
)

$ErrorActionPreference = 'Stop'
$maximumSourceBytes = 128MB
$maximumPatternBytes = 64

$sourceFullPath = [IO.Path]::GetFullPath($SourcePath)
if (-not (Test-Path -LiteralPath $sourceFullPath -PathType Leaf)) {
    throw "SourcePath does not name a file: $sourceFullPath"
}

$compactPattern = $Pattern.Replace(' ', '').Replace('_', '')
if ($compactPattern.Length -eq 0 -or ($compactPattern.Length % 2) -ne 0 -or
    $compactPattern.Length -gt $maximumPatternBytes * 2 -or
    $compactPattern -notmatch '^[0-9a-fA-F]+$') {
    throw "Pattern must contain 1 through $maximumPatternBytes complete hexadecimal bytes."
}

$sourceInfo = Get-Item -LiteralPath $sourceFullPath
if ($sourceInfo.Length -gt $maximumSourceBytes) {
    throw "SourcePath exceeds the $maximumSourceBytes-byte analysis limit."
}

[byte[]]$needle = for ($index = 0; $index -lt $compactPattern.Length; $index += 2) {
    [Convert]::ToByte($compactPattern.Substring($index, 2), 16)
}
[byte[]]$haystack = [IO.File]::ReadAllBytes($sourceFullPath)
$offsets = [Collections.Generic.List[int]]::new()
for ($offset = 0; $offset -le $haystack.Length - $needle.Length; $offset++) {
    $matches = $true
    for ($needleOffset = 0; $needleOffset -lt $needle.Length; $needleOffset++) {
        if ($haystack[$offset + $needleOffset] -ne $needle[$needleOffset]) {
            $matches = $false
            break
        }
    }
    if ($matches) { $offsets.Add($offset) }
}

[pscustomobject]@{
    SourcePath = $sourceFullPath
    SourceByteLength = $haystack.Length
    PatternByteLength = $needle.Length
    MatchCount = $offsets.Count
    Offsets = $offsets
} | ConvertTo-Json -Compress
