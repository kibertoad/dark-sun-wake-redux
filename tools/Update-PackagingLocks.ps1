[CmdletBinding()]
param(
    [ValidateSet('win-x64', 'linux-x64', 'osx-x64', 'osx-arm64')]
    [string[]] $Runtime = @('win-x64', 'linux-x64', 'osx-x64', 'osx-arm64'),
    [switch] $Check
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
# Maintenance regenerates the explicit profiles; Check verifies them in locked mode.
$lockArguments = @(if ($Check) { '--locked-mode' } else { '--force-evaluate'; '-p:RestoreLockedMode=false' })
foreach ($target in $Runtime) {
    foreach ($project in @('src/DarkSunWakeRedux.Game/DarkSunWakeRedux.Game.csproj', 'src/DarkSunWakeRedux.Extractor/DarkSunWakeRedux.Extractor.csproj')) {
        $singleFile = $target -ne 'win-x64' -or $project.Contains('.Extractor/')
        $mode = if ($singleFile) { 'singlefile' } else { 'directory' }
        $profile = "$target-$mode"
        dotnet restore (Join-Path $root $project) --runtime $target `
            "-p:RuntimeIdentifier=$target" `
            '-p:SelfContained=true' "-p:PublishSingleFile=$($singleFile.ToString().ToLowerInvariant())" `
            "-p:PackageLockProfile=$profile" @lockArguments --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw "Packaging lock $profile failed for $project." }
    }
}
