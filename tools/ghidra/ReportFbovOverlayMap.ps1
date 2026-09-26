[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$SourcePath,

    # Addresses in the image New-FbovMappedImage.ps1 builds, as Ghidra shows them with the load
    # image at segment 1000 (for example 5000:a4b9). Each is converted to the file offset of the
    # same byte in SourcePath.
    [string[]]$MappedAddress = @()
)

# Prints, for each overlay in the FBOV pack of a DSUN.EXE-style executable, the segment of its
# resident header, the file offset and length of its code, and the segment:offset that code has in
# the mapped image. The mapped image keeps every byte after the MZ header in place and only
# rewrites the header, so a mapped address converts to a file offset in SourcePath as
# header bytes + (segment - 0x1000) * 16 + offset. Overlay code is cited by that file offset.

$ErrorActionPreference = 'Stop'
$maximumSourceBytes = 16MB
$loadSegment = 0x1000

$sourceFullPath = [IO.Path]::GetFullPath($SourcePath)
if (-not (Test-Path -LiteralPath $sourceFullPath -PathType Leaf)) {
    throw "SourcePath does not name a file: $sourceFullPath"
}
if ((Get-Item -LiteralPath $sourceFullPath).Length -gt $maximumSourceBytes) {
    throw "SourcePath exceeds the $maximumSourceBytes-byte analysis limit."
}

function Read-UInt16([byte[]]$Bytes, [long]$Offset) {
    if ($Offset -lt 0 -or $Offset -gt $Bytes.Length - 2) { throw "u16 offset $Offset is out of range." }
    return [BitConverter]::ToUInt16($Bytes, $Offset)
}

function Read-UInt32([byte[]]$Bytes, [long]$Offset) {
    if ($Offset -lt 0 -or $Offset -gt $Bytes.Length - 4) { throw "u32 offset $Offset is out of range." }
    return [BitConverter]::ToUInt32($Bytes, $Offset)
}

function Assert-Range([long]$Offset, [long]$Length, [long]$Limit, [string]$Description) {
    if ($Offset -lt 0 -or $Length -lt 0 -or $Offset -gt $Limit - $Length) {
        throw "$Description is outside its declared boundary."
    }
}

function Format-SegmentAddress([long]$Segment, [long]$Offset) {
    return '{0:X4}:{1:X4}' -f $Segment, $Offset
}

$bytes = [IO.File]::ReadAllBytes($sourceFullPath)
if ($bytes.Length -lt 32 -or $bytes[0] -ne [byte][char]'M' -or $bytes[1] -ne [byte][char]'Z') {
    throw 'SourcePath is not a complete MZ executable.'
}

$lastPageBytes = Read-UInt16 $bytes 2
$pageCount = Read-UInt16 $bytes 4
$headerBytes = [long](Read-UInt16 $bytes 8) * 16
if ($pageCount -eq 0 -or $lastPageBytes -gt 512) { throw 'MZ page counts are invalid.' }
$mzBytes = if ($lastPageBytes -eq 0) { [long]$pageCount * 512 } else { ([long]$pageCount - 1) * 512 + $lastPageBytes }
$fbovOffset = [long][Math]::Ceiling($mzBytes / 16.0) * 16
Assert-Range $headerBytes 0 $mzBytes 'MZ header'
Assert-Range $fbovOffset 16 $bytes.Length 'FBOV envelope'
if ([Text.Encoding]::ASCII.GetString($bytes, $fbovOffset, 4) -ne 'FBOV') {
    throw 'The aligned data after the MZ file is not FBOV.'
}

$payloadBytes = Read-UInt32 $bytes ($fbovOffset + 4)
$segmentTableOffset = Read-UInt32 $bytes ($fbovOffset + 8)
$segmentCount = [BitConverter]::ToInt32($bytes, $fbovOffset + 12)
if ($segmentCount -le 0 -or $segmentCount -gt 65536) { throw 'FBOV segment count is invalid.' }
Assert-Range $fbovOffset ([long]16 + $payloadBytes) $bytes.Length 'FBOV payload'
Assert-Range $segmentTableOffset ([long]$segmentCount * 8) $mzBytes 'FBOV segment table'

