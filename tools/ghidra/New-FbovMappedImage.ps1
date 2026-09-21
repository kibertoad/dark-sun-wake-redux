[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$SourcePath,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$sourceFullPath = [IO.Path]::GetFullPath($SourcePath)
$outputFullPath = [IO.Path]::GetFullPath($OutputPath)

if ($sourceFullPath -eq $outputFullPath) {
    throw 'SourcePath and OutputPath must differ.'
}
if ($outputFullPath.StartsWith($repositoryRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'OutputPath must be outside the repository because it contains original executable bytes.'
}
if (Test-Path -LiteralPath $outputFullPath) {
    throw "OutputPath already exists: $outputFullPath"
}

function Read-UInt16([byte[]]$Bytes, [int]$Offset) {
    if ($Offset -lt 0 -or $Offset -gt $Bytes.Length - 2) { throw "u16 offset $Offset is out of range." }
    return [BitConverter]::ToUInt16($Bytes, $Offset)
}

function Read-UInt32([byte[]]$Bytes, [int]$Offset) {
    if ($Offset -lt 0 -or $Offset -gt $Bytes.Length - 4) { throw "u32 offset $Offset is out of range." }
    return [BitConverter]::ToUInt32($Bytes, $Offset)
}

function Write-UInt16([byte[]]$Bytes, [int]$Offset, [uint16]$Value) {
    if ($Offset -lt 0 -or $Offset -gt $Bytes.Length - 2) { throw "u16 offset $Offset is out of range." }
    [BitConverter]::GetBytes($Value).CopyTo($Bytes, $Offset)
}

function Write-UInt32([byte[]]$Bytes, [int]$Offset, [uint32]$Value) {
    if ($Offset -lt 0 -or $Offset -gt $Bytes.Length - 4) { throw "u32 offset $Offset is out of range." }
    [BitConverter]::GetBytes($Value).CopyTo($Bytes, $Offset)
}

function Assert-Range([long]$Offset, [long]$Length, [long]$Limit, [string]$Description) {
    if ($Offset -lt 0 -or $Length -lt 0 -or $Offset -gt $Limit - $Length) {
        throw "$Description is outside its declared boundary."
    }
}

$bytes = [IO.File]::ReadAllBytes($sourceFullPath)
if ($bytes.Length -lt 32 -or $bytes[0] -ne [byte][char]'M' -or $bytes[1] -ne [byte][char]'Z') {
    throw 'SourcePath is not a complete MZ executable.'
}

$lastPageBytes = Read-UInt16 $bytes 2
$pageCount = Read-UInt16 $bytes 4
$relocationCount = Read-UInt16 $bytes 6
$headerBytes = [long](Read-UInt16 $bytes 8) * 16
$relocationTableOffset = Read-UInt16 $bytes 24
if ($pageCount -eq 0 -or $lastPageBytes -gt 512) { throw 'MZ page counts are invalid.' }
$mzBytes = if ($lastPageBytes -eq 0) { [long]$pageCount * 512 } else { ([long]$pageCount - 1) * 512 + $lastPageBytes }
$fbovOffset = [long][Math]::Ceiling($mzBytes / 16.0) * 16
Assert-Range $headerBytes 0 $mzBytes 'MZ header'
Assert-Range $relocationTableOffset ([long]$relocationCount * 4) $headerBytes 'MZ relocation table'
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

$relocations = [Collections.Generic.List[object]]::new()
for ($index = 0; $index -lt $relocationCount; $index++) {
    $offset = $relocationTableOffset + $index * 4
    $relocations.Add(@((Read-UInt16 $bytes $offset), (Read-UInt16 $bytes ($offset + 2))))
}

$overlayCount = 0
$trampolineCount = 0
$functionReferenceFixupCount = 0
for ($index = 0; $index -lt $segmentCount; $index++) {
    $descriptorOffset = $segmentTableOffset + $index * 8
    $segment = Read-UInt16 $bytes $descriptorOffset
    $flags = Read-UInt16 $bytes ($descriptorOffset + 4)
    if (($flags -band 2) -eq 0) { continue }

    $overlayCount++
    $overlayHeaderOffset = $headerBytes + [long]$segment * 16
    Assert-Range $overlayHeaderOffset 32 $mzBytes "FBOV overlay header $index"
    if ($bytes[$overlayHeaderOffset] -ne 0xcd -or $bytes[$overlayHeaderOffset + 1] -ne 0x3f) {
        throw "FBOV overlay header $index lacks the expected trap prefix."
    }
    $payloadOffset = Read-UInt32 $bytes ($overlayHeaderOffset + 4)
    $codeBytes = Read-UInt16 $bytes ($overlayHeaderOffset + 8)
    $fixupBytes = Read-UInt16 $bytes ($overlayHeaderOffset + 10)
    $jumpCount = Read-UInt16 $bytes ($overlayHeaderOffset + 12)
    if (($fixupBytes % 2) -ne 0) { throw "FBOV overlay header $index has an odd fixup length." }
    $codeOffset = $fbovOffset + 16 + [long]$payloadOffset
    Assert-Range $codeOffset ([long]$codeBytes + $fixupBytes) $bytes.Length "FBOV overlay payload $index"
    Assert-Range ($overlayHeaderOffset + 32) ([long]$jumpCount * 5) $mzBytes "FBOV trampoline table $index"

    $relativeCodeOffset = $codeOffset - $headerBytes
    $codeSegment = [uint16]($relativeCodeOffset -shr 4)
    $codeDisplacement = [uint16]($relativeCodeOffset -band 0x0f)
    for ($jumpIndex = 0; $jumpIndex -lt $jumpCount; $jumpIndex++) {
        $trapOffset = $overlayHeaderOffset + 32 + $jumpIndex * 5
        $targetOffset = Read-UInt16 $bytes ($trapOffset + 2)
        $bytes[$trapOffset] = 0xea
        Write-UInt16 $bytes ($trapOffset + 1) ([uint16]($targetOffset + $codeDisplacement))
        Write-UInt16 $bytes ($trapOffset + 3) $codeSegment
        $relocations.Add(@($segment, [uint16](($trapOffset + 3) - $overlayHeaderOffset)))
        $trampolineCount++
    }

    for ($fixupIndex = 0; $fixupIndex -lt ($fixupBytes / 2); $fixupIndex++) {
        $fixupEntryOffset = $codeOffset + $codeBytes + $fixupIndex * 2
        $codeWordOffset = Read-UInt16 $bytes $fixupEntryOffset
        Assert-Range ($codeOffset + $codeWordOffset) 2 ($codeOffset + $codeBytes) "FBOV fixup $index/$fixupIndex"
        $encodedSegment = Read-UInt16 $bytes ($codeOffset + $codeWordOffset)
        $targetDescriptorIndex = $encodedSegment -shr 3
        if ($targetDescriptorIndex -ge $segmentCount) {
            throw "FBOV fixup $index/$fixupIndex references descriptor $targetDescriptorIndex outside the segment table."
        }
        $targetSegment = Read-UInt16 $bytes ($segmentTableOffset + $targetDescriptorIndex * 8)
        Write-UInt16 $bytes ($codeOffset + $codeWordOffset) $targetSegment
        $relocations.Add(@($codeSegment, [uint16]($codeWordOffset + $codeDisplacement)))
        if (($encodedSegment -band 1) -ne 0) { $functionReferenceFixupCount++ }
    }
}

$newRelocationCount = $relocations.Count
if ($newRelocationCount -gt [UInt16]::MaxValue) { throw 'The merged relocation table exceeds the MZ limit.' }
$newHeaderBytes = [long][Math]::Ceiling(($relocationTableOffset + $newRelocationCount * 4) / 16.0) * 16
$outputLength = $newHeaderBytes + $bytes.Length - $headerBytes
if ($outputLength -gt [UInt32]::MaxValue) { throw 'The merged MZ image exceeds the MZ size limit.' }
Write-UInt16 $bytes 6 ([uint16]$newRelocationCount)
Write-UInt16 $bytes 8 ([uint16]($newHeaderBytes / 16))
Write-UInt16 $bytes 4 ([uint16][Math]::Ceiling($outputLength / 512.0))
Write-UInt16 $bytes 2 ([uint16]($outputLength % 512))
for ($index = 0; $index -lt 8; $index++) { $bytes[$fbovOffset + $index] = 0 }

$outputDirectory = Split-Path -Parent $outputFullPath
if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
    [IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
}
$stream = [IO.File]::Create($outputFullPath)
try {
    $stream.Write($bytes, 0, $relocationTableOffset)
    foreach ($relocation in $relocations) {
        $relocationOffsetBytes = [BitConverter]::GetBytes([uint16]$relocation[0])
        $relocationSegmentBytes = [BitConverter]::GetBytes([uint16]$relocation[1])
        $stream.Write($relocationOffsetBytes, 0, $relocationOffsetBytes.Length)
        $stream.Write($relocationSegmentBytes, 0, $relocationSegmentBytes.Length)
    }
    $padding = New-Object byte[] ($newHeaderBytes - $relocationTableOffset - $newRelocationCount * 4)
    $stream.Write($padding, 0, $padding.Length)
    $stream.Write($bytes, $headerBytes, $bytes.Length - $headerBytes)
}
catch {
    $stream.Dispose()
    Remove-Item -LiteralPath $outputFullPath -Force -ErrorAction SilentlyContinue
    throw
}
finally {
    $stream.Dispose()
}

[pscustomobject]@{
    SourcePath = $sourceFullPath
    OutputPath = $outputFullPath
    OverlayHeaderCount = $overlayCount
    TrampolineCount = $trampolineCount
    FunctionReferenceFixupCount = $functionReferenceFixupCount
    OutputByteLength = (Get-Item -LiteralPath $outputFullPath).Length
} | ConvertTo-Json -Compress
