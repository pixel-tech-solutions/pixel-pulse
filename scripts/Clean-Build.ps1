#Requires -Version 5.1
<#
.SYNOPSIS
    Removes build artifacts (bin, obj) from Pixel Pulse projects.

.DESCRIPTION
    Cleans bin/ and obj/ folders for PixelPulse, PixelPulse.DatabaseBuilder, and PixelPulse.Installer.
    Optional: clean NuGet package cache for solution; optional: remove local database file.

.PARAMETER IncludeNuGetCache
    Also run dotnet nuget locals all --clear for the solution (use with care).

.PARAMETER RemoveDatabase
    Remove the application database file from %AppData%\PixelPulse\quotes.db if it exists.

.PARAMETER Force
    Skip confirmation prompt.

.PARAMETER WhatIf
    Show what would be removed without deleting.

.EXAMPLE
    .\Clean-Build.ps1
.EXAMPLE
    .\Clean-Build.ps1 -Force -RemoveDatabase
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [switch]$IncludeNuGetCache,
    [switch]$RemoveDatabase,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$script:RootDir = $PSScriptRoot -replace '\\scripts$', ''
if (-not $script:RootDir) { $script:RootDir = (Get-Location).Path }

function Write-Status { param([string]$Message, [string]$Status = 'Info')
    $color = switch ($Status) { 'Success' { 'Green' } 'Error' { 'Red' } 'Warning' { 'Yellow' } default { 'Cyan' }
    Write-Host $Message -ForegroundColor $color
}

$dirsToClean = @(
    (Join-Path $script:RootDir 'PixelPulse\bin'),
    (Join-Path $script:RootDir 'PixelPulse\obj'),
    (Join-Path $script:RootDir 'PixelPulse.DatabaseBuilder\bin'),
    (Join-Path $script:RootDir 'PixelPulse.DatabaseBuilder\obj'),
    (Join-Path $script:RootDir 'PixelPulse.Installer\bin'),
    (Join-Path $script:RootDir 'PixelPulse.Installer\obj')
)

$toRemove = @()
foreach ($d in $dirsToClean) {
    if (Test-Path $d) { $toRemove += $d }
}
if ($RemoveDatabase) {
    $dbPath = Join-Path $env:APPDATA 'PixelPulse\quotes.db'
    if (Test-Path $dbPath) { $toRemove += $dbPath }
}

if ($toRemove.Count -eq 0) {
    Write-Status 'Nothing to clean.' 'Info'
    exit 0
}

if (-not $Force -and -not $PSCmdlet.ShouldProcess('Pixel Pulse build artifacts', 'Clean')) {
    Write-Status 'The following would be removed:' 'Info'
    $toRemove | ForEach-Object { Write-Host "  $_" }
    if ($IncludeNuGetCache) { Write-Status 'NuGet cache would be cleared (dotnet nuget locals all --clear).' 'Warning' }
    exit 0
}

if (-not $Force) {
    Write-Status 'The following will be removed:' 'Warning'
    $toRemove | ForEach-Object { Write-Host "  $_" }
    if ($IncludeNuGetCache) { Write-Status 'NuGet cache will be cleared.' 'Warning' }
    $confirm = Read-Host 'Continue? (y/N)'
    if ($confirm -notmatch '^y') {
        Write-Status 'Cancelled.' 'Info'
        exit 0
    }
}

try {
    foreach ($path in $toRemove) {
        if (Test-Path $path) {
            if ($PSCmdlet.ShouldProcess($path, 'Remove')) {
                Remove-Item -Path $path -Recurse -Force -ErrorAction Stop
                Write-Status "Removed: $path" 'Success'
            }
        }
    }
    if ($IncludeNuGetCache -and $PSCmdlet.ShouldProcess('NuGet cache', 'Clear')) {
        dotnet nuget locals all --clear 2>&1 | Out-Null
        Write-Status 'NuGet cache cleared.' 'Success'
    }
    Write-Status 'Clean completed.' 'Success'
    exit 0
} catch {
    Write-Status "Error: $_" 'Error'
    exit 1
}
