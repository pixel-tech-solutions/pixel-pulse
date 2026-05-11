#Requires -Version 5.1
<#
.SYNOPSIS
    Builds all Pixel Pulse projects (application, database builder, and optionally the installer).

.DESCRIPTION
    Restores NuGet packages, builds PixelPulse and PixelPulse.DatabaseBuilder in Release (or specified) configuration,
    and builds the WiX installer if WiX Toolset is available.

.PARAMETER Configuration
    Build configuration: Debug or Release. Default is Release.

.PARAMETER SkipInstaller
    Skip building the WiX installer even if WiX is available.

.PARAMETER WhatIf
    Show what would be done without executing.

.EXAMPLE
    .\Build-All.ps1
.EXAMPLE
    .\Build-All.ps1 -Configuration Debug -SkipInstaller
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$SkipInstaller
)

$ErrorActionPreference = 'Stop'
$script:RootDir = $PSScriptRoot -replace '\\scripts$', ''
if (-not $script:RootDir) { $script:RootDir = (Get-Location).Path }

function Write-Status { param([string]$Message, [string]$Status = 'Info')
    $color = switch ($Status) { 'Success' { 'Green' } 'Error' { 'Red' } 'Warning' { 'Yellow' } default { 'Cyan' } }
    Write-Host $Message -ForegroundColor $color
}

function Test-DotNet {
    try {
        $null = Get-Command dotnet -ErrorAction Stop
        return $true
    } catch { return $false }
}

function Test-WixAvailable {
    try {
        $msbuild = Get-Command msbuild -ErrorAction SilentlyContinue
        if (-not $msbuild) { return $false }
        $wixPath = Join-Path $env:ProgramFiles 'WiX Toolset v3.x\bin'
        if (Test-Path $wixPath) { return $true }
        $wixPath = Join-Path ${env:ProgramFiles(x86)} 'WiX Toolset v3.x\bin'
        return (Test-Path $wixPath)
    } catch { return $false }
}

$results = @{
    Restore = $false
    PixelPulse = $false
    DatabaseBuilder = $false
    Installer = $false
    Errors = [System.Collections.ArrayList]::new()
}

