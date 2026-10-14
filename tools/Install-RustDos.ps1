[CmdletBinding()]
param([string] $Destination)
# Installs the rust-dos release that dinorefurb-dosbox-session pins (RUST_DOS_PINNED_TAG) for agent
# runs of the original. The archive and the executable must match the hashes the package's README
# gives for the Windows x86_64 ZIP; anything else is refused and nothing is left in place.
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
if (-not $Destination) { $Destination = Join-Path $root 'artifacts/rust-dos' }
$tag = 'v1.7.2'
$archiveName = "rust-dos-$tag-windows-x86_64.zip"
$archiveSha256 = '2ca58dabaf1c392485418c180ff2728c5e8a76a970bf150890bab3b978f88da8'
$executableSha256 = '310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4'
$executable = Join-Path $Destination 'rust-dos.exe'

if ((Test-Path -LiteralPath $executable -PathType Leaf) -and
    (Get-FileHash -LiteralPath $executable -Algorithm SHA256).Hash.ToLowerInvariant() -eq $executableSha256 -and
    (Test-Path -LiteralPath (Join-Path $Destination 'SDL2.dll') -PathType Leaf)) {
    Write-Output "rust-dos $tag is already installed at $executable."
    return
}

$staging = Join-Path ([IO.Path]::GetTempPath()) ("rust-dos-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $staging | Out-Null
try {
    $archive = Join-Path $staging $archiveName
    Invoke-WebRequest -Uri "https://github.com/dividebysandwich/rust-dos/releases/download/$tag/$archiveName" -OutFile $archive -UseBasicParsing
    $actual = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actual -ne $archiveSha256) { throw "$archiveName has SHA-256 $actual, expected $archiveSha256." }
    $expanded = Join-Path $staging 'expanded'
    Expand-Archive -LiteralPath $archive -DestinationPath $expanded
    $found = @(Get-ChildItem -LiteralPath $expanded -Recurse -Filter 'rust-dos.exe' -File)
    if ($found.Count -ne 1) { throw "$archiveName holds $($found.Count) rust-dos.exe files, expected 1." }
    $actual = (Get-FileHash -LiteralPath $found[0].FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actual -ne $executableSha256) { throw "rust-dos.exe has SHA-256 $actual, expected $executableSha256." }
    if (-not (Test-Path -LiteralPath (Join-Path $found[0].DirectoryName 'SDL2.dll') -PathType Leaf)) {
        throw "$archiveName has no SDL2.dll beside rust-dos.exe."
    }
    if (Test-Path -LiteralPath $Destination) { Remove-Item -LiteralPath $Destination -Recurse -Force }
    New-Item -ItemType Directory -Path (Split-Path -Parent $Destination) -Force | Out-Null
    Move-Item -LiteralPath $found[0].DirectoryName -Destination $Destination
    Write-Output "Installed rust-dos $tag at $executable (SHA-256 $executableSha256)."
} finally {
    if (Test-Path -LiteralPath $staging) { Remove-Item -LiteralPath $staging -Recurse -Force }
}
