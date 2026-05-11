#Requires -Version 5.1
<#
.SYNOPSIS
    Quick build of the PixelPulse application only (Debug), for fast development feedback.

.DESCRIPTION
    Restores and builds only the PixelPulse project in Debug configuration.
    Does not build DatabaseBuilder or the installer.

.PARAMETER Configuration
    Build configuration. Default is Debug.

.EXAMPLE
    .\Build-Quick.ps1
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$script:RootDir = $PSScriptRoot -replace '\\scripts$', ''
if (-not $script:RootDir) { $script:RootDir = (Get-Location).Path }

function Write-Status {
    param([string]$Message, [string]$Status = 'Info')
    $color = switch ($Status) { 'Success' { 'Green' } 'Error' { 'Red' } default { 'Cyan' } }
    Write-Host $Message -ForegroundColor $color
}

try {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Status 'dotnet CLI not found. Install .NET 8.0 SDK.' 'Error'
        exit 1
    }
    Set-Location $script:RootDir
    $proj = Join-Path $script:RootDir 'PixelPulse\PixelPulse.csproj'
    if (-not (Test-Path $proj)) { throw "Project not found: $proj" }
    Write-Status "Quick build: PixelPulse ($Configuration)..." 'Info'
    dotnet restore $proj 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Restore failed.' }
    dotnet build $proj --configuration $Configuration --no-restore 2>&1 | Out-Host
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    Write-Status 'Quick build succeeded.' 'Success'
    exit 0
} catch {
    Write-Status "Error: $_" 'Error'
    exit 1
} finally {
    Set-Location $script:RootDir
}
