<#
.SYNOPSIS
  Builds GrindBot.exe into windows\dist.

  Your settings are NOT in dist\ — they live in %APPDATA%\GrindBot, so wiping
  the build folder or rebuilding never touches them. GrindBot seeds that folder
  itself on first run.

.PARAMETER SelfContained
  Bundle the .NET runtime so the folder runs on a machine with no .NET
  installed. Bigger (~150 MB) but portable — copy dist\ anywhere and go.

.PARAMETER Portable
  Keep config.json and messages.txt inside dist\ instead of %APPDATA%, so the
  whole folder travels together on a stick. A file next to the exe always wins.

.PARAMETER Run
  Start GrindBot as soon as it is built.
#>
[CmdletBinding()]
param(
  [switch]$SelfContained,
  [switch]$Portable,
  [switch]$Run
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$repo = Split-Path -Parent $here
$dist = Join-Path $here 'dist'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
  throw "dotnet not found. Install the .NET 8 SDK: https://dotnet.microsoft.com/download"
}

# A running copy holds a lock on the exe.
Get-Process GrindBot -ErrorAction SilentlyContinue | ForEach-Object {
  Write-Host "Stopping the running GrindBot (pid $($_.Id))"
  try { $_.Kill(); $_.WaitForExit(5000) | Out-Null } catch { }
}

$publishArgs = @(
  'publish', (Join-Path $here 'GrindBot.csproj'),
  '-c', 'Release',
  '-r', 'win-x64',
  '-o', $dist,
  '--nologo'
)
if ($SelfContained) {
  $publishArgs += @('--self-contained', 'true', '-p:PublishSingleFile=false')
} else {
  $publishArgs += @('--self-contained', 'false')
}

& dotnet @publishArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }

$data = Join-Path $env:APPDATA 'GrindBot'

if ($Portable) {
  # Everything in one folder. GrindBot prefers a file next to the exe, so these
  # win over anything already in %APPDATA%.
  $seed = @{
    'config.json'  = Join-Path $repo 'config.default.json'
    'messages.txt' = Join-Path $repo 'messages.txt'
  }
  foreach ($name in $seed.Keys) {
    $target = Join-Path $dist $name
    if (-not (Test-Path $target)) {
      Copy-Item $seed[$name] $target
      Write-Host "Seeded $name into dist\"
    }
  }
  $settingsHome = $dist
} else {
  # Settings used to live in dist\, which a clean build or a stray delete would
  # take with it. Move any leftovers somewhere durable, once.
  New-Item -ItemType Directory -Force $data | Out-Null
  foreach ($name in @('config.json', 'messages.txt')) {
    $old = Join-Path $dist $name
    $new = Join-Path $data $name
    if ((Test-Path $old) -and -not (Test-Path $new)) {
      Move-Item $old $new
      Write-Host "Moved your $name to $data" -ForegroundColor Yellow
    } elseif (Test-Path $old) {
      Write-Host "Note: dist\$name still exists and overrides $new" -ForegroundColor Yellow
    }
  }
  $settingsHome = $data
}

# Packs are shipped content, so they always refresh. Your own packs go in
# %APPDATA%\GrindBot\packs — the picker merges both.
$packs = Join-Path $dist 'packs'
if (-not (Test-Path $packs)) { New-Item -ItemType Directory $packs | Out-Null }
Copy-Item (Join-Path $repo 'packs\*.txt') $packs -Force

$exe = Join-Path $dist 'GrindBot.exe'
Write-Host ""
Write-Host "Built $exe" -ForegroundColor Green
Write-Host "Settings live in $settingsHome"

if ($Run) {
  Start-Process $exe
  Write-Host "Started. The robot is in your system tray." -ForegroundColor Green
}
