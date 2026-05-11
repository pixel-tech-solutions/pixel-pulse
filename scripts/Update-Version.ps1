#Requires -Version 5.1
<#
.SYNOPSIS
    Updates version numbers in Pixel Pulse project files and optionally CHANGELOG.

.DESCRIPTION
    Sets AssemblyVersion and FileVersion in PixelPulse.csproj, and Product Version
    in Product.wxs. Optionally appends a new section to CHANGELOG.md.

.PARAMETER Version
    Version string (e.g. 1.0.1 or 1.1.0). Must be three parts: major.minor.patch.

.PARAMETER UpdateChangelog
    If set, append a new version section to CHANGELOG.md with placeholder content.

.PARAMETER WhatIf
    Show what would be updated without making changes.

.EXAMPLE
    .\Update-Version.ps1 -Version 1.0.1
.EXAMPLE
    .\Update-Version.ps1 -Version 1.1.0 -UpdateChangelog
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,
    [switch]$UpdateChangelog
)

$ErrorActionPreference = 'Stop'
$script:RootDir = $PSScriptRoot -replace '\\scripts$', ''
if (-not $script:RootDir) { $script:RootDir = (Get-Location).Path }

if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    Write-Error "Version must be major.minor.patch (e.g. 1.0.1). Got: $Version"
    exit 1
}

$versionParts = $Version -split '\.'
$assemblyVersion = "$($versionParts[0]).$($versionParts[1]).0.0"

function Write-Status {
    param([string]$Message, [string]$Status = 'Info')
    $color = switch ($Status) { 'Success' { 'Green' } 'Error' { 'Red' } default { 'Cyan' } }
    Write-Host $Message -ForegroundColor $color
}

$csproj = Join-Path $script:RootDir 'PixelPulse\PixelPulse.csproj'
$wxs = Join-Path $script:RootDir 'PixelPulse.Installer\Product.wxs'
$changelog = Join-Path $script:RootDir 'CHANGELOG.md'

try {
    Write-Status "Updating version to $Version (AssemblyVersion: $assemblyVersion)" 'Info'

    if (-not (Test-Path $csproj)) { throw "Project not found: $csproj" }
    $content = Get-Content $csproj -Raw -Encoding UTF8
    $content = $content -replace '<AssemblyVersion>[^<]+</AssemblyVersion>', "<AssemblyVersion>$assemblyVersion</AssemblyVersion>"
    $content = $content -replace '<FileVersion>[^<]+</FileVersion>', "<FileVersion>$assemblyVersion</FileVersion>"
    if ($PSCmdlet.ShouldProcess($csproj, 'Update version')) {
        Set-Content -Path $csproj -Value $content -Encoding UTF8 -NoNewline
        Write-Status "Updated: PixelPulse.csproj" 'Success'
    }

    if (Test-Path $wxs) {
        $wxsContent = Get-Content $wxs -Raw -Encoding UTF8
        $wxsContent = $wxsContent -replace 'Version="[^"]+"', "Version=`"$Version`""
        if ($PSCmdlet.ShouldProcess($wxs, 'Update version')) {
            Set-Content -Path $wxs -Value $wxsContent -Encoding UTF8 -NoNewline
            Write-Status "Updated: Product.wxs" 'Success'
        }
    }

    if ($UpdateChangelog -and (Test-Path $changelog)) {
        $date = Get-Date -Format 'yyyy-MM-dd'
        $newSection = @"

## [$Version] - $date

### Added
- (Describe new features)

### Changed
- (Describe changes)

### Fixed
- (Describe fixes)
"@
        if ($PSCmdlet.ShouldProcess($changelog, 'Append version section')) {
            Add-Content -Path $changelog -Value $newSection -Encoding UTF8
            Write-Status "Appended version section to CHANGELOG.md" 'Success'
        }
    }

    Write-Status 'Version update completed.' 'Success'
    exit 0
} catch {
    Write-Status "Error: $_" 'Error'
    exit 1
}
