#Requires -Version 5.1
<#
.SYNOPSIS
    Verifies project setup: .NET SDK, optional WiX, restores packages, and project structure.

.DESCRIPTION
    Checks that .NET 8.0 SDK is available, optionally checks for WiX Toolset,
    restores NuGet packages for the solution, and ensures required directories exist.

.PARAMETER SkipRestore
    Do not run dotnet restore.

.PARAMETER CheckWix
    Also verify WiX Toolset is available (for installer builds).

.EXAMPLE
    .\Setup-Project.ps1
.EXAMPLE
    .\Setup-Project.ps1 -CheckWix -SkipRestore
#>
[CmdletBinding()]
param(
    [switch]$SkipRestore,
    [switch]$CheckWix
)

$ErrorActionPreference = 'Stop'
$script:RootDir = $PSScriptRoot -replace '\\scripts$', ''
if (-not $script:RootDir) { $script:RootDir = (Get-Location).Path }

function Write-Status {
    param([string]$Message, [string]$Status = 'Info')
    $color = switch ($Status) { 'Success' { 'Green' } 'Error' { 'Red' } 'Warning' { 'Yellow' } default { 'Cyan' } }
    Write-Host $Message -ForegroundColor $color
}

$ok = $true
try {
    Set-Location $script:RootDir
    Write-Status 'Pixel Pulse - Project Setup' 'Info'
    Write-Status "Root: $script:RootDir" 'Info'
    Write-Host ''

    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnet) {
        Write-Status '.NET SDK not found. Install .NET 8.0 SDK.' 'Error'
        $ok = $false
    } else {
        $ver = dotnet --version 2>&1
        Write-Status "dotnet found: $ver" 'Success'
        if ($ver -notmatch '^8\.') {
            Write-Status 'Recommend .NET 8.0 for this project.' 'Warning'
        }
    }

    $sln = Join-Path $script:RootDir 'PixelPulse.sln'
    if (-not (Test-Path $sln)) {
        Write-Status 'PixelPulse.sln not found.' 'Error'
        $ok = $false
    } else {
        Write-Status 'Solution file found.' 'Success'
    }

    $dirs = @(
        'PixelPulse',
        'PixelPulse\Resources',
        'PixelPulse.DatabaseBuilder',
        'PixelPulse.Installer',
        'PixelPulse.Installer\Assets',
        'PixelPulse.Installer\UI'
    )
    foreach ($d in $dirs) {
        $full = Join-Path $script:RootDir $d
        if (-not (Test-Path $full)) {
            New-Item -ItemType Directory -Path $full -Force | Out-Null
            Write-Status "Created: $d" 'Success'
        }
    }

    if ($CheckWix) {
        $msbuild = Get-Command msbuild -ErrorAction SilentlyContinue
        $wixPath = Join-Path $env:ProgramFiles 'WiX Toolset v3.x\bin'
        $wixPath86 = Join-Path ${env:ProgramFiles(x86)} 'WiX Toolset v3.x\bin'
        if ($msbuild -and ((Test-Path $wixPath) -or (Test-Path $wixPath86))) {
            Write-Status 'WiX Toolset found.' 'Success'
        } else {
            Write-Status 'WiX Toolset not found. Install for installer builds.' 'Warning'
        }
    }

    if (-not $SkipRestore -and $ok -and (Test-Path $sln)) {
        Write-Status 'Restoring NuGet packages...' 'Info'
        dotnet restore $sln 2>&1 | Out-Host
        if ($LASTEXITCODE -ne 0) {
            Write-Status 'Restore failed.' 'Error'
            $ok = $false
        } else {
            Write-Status 'Restore succeeded.' 'Success'
        }
    }

    Write-Host ''
    if ($ok) {
        Write-Status 'Setup completed successfully.' 'Success'
        exit 0
    }
    Write-Status 'Setup completed with errors.' 'Error'
    exit 1
} catch {
    Write-Status "Error: $_" 'Error'
    exit 1
} finally {
    Set-Location $script:RootDir
}
