[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$SourcePath,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Pattern,

    [ValidateRange(1, 65536)]
    [int]$MaximumMatches = 16384
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
$matchCount = 0
$lastOffset = $haystack.Length - $needle.Length
$offset = 0
while ($offset -le $lastOffset) {
    # Array.IndexOf skips to the next candidate first byte in compiled code; a script loop over
    # every byte of a 128 MB file would take minutes.
    $offset = [Array]::IndexOf($haystack, $needle[0], $offset, $lastOffset - $offset + 1)
    if ($offset -lt 0) { break }
    $isMatch = $true
    for ($needleOffset = 1; $needleOffset -lt $needle.Length; $needleOffset++) {
        if ($haystack[$offset + $needleOffset] -ne $needle[$needleOffset]) {
            $isMatch = $false
            break
        }
    }
    if ($isMatch) {
        $matchCount++
        if ($matchCount -gt $MaximumMatches) {
            throw "Pattern matches exceed the $MaximumMatches-result analysis limit."
        }

        $offsets.Add($offset)
    }
    $offset++
}

[pscustomobject]@{
    SourcePath = $sourceFullPath
    SourceByteLength = $haystack.Length
    PatternByteLength = $needle.Length
    MatchCount = $matchCount
    Offsets = $offsets
} | ConvertTo-Json -Compress
