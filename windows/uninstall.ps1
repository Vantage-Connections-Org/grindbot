<#
.SYNOPSIS
  Removes GrindBot from this machine.

  Deleting dist\ on its own is not enough: "Start with Windows" writes an HKCU
  Run value that would keep pointing at an exe that no longer exists, and your
  settings live in %APPDATA%\GrindBot. This clears both.

.PARAMETER KeepSettings
  Leave %APPDATA%\GrindBot alone, so your config and custom packs survive.
#>
[CmdletBinding(SupportsShouldProcess)]
param([switch]$KeepSettings)

$ErrorActionPreference = 'Stop'

Get-Process GrindBot -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "Stopping GrindBot (pid $($_.Id))"
    try { $_.Kill(); $_.WaitForExit(5000) | Out-Null } catch { }
}

$runKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
if ((Get-ItemProperty $runKey -Name GrindBot -ErrorAction SilentlyContinue)) {
    Remove-ItemProperty $runKey -Name GrindBot
    Write-Host "Removed the start-with-Windows entry."
} else {
    Write-Host "No start-with-Windows entry to remove."
}

$data = Join-Path $env:APPDATA 'GrindBot'
if (Test-Path $data) {
    if ($KeepSettings) {
        Write-Host "Kept your settings in $data"
    } else {
        Remove-Item $data -Recurse -Force
        Write-Host "Removed $data"
    }
}

$dist = Join-Path $PSScriptRoot 'dist'
if (Test-Path $dist) {
    Remove-Item $dist -Recurse -Force
    Write-Host "Removed $dist"
}

Write-Host "GrindBot is gone. Delete this folder to finish."
