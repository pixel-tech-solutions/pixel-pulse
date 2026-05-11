#Requires -Version 5.1
<#
.SYNOPSIS
    Resets the Pixel Pulse quotes database and optionally rebuilds it.

.DESCRIPTION
    Optionally backs up the existing database, then deletes %AppData%\PixelPulse\quotes.db.
    If -Rebuild is set, runs Build-Database.ps1 to repopulate.

.PARAMETER Backup
    Copy existing quotes.db to quotes.db.bak before deleting (if it exists).

.PARAMETER Rebuild
    After removing the database, run Build-Database.ps1 to repopulate.

.PARAMETER Force
    Skip confirmation prompt.

.PARAMETER WhatIf
    Show what would be done without executing.

.EXAMPLE
    .\Reset-Database.ps1 -Rebuild
.EXAMPLE
    .\Reset-Database.ps1 -Backup -Force
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [switch]$Backup,
    [switch]$Rebuild,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$script:RootDir = $PSScriptRoot -replace '\\scripts$', ''
if (-not $script:RootDir) { $script:RootDir = (Get-Location).Path }

function Write-Status {
    param([string]$Message, [string]$Status = 'Info')
    $color = switch ($Status) { 'Success' { 'Green' } 'Error' { 'Red' } 'Warning' { 'Yellow' } default { 'Cyan' } }
    Write-Host $Message -ForegroundColor $color
}

$dbDir = Join-Path $env:APPDATA 'PixelPulse'
$dbPath = Join-Path $dbDir 'quotes.db'
$bakPath = Join-Path $dbDir 'quotes.db.bak'

try {
    if (-not (Test-Path $dbPath)) {
        Write-Status 'No database file found. Nothing to reset.' 'Info'
        if ($Rebuild) {
            $buildScript = Join-Path $script:RootDir 'scripts\Build-Database.ps1'
            if (Test-Path $buildScript) {
                & $buildScript
                exit $LASTEXITCODE
            }
            Write-Status 'Build-Database.ps1 not found.' 'Error'
            exit 1
        }
        exit 0
    }

    if (-not $Force -and -not $PSCmdlet.ShouldProcess($dbPath, 'Remove database')) {
        Write-Status "Would remove: $dbPath" 'Info'
        if ($Backup) { Write-Status "Would backup to: $bakPath" 'Info' }
        if ($Rebuild) { Write-Status 'Would run Build-Database.ps1 after reset.' 'Info' }
        exit 0
    }

    if (-not $Force) {
        $confirm = Read-Host "Remove database and $(if ($Rebuild) { 'rebuild' } else { 'stop' })? (y/N)"
        if ($confirm -notmatch '^y') {
            Write-Status 'Cancelled.' 'Info'
            exit 0
        }
    }

    if ($Backup) {
        if ($PSCmdlet.ShouldProcess($bakPath, 'Backup database')) {
            Copy-Item -Path $dbPath -Destination $bakPath -Force -ErrorAction Stop
            Write-Status "Backed up to $bakPath" 'Success'
        }
    }

    if ($PSCmdlet.ShouldProcess($dbPath, 'Remove')) {
        Remove-Item -Path $dbPath -Force -ErrorAction Stop
        Write-Status "Removed: $dbPath" 'Success'
    }

    if ($Rebuild) {
        $buildScript = Join-Path $script:RootDir 'scripts\Build-Database.ps1'
        if (-not (Test-Path $buildScript)) {
            Write-Status 'Build-Database.ps1 not found.' 'Error'
            exit 1
        }
        Write-Status 'Rebuilding database...' 'Info'
        & $buildScript
        exit $LASTEXITCODE
    }
    exit 0
} catch {
    Write-Status "Error: $_" 'Error'
    exit 1
}
