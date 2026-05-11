#Requires -Version 5.1
<#
.SYNOPSIS
    Validates Pixel Pulse graphics assets (existence, dimensions, format).

.DESCRIPTION
    Checks application icon and installer assets (banner.bmp, dialog.bmp, icon.ico).
    Validates BMP dimensions using System.Drawing. Optionally outputs a JSON or XML report.

.PARAMETER ReportPath
    If set, write validation report to this file (JSON or XML by extension).

.PARAMETER ReportFormat
    Report format: Json or Xml. Used when ReportPath is set.

.EXAMPLE
    .\Validate-Assets.ps1
.EXAMPLE
    .\Validate-Assets.ps1 -ReportPath report.json -ReportFormat Json
#>
[CmdletBinding()]
param(
    [string]$ReportPath,
    [ValidateSet('Json', 'Xml')]
    [string]$ReportFormat = 'Json'
)

$ErrorActionPreference = 'Stop'
$script:RootDir = $PSScriptRoot -replace '\\scripts$', ''
if (-not $script:RootDir) { $script:RootDir = (Get-Location).Path }

function Write-Status {
    param([string]$Message, [string]$Status = 'Info')
    $color = switch ($Status) { 'Success' { 'Green' } 'Error' { 'Red' } 'Warning' { 'Yellow' } default { 'Cyan' } }
    Write-Host $Message -ForegroundColor $color
}

$errors = [System.Collections.ArrayList]::new()
$report = @{
    Passed = $true
    Timestamp = (Get-Date).ToString('o')
    Assets = @{}
    Errors = [System.Collections.ArrayList]::new()
}

function Test-AssetExists {
    param([string]$RelativePath, [string]$Label)
    $fullPath = Join-Path $script:RootDir $RelativePath
    $exists = Test-Path $fullPath
    $report.Assets[$Label] = @{ Path = $RelativePath; Exists = $exists }
    if (-not $exists) {
        $err = "Asset not found: $RelativePath"
        $errors.Add($err) | Out-Null
        $report.Errors.Add($err) | Out-Null
        $report.Passed = $false
        Write-Status "Missing: $RelativePath" 'Error'
        return $false
    }
    Write-Status "Found: $RelativePath" 'Success'
    return $true
}

function Test-BmpDimensions {
    param([string]$RelativePath, [int]$ExpectedWidth, [int]$ExpectedHeight, [string]$Label)
    $fullPath = Join-Path $script:RootDir $RelativePath
    if (-not (Test-Path $fullPath)) { return }
    try {
        Add-Type -AssemblyName System.Drawing -ErrorAction Stop
        $img = [System.Drawing.Image]::FromFile($fullPath)
        try {
            $w = $img.Width
            $h = $img.Height
            $report.Assets[$Label] = @{ Path = $RelativePath; Width = $w; Height = $h; ExpectedWidth = $ExpectedWidth; ExpectedHeight = $ExpectedHeight; Valid = ($w -eq $ExpectedWidth -and $h -eq $ExpectedHeight) }
            if ($w -ne $ExpectedWidth -or $h -ne $ExpectedHeight) {
                $err = "$Label dimensions incorrect: ${w}x${h} (expected ${ExpectedWidth}x${ExpectedHeight})"
                $errors.Add($err) | Out-Null
                $report.Errors.Add($err) | Out-Null
                $report.Passed = $false
                Write-Status $err 'Error'
            } else {
                Write-Status "$Label dimensions OK (${w}x${h})" 'Success'
            }
        } finally {
            $img.Dispose()
        }
    } catch {
        $err = "$Label could not be read: $_"
        $errors.Add($err) | Out-Null
        $report.Errors.Add($err) | Out-Null
        $report.Passed = $false
        Write-Status $err 'Error'
    }
}

try {
    Write-Status 'Validating Pixel Pulse assets...' 'Info'
    Write-Status "Root: $script:RootDir" 'Info'

    Test-AssetExists -RelativePath 'PixelPulse\Resources\icon.ico' -Label 'AppIcon' | Out-Null
    Test-AssetExists -RelativePath 'PixelPulse.Installer\Assets\banner.bmp' -Label 'BannerBmp' | Out-Null
    Test-AssetExists -RelativePath 'PixelPulse.Installer\Assets\dialog.bmp' -Label 'DialogBmp' | Out-Null
    Test-AssetExists -RelativePath 'PixelPulse.Installer\Assets\icon.ico' -Label 'InstallerIcon' | Out-Null

    Test-BmpDimensions -RelativePath 'PixelPulse.Installer\Assets\banner.bmp' -ExpectedWidth 493 -ExpectedHeight 58 -Label 'BannerDimensions'
    Test-BmpDimensions -RelativePath 'PixelPulse.Installer\Assets\dialog.bmp' -ExpectedWidth 493 -ExpectedHeight 312 -Label 'DialogDimensions'

    if ($ReportPath) {
        $report.Errors = @($report.Errors)
        $dir = Split-Path -Parent $ReportPath
        if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
        if ([System.IO.Path]::GetExtension($ReportPath) -eq '.xml') {
            $report | Export-Clixml -Path $ReportPath -Force
        } else {
            $report | ConvertTo-Json -Depth 5 | Set-Content -Path $ReportPath -Encoding UTF8
        }
        Write-Status "Report written to $ReportPath" 'Info'
    }

    Write-Host ''
    if ($errors.Count -eq 0) {
        Write-Status 'All assets validated successfully.' 'Success'
        exit 0
    }
    Write-Status "Validation failed with $($errors.Count) error(s)." 'Error'
    foreach ($e in $errors) { Write-Status "  $e" 'Error' }
    exit 1
} catch {
    Write-Status "Fatal error: $_" 'Error'
    exit 1
}