$overlays = [Collections.Generic.List[object]]::new()
for ($index = 0; $index -lt $segmentCount; $index++) {
    $descriptorOffset = $segmentTableOffset + $index * 8
    $segment = Read-UInt16 $bytes $descriptorOffset
    $flags = Read-UInt16 $bytes ($descriptorOffset + 4)
    if (($flags -band 2) -eq 0) { continue }

    $overlayHeaderOffset = $headerBytes + [long]$segment * 16
    Assert-Range $overlayHeaderOffset 32 $mzBytes "FBOV overlay header $index"
    if ($bytes[$overlayHeaderOffset] -ne 0xcd -or $bytes[$overlayHeaderOffset + 1] -ne 0x3f) {
        throw "FBOV overlay header $index lacks the expected trap prefix."
    }
    $payloadOffset = Read-UInt32 $bytes ($overlayHeaderOffset + 4)
    $codeBytes = Read-UInt16 $bytes ($overlayHeaderOffset + 8)
    $fixupBytes = Read-UInt16 $bytes ($overlayHeaderOffset + 10)
    $jumpCount = Read-UInt16 $bytes ($overlayHeaderOffset + 12)
    $codeOffset = $fbovOffset + 16 + [long]$payloadOffset
    Assert-Range $codeOffset ([long]$codeBytes + $fixupBytes) $bytes.Length "FBOV overlay payload $index"

    $relativeCodeOffset = $codeOffset - $headerBytes
    $mappedSegment = $loadSegment + ($relativeCodeOffset -shr 4)
    $mappedDisplacement = $relativeCodeOffset -band 0x0f
    if ($mappedSegment -gt 0xffff) { throw "FBOV overlay $index maps beyond segment FFFF." }
    $overlays.Add([pscustomobject]@{
        Descriptor = $index
        HeaderSegment = '{0:X4}' -f ($loadSegment + $segment)
        HeaderFileOffset = '0x{0:X8}' -f $overlayHeaderOffset
        TrampolineCount = $jumpCount
        CodeFileOffset = '0x{0:X8}' -f $codeOffset
        CodeBytes = $codeBytes
        FixupBytes = $fixupBytes
        MappedCodeStart = Format-SegmentAddress $mappedSegment $mappedDisplacement
        MappedCodeEnd = Format-SegmentAddress ($loadSegment + (($relativeCodeOffset + $codeBytes) -shr 4)) (($relativeCodeOffset + $codeBytes) -band 0x0f)
        CodeStartLinear = $codeOffset
        CodeEndLinear = $codeOffset + $codeBytes
    })
}

$conversions = [Collections.Generic.List[object]]::new()
foreach ($address in $MappedAddress) {
    if ($address -notmatch '^\s*([0-9a-fA-F]{1,4}):([0-9a-fA-F]{1,4})\s*$') {
        throw "MappedAddress '$address' is not segment:offset in hexadecimal."
    }
    $segmentValue = [Convert]::ToInt64($Matches[1], 16)
    $offsetValue = [Convert]::ToInt64($Matches[2], 16)
    if ($segmentValue -lt $loadSegment) { throw "MappedAddress '$address' lies below the load segment 1000." }
    $fileOffset = $headerBytes + ($segmentValue - $loadSegment) * 16 + $offsetValue
    if ($fileOffset -ge $bytes.Length) { throw "MappedAddress '$address' lies beyond the end of SourcePath." }
    $owner = $overlays | Where-Object { $fileOffset -ge $_.CodeStartLinear -and $fileOffset -lt $_.CodeEndLinear } | Select-Object -First 1
    $region = if ($owner) { 'overlay' } elseif ($fileOffset -lt $mzBytes) { 'resident' } else { 'fbov-data' }
    $conversions.Add([pscustomobject]@{
        MappedAddress = Format-SegmentAddress $segmentValue $offsetValue
        FileOffset = '0x{0:X8}' -f $fileOffset
        Region = $region
        Overlay = if ($owner) { $owner.Descriptor } else { $null }
        OffsetInOverlayCode = if ($owner) { '0x{0:X4}' -f ($fileOffset - $owner.CodeStartLinear) } else { $null }
        ResidentAddress = if ($region -eq 'resident') { Format-SegmentAddress $segmentValue $offsetValue } else { $null }
    })
}

[pscustomobject]@{
    SourcePath = $sourceFullPath
    HeaderBytes = $headerBytes
    ResidentImageBytes = $mzBytes - $headerBytes
    FbovFileOffset = '0x{0:X8}' -f $fbovOffset
    FbovEndFileOffset = '0x{0:X8}' -f ($fbovOffset + 16 + $payloadBytes)
    OverlayCount = $overlays.Count
    Overlays = @($overlays | Select-Object Descriptor, HeaderSegment, HeaderFileOffset, TrampolineCount, CodeFileOffset, CodeBytes, FixupBytes, MappedCodeStart, MappedCodeEnd)
    Conversions = @($conversions)
} | ConvertTo-Json -Depth 4
