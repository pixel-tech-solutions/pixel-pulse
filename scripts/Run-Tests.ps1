#Requires -Version 5.1
<#
.SYNOPSIS
    Runs basic smoke tests for Pixel Pulse (startup, database path, build outputs).

.DESCRIPTION
    Verifies build outputs exist, database path is valid, and can optionally launch
    the application for a short smoke test.

.PARAMETER Configuration
    Build configuration to test (Debug or Release). Default is Release.

.PARAMETER LaunchApp
    Launch PixelPulse.exe and wait a few seconds, then close (basic startup test).

.PARAMETER LaunchSeconds
    Seconds to keep the app running when -LaunchApp is used. Default 5.

.EXAMPLE
    .\Run-Tests.ps1
.EXAMPLE
    .\Run-Tests.ps1 -LaunchApp -LaunchSeconds 3
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$LaunchApp,
    [int]$LaunchSeconds = 5
)

$ErrorActionPreference = 'Stop'
$script:RootDir = $PSScriptRoot -replace '\\scripts$', ''
if (-not $script:RootDir) { $script:RootDir = (Get-Location).Path }

function Write-Status {
    param([string]$Message, [string]$Status = 'Info')
    $color = switch ($Status) { 'Success' { 'Green' } 'Error' { 'Red' } 'Warning' { 'Yellow' } default { 'Cyan' } }
    Write-Host $Message -ForegroundColor $color
}

$failed = 0
$total = 0

try {
    Write-Status 'Pixel Pulse - Smoke Tests' 'Info'
    Write-Status "Root: $script:RootDir | Config: $Configuration" 'Info'
    Write-Host ''

    $exePath = Join-Path $script:RootDir "PixelPulse\bin\$Configuration\net8.0-windows\PixelPulse.exe"
    $dbPath = Join-Path $env:APPDATA 'PixelPulse\quotes.db'
    $dbDir = Join-Path $env:APPDATA 'PixelPulse'

    $total++
    if (Test-Path $exePath) {
        Write-Status '[PASS] PixelPulse.exe exists' 'Success'
    } else {
        Write-Status '[FAIL] PixelPulse.exe exists' 'Error'
        $failed++
    }

    $total++
    if (Test-Path $dbDir) {
        Write-Status '[PASS] AppData PixelPulse folder exists' 'Success'
    } else {
        Write-Status '[FAIL] AppData PixelPulse folder exists' 'Error'
        $failed++
    }

    $total++
    $dbOk = $false
    if (Test-Path $dbPath) { $dbOk = $true }
    else {
        try {
            if (-not (Test-Path $dbDir)) { New-Item -ItemType Directory -Path $dbDir -Force | Out-Null }
            $testFile = Join-Path $dbDir '._write_test'
            [System.IO.File]::WriteAllText($testFile, '')
            Remove-Item $testFile -Force -ErrorAction SilentlyContinue
            $dbOk = $true
        } catch { }
    }
    if ($dbOk) { Write-Status '[PASS] Database file or folder writable' 'Success' }
    else { Write-Status '[FAIL] Database file or folder writable' 'Error'; $failed++ }

    if ($LaunchApp -and (Test-Path $exePath)) {
        $total++
        Write-Status 'Launching app for startup test...' 'Info'
        try {
            $p = Start-Process -FilePath $exePath -PassThru -WindowStyle Normal
            Start-Sleep -Seconds $LaunchSeconds
            if (-not $p.HasExited) { $p.Kill(); Start-Sleep -Milliseconds 500 }
            Write-Status '[PASS] Application startup' 'Success'
        } catch {
            Write-Status "[FAIL] Application startup: $_" 'Error'
            $failed++
        }
    }

    Write-Host ''
    if ($failed -eq 0) {
        Write-Status "All $total check(s) passed." 'Success'
        exit 0
    }
    Write-Status "$failed of $total check(s) failed." 'Error'
    exit 1
} catch {
    Write-Status "Fatal error: $_" 'Error'
    exit 1
}
