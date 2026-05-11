#Requires -Version 5.1
<#
.SYNOPSIS
    Generates placeholder graphics assets for testing builds (simple colored graphics).

.DESCRIPTION
    Creates basic placeholder BMP and ICO files with correct dimensions and formats.
    These are for testing the build process - replace with proper branded graphics before release.

.PARAMETER Force
    Overwrite existing files.

.EXAMPLE
    .\Generate-PlaceholderAssets.ps1
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param([switch]$Force)

$ErrorActionPreference = 'Stop'
$script:RootDir = $PSScriptRoot -replace '\\scripts$', ''
if (-not $script:RootDir) { $script:RootDir = (Get-Location).Path }

Add-Type -AssemblyName System.Drawing

function Write-Status {
    param([string]$Message, [string]$Status = 'Info')
    $color = switch ($Status) { 'Success' { 'Green' } 'Error' { 'Red' } 'Warning' { 'Yellow' } default { 'Cyan' } }
    Write-Host $Message -ForegroundColor $color
}

function New-PlaceholderBmp {
    param([string]$Path, [int]$Width, [int]$Height, [string]$Text, [System.Drawing.Color]$Color1, [System.Drawing.Color]$Color2)
    if ((Test-Path $Path) -and -not $Force) {
        Write-Status "Skipping (exists): $Path" 'Warning'
        return
    }
    $bmp = New-Object System.Drawing.Bitmap($Width, $Height)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    try {
        $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
            [System.Drawing.Point]::new(0, 0),
            [System.Drawing.Point]::new($Width, $Height),
            $Color1,
            $Color2
        )
        $g.FillRectangle($brush, 0, 0, $Width, $Height)
        if ($Text) {
            $font = New-Object System.Drawing.Font('Segoe UI', 12, [System.Drawing.FontStyle]::Bold)
            $sf = New-Object System.Drawing.StringFormat
            $sf.Alignment = [System.Drawing.StringAlignment]::Center
            $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
            $g.DrawString($Text, $font, [System.Drawing.Brushes]::White, [System.Drawing.RectangleF]::new(0, 0, $Width, $Height), $sf)
        }
        $bmp.Save($Path, [System.Drawing.Imaging.ImageFormat]::Bmp)
        Write-Status "Created: $Path" 'Success'
    } finally {
        $g.Dispose()
        $bmp.Dispose()
    }
}

function New-PlaceholderIco {
    param([string]$Path, [int[]]$Sizes = @(16, 32, 48, 256))
    if ((Test-Path $Path) -and -not $Force) {
        Write-Status "Skipping (exists): $Path" 'Warning'
        return
    }
    $images = New-Object System.Collections.ArrayList
    try {
        foreach ($size in $Sizes) {
            $bmp = New-Object System.Drawing.Bitmap($size, $size)
            $g = [System.Drawing.Graphics]::FromImage($bmp)
            try {
                $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
                $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
                    [System.Drawing.Rectangle]::new(0, 0, $size, $size),
                    [System.Drawing.Color]::FromArgb(107, 70, 193),
                    [System.Drawing.Color]::FromArgb(59, 130, 246),
                    [System.Drawing.Drawing2D.LinearGradientMode]::Diagonal
                )
                $g.FillEllipse($brush, 2, 2, $size - 4, $size - 4)
                $font = New-Object System.Drawing.Font('Arial', ($size * 0.6), [System.Drawing.FontStyle]::Bold)
                $sf = New-Object System.Drawing.StringFormat
                $sf.Alignment = [System.Drawing.StringAlignment]::Center
                $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
                $g.DrawString('P', $font, [System.Drawing.Brushes]::White, [System.Drawing.RectangleF]::new(0, 0, $size, $size), $sf)
                $images.Add($bmp) | Out-Null
            } catch {
                $bmp.Dispose()
                throw
            }
        }
        $icoStream = New-Object System.IO.MemoryStream
        try {
            $icoWriter = New-Object System.IO.BinaryWriter($icoStream)
            $icoWriter.Write([int16]0)
            $icoWriter.Write([int16]1)
            $icoWriter.Write([int16]$images.Count)
            $offset = 6 + ($images.Count * 16)
            foreach ($img in $images) {
                $w = if ($img.Width -ge 256) { [byte]0 } else { [byte]$img.Width }
                $h = if ($img.Height -ge 256) { [byte]0 } else { [byte]$img.Height }
                $icoWriter.Write($w)
                $icoWriter.Write($h)
                $icoWriter.Write([int16]0)
                $icoWriter.Write([int16]1)
                $icoWriter.Write([int16]32)
                $data = [System.IO.MemoryStream]::new()
                $img.Save($data, [System.Drawing.Imaging.ImageFormat]::Png)
                $dataLength = [int]$data.Length
                $icoWriter.Write([int32]$dataLength)
                $icoWriter.Write([int32]$offset)
                $offset += $dataLength
            }
            foreach ($img in $images) {
                $data = [System.IO.MemoryStream]::new()
                $img.Save($data, [System.Drawing.Imaging.ImageFormat]::Png)
                $icoWriter.Write($data.ToArray())
            }
            $icoStream.Position = 0
            $fileStream = [System.IO.File]::Create($Path)
            try {
                $icoStream.CopyTo($fileStream)
            } finally {
                $fileStream.Close()
            }
            Write-Status "Created: $Path" 'Success'
        } finally {
            $icoStream.Dispose()
        }
    } finally {
        foreach ($img in $images) { $img.Dispose() }
    }
}

try {
    Write-Status 'Generating placeholder graphics assets...' 'Info'
    Write-Status 'Note: Replace with proper branded graphics before release!' 'Warning'
    Write-Host ''

    $purple = [System.Drawing.Color]::FromArgb(107, 70, 193)
    $blue = [System.Drawing.Color]::FromArgb(59, 130, 246)

    $appIcon = Join-Path $script:RootDir 'PixelPulse\Resources\icon.ico'
    $banner = Join-Path $script:RootDir 'PixelPulse.Installer\Assets\banner.bmp'
    $dialog = Join-Path $script:RootDir 'PixelPulse.Installer\Assets\dialog.bmp'
    $installerIcon = Join-Path $script:RootDir 'PixelPulse.Installer\Assets\icon.ico'

    foreach ($dir in @(
        (Split-Path $appIcon),
        (Split-Path $banner),
        (Split-Path $dialog),
        (Split-Path $installerIcon)
    )) {
        if (-not (Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
        }
    }

    if ($PSCmdlet.ShouldProcess($appIcon, 'Create placeholder icon')) {
        New-PlaceholderIco -Path $appIcon -Sizes @(16, 32, 48, 256)
    }

    if ($PSCmdlet.ShouldProcess($banner, 'Create placeholder banner')) {
        New-PlaceholderBmp -Path $banner -Width 493 -Height 58 -Text 'Pixel Tech Solutions' -Color1 $purple -Color2 $blue
    }

    if ($PSCmdlet.ShouldProcess($dialog, 'Create placeholder dialog')) {
        New-PlaceholderBmp -Path $dialog -Width 493 -Height 312 -Text '' -Color1 $purple -Color2 $blue
    }

    if ($PSCmdlet.ShouldProcess($installerIcon, 'Create placeholder installer icon')) {
        Copy-Item -Path $appIcon -Destination $installerIcon -Force -ErrorAction SilentlyContinue
        Write-Status "Created: $installerIcon (copied from app icon)" 'Success'
    }

    Write-Host ''
    Write-Status 'Placeholder assets created. Run Validate-Assets.ps1 to verify.' 'Success'
    exit 0
} catch {
    Write-Status "Error: $_" 'Error'
    exit 1
}
