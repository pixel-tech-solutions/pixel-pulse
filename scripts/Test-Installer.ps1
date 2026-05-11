#Requires -Version 5.1
<#
.SYNOPSIS
    Verifies the Pixel Pulse installer MSI exists and optionally runs a silent install/uninstall test.

.DESCRIPTION
    Checks that the built MSI exists, optionally runs msiexec silent install to a temp folder,
    verifies files, then uninstalls. Useful for CI or local verification.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default Release.

.PARAMETER SkipSilentInstall
    Only check that the MSI file exists; do not run silent install/uninstall.

.PARAMETER InstallPath
    Custom directory for test install. Default is a temp subfolder.

.EXAMPLE
    .\Test-Installer.ps1
.EXAMPLE
    .\Test-Installer.ps1 -SkipSilentInstall
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$SkipSilentInstall,
    [string]$InstallPath
)

$ErrorActionPreference = 'Stop'
$script:RootDir = $PSScriptRoot -replace '\\scripts$', ''
if (-not $script:RootDir) { $script:RootDir = (Get-Location).Path }

function Write-Status {
    param([string]$Message, [string]$Status = 'Info')
    $color = switch ($Status) { 'Success' { 'Green' } 'Error' { 'Red' } 'Warning' { 'Yellow' } default { 'Cyan' } }
    Write-Host $Message -ForegroundColor $color
}

$msiPath = Join-Path $script:RootDir "PixelPulse.Installer\bin\$Configuration\PixelPulseInstaller.msi"
$msiPathAlt = Join-Path $script:RootDir "PixelPulse.Installer\bin\$Configuration\*.msi"

try {
    Write-Status 'Pixel Pulse - Installer Tests' 'Info'

    if (-not (Test-Path $msiPath)) {
        $alt = Get-Item $msiPathAlt -ErrorAction SilentlyContinue
        if ($alt) { $msiPath = $alt.FullName }
    }
    if (-not (Test-Path $msiPath)) {
        Write-Status "MSI not found: $msiPath" 'Error'
        Write-Status 'Build the installer first (e.g. .\Build-All.ps1).' 'Warning'
        exit 1
    }
    $msi = Get-Item $msiPath
    Write-Status "MSI found: $($msi.FullName) ($($msi.Length) bytes)" 'Success'

    if ($SkipSilentInstall) {
        Write-Status 'Skipping silent install (SkipSilentInstall).' 'Info'
        exit 0
    }

    if (-not $InstallPath) {
        $InstallPath = Join-Path $env:TEMP "PixelPulseTestInstall_$(Get-Date -Format 'yyyyMMddHHmmss')"
    }
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
    $logInstall = Join-Path $env:TEMP 'PixelPulse_install.log'
    $logUninstall = Join-Path $env:TEMP 'PixelPulse_uninstall.log'

    Write-Status "Test install path: $InstallPath" 'Info'
    $productCode = $null
    try {
        $msiArgs = @(
            '/i', $msi.FullName,
            '/qn',
            "INSTALLDIR=`"$InstallPath`"",
            "/l*v", $logInstall
        )
        $p = Start-Process -FilePath 'msiexec.exe' -ArgumentList $msiArgs -Wait -PassThru -NoNewWindow
        if ($p.ExitCode -ne 0) {
            Write-Status "Install failed (exit code $($p.ExitCode)). Check $logInstall" 'Error'
            exit 1
        }
        Write-Status 'Silent install completed.' 'Success'

        $exeInDir = Get-ChildItem -Path $InstallPath -Filter 'PixelPulse.exe' -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($exeInDir) {
            Write-Status "PixelPulse.exe found at: $($exeInDir.FullName)" 'Success'
        } else {
            Write-Status 'PixelPulse.exe not found under install path.' 'Warning'
        }

        $prodLine = Get-Content $logInstall -ErrorAction SilentlyContinue | Where-Object { $_ -match 'ProductCode' } | Select-Object -First 1
        if ($prodLine -match '\{([A-F0-9-]+\})') { $productCode = $Matches[1] }

        if ($productCode) {
            $uargs = @('/x', $productCode, '/qn', "/l*v", $logUninstall)
            $pu = Start-Process -FilePath 'msiexec.exe' -ArgumentList $uargs -Wait -PassThru -NoNewWindow
            if ($pu.ExitCode -eq 0) {
                Write-Status 'Silent uninstall completed.' 'Success'
            } else {
                Write-Status "Uninstall returned exit code $($pu.ExitCode)." 'Warning'
            }
        } else {
            Write-Status 'Could not determine ProductCode for uninstall. Uninstall manually if needed.' 'Warning'
        }
    } finally {
        if (Test-Path $InstallPath) {
            Remove-Item -Path $InstallPath -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
    Write-Status 'Installer test completed.' 'Success'
    exit 0
} catch {
    Write-Status "Error: $_" 'Error'
    exit 1
}