try {
    Write-Status "Pixel Pulse - Full Build (Configuration: $Configuration)" 'Info'
    Write-Status "Root: $script:RootDir" 'Info'

    if (-not (Test-DotNet)) {
        Write-Status 'dotnet CLI not found. Install .NET 8.0 SDK.' 'Error'
        $results.Errors.Add('dotnet not in PATH') | Out-Null
        exit 1
    }

    Set-Location $script:RootDir

    # Restore
    if ($PSCmdlet.ShouldProcess('NuGet packages', 'Restore')) {
        Write-Status 'Restoring NuGet packages...' 'Info'
        $restore = dotnet restore 2>&1
        if ($LASTEXITCODE -ne 0) {
            $results.Errors.Add("Restore failed: $restore") | Out-Null
            throw "Restore failed."
        }
        Write-Status 'Restore succeeded.' 'Success'
        $results.Restore = $true
    }

    # Build PixelPulse (self-contained)
    $pixelPulseProj = Join-Path $script:RootDir 'PixelPulse\PixelPulse.csproj'
    if (-not (Test-Path $pixelPulseProj)) { throw "Project not found: $pixelPulseProj" }
    if ($PSCmdlet.ShouldProcess('PixelPulse', 'Build')) {
        Write-Status "Building PixelPulse (self-contained)..." 'Info'
        $build = dotnet publish $pixelPulseProj --configuration $Configuration --runtime win-x64 --self-contained true --output "PixelPulse\bin\$Configuration\net8.0-windows" --no-restore 2>&1
        if ($LASTEXITCODE -ne 0) {
            $results.Errors.Add("PixelPulse build failed: $build") | Out-Null
            throw "PixelPulse build failed."
        }
        Write-Status 'PixelPulse self-contained build succeeded.' 'Success'
        $results.PixelPulse = $true
    }

    # Build DatabaseBuilder
    $dbProj = Join-Path $script:RootDir 'PixelPulse.DatabaseBuilder\PixelPulse.DatabaseBuilder.csproj'
    if (-not (Test-Path $dbProj)) { throw "Project not found: $dbProj" }
    if ($PSCmdlet.ShouldProcess('PixelPulse.DatabaseBuilder', 'Build')) {
        Write-Status "Building PixelPulse.DatabaseBuilder ($Configuration)..." 'Info'
        $build = dotnet build $dbProj --configuration $Configuration --no-restore 2>&1
        if ($LASTEXITCODE -ne 0) {
            $results.Errors.Add("DatabaseBuilder build failed: $build") | Out-Null
            throw "DatabaseBuilder build failed."
        }
        Write-Status 'PixelPulse.DatabaseBuilder build succeeded.' 'Success'
        $results.DatabaseBuilder = $true
    }

    # Download Quotes and Build Database
    if ($PSCmdlet.ShouldProcess('Quote Database', 'Download and Build')) {
        Write-Status "Downloading quotes and building database..." 'Info'
        $downloadScript = Join-Path $script:RootDir 'scripts\Download-Quotes.ps1'
        if (-not (Test-Path $downloadScript)) {
            Write-Status "Download-Quotes.ps1 not found, skipping quote database build" 'Warning'
        } else {
            try {
                & $downloadScript -Force
                if ($LASTEXITCODE -ne 0) {
                    Write-Status "Quote database build failed (non-fatal)" 'Warning'
                    $results.Errors.Add('Quote database build failed') | Out-Null
                } else {
                    Write-Status 'Quote database build succeeded.' 'Success'
                }
            } catch {
                Write-Status "Quote database build failed: $_" 'Warning'
                $results.Errors.Add("Quote database build failed: $_") | Out-Null
            }
        }
    }

    # Installer (optional)
    if (-not $SkipInstaller -and (Test-WixAvailable)) {
        $wixProj = Join-Path $script:RootDir 'PixelPulse.Installer\PixelPulse.Installer.wixproj'
        if (Test-Path $wixProj) {
            if ($PSCmdlet.ShouldProcess('PixelPulse.Installer', 'Build')) {
                Write-Status "Building PixelPulse.Installer ($Configuration)..." 'Info'
                $msbuildArgs = @(
                    $wixProj,
                    "/p:Configuration=$Configuration",
                    '/verbosity:minimal'
                )
                & msbuild $msbuildArgs 2>&1 | Out-Host
                if ($LASTEXITCODE -ne 0) {
                    Write-Status 'Installer build failed (non-fatal).' 'Warning'
                    $results.Errors.Add('Installer build failed') | Out-Null
                } else {
                    Write-Status 'PixelPulse.Installer build succeeded.' 'Success'
                    $results.Installer = $true
                }
            }
        }
    } elseif (-not $SkipInstaller) {
        Write-Status 'WiX Toolset not found. Skipping installer build.' 'Warning'
    }

    # Summary
    Write-Host ''
    Write-Status '--- Build Summary ---' 'Info'
    Write-Status "Restore: $(if ($results.Restore) { 'OK' } else { 'Skipped/Failed' })" $(if ($results.Restore) { 'Success' } else { 'Warning' })
    Write-Status "PixelPulse: $(if ($results.PixelPulse) { 'OK' } else { 'Failed' })" $(if ($results.PixelPulse) { 'Success' } else { 'Error' })
    Write-Status "DatabaseBuilder: $(if ($results.DatabaseBuilder) { 'OK' } else { 'Failed' })" $(if ($results.DatabaseBuilder) { 'Success' } else { 'Error' })
    Write-Status "Installer: $(if ($results.Installer) { 'OK' } elseif ($SkipInstaller) { 'Skipped' } else { 'Skipped/Failed' })" $(if ($results.Installer) { 'Success' } else { 'Warning' })
    if ($results.Errors.Count -gt 0) {
        foreach ($e in $results.Errors) { Write-Status "Error: $e" 'Error' }
        exit 1
    }
    exit 0
} catch {
    Write-Status "Fatal error: $_" 'Error'
    exit 1
} finally {
    Set-Location $script:RootDir
}
