# Asset Validation Guide

This guide helps verify that graphics assets are correctly formatted and ready for integration.

## Validation Checklist

Before integrating graphics assets, verify each file meets these requirements:

## Application Icon (`icon.ico`)

### File Location
```
PixelPulse\Resources\icon.ico
```

### Requirements
- [ ] File exists
- [ ] File extension is `.ico`
- [ ] File size is reasonable (typically 50-500 KB)
- [ ] File opens in Windows Explorer and displays icon
- [ ] Multiple sizes are embedded:
  - [ ] 16x16 pixels
  - [ ] 32x32 pixels
  - [ ] 48x48 pixels
  - [ ] 256x256 pixels

### Validation Methods

#### Method 1: Windows Explorer
1. Navigate to `PixelPulse\Resources\`
2. Right-click `icon.ico`
3. Select "Properties"
4. Check "Details" tab for embedded sizes

#### Method 2: ICO Viewer Software
Use tools like:
- IcoFX
- Greenfish Icon Editor Pro
- Online ICO viewers

#### Method 3: PowerShell Script
```powershell
# Check if file exists and get basic info
$iconPath = "PixelPulse\Resources\icon.ico"
if (Test-Path $iconPath) {
    $file = Get-Item $iconPath
    Write-Host "File exists: $($file.FullName)"
    Write-Host "File size: $($file.Length) bytes"
    Write-Host "Last modified: $($file.LastWriteTime)"
} else {
    Write-Host "File not found!"
}
```

## Installer Banner (`banner.bmp`)

### File Location
```
PixelPulse.Installer\Assets\banner.bmp
```

### Requirements
- [ ] File exists
- [ ] File extension is `.bmp`
- [ ] Dimensions are exactly **493x58 pixels**
- [ ] Color depth is 24-bit or 32-bit RGB
- [ ] File opens correctly in image viewer
- [ ] No transparency (WiX installer limitation)

### Validation Methods

#### Method 1: Image Viewer
1. Open file in Windows Photos, Paint, or similar
2. Check image properties for dimensions
3. Verify dimensions are 493x58

#### Method 2: PowerShell Script
```powershell
Add-Type -AssemblyName System.Drawing
$imagePath = "PixelPulse.Installer\Assets\banner.bmp"
if (Test-Path $imagePath) {
    $image = [System.Drawing.Image]::FromFile($imagePath)
    Write-Host "Width: $($image.Width) pixels"
    Write-Host "Height: $($image.Height) pixels"
    Write-Host "Format: $($image.PixelFormat)"
    $image.Dispose()
    
    if ($image.Width -eq 493 -and $image.Height -eq 58) {
        Write-Host "✅ Dimensions correct!"
    } else {
        Write-Host "❌ Dimensions incorrect! Expected 493x58"
    }
} else {
    Write-Host "File not found!"
}
```

## Dialog Background (`dialog.bmp`)

### File Location
```
PixelPulse.Installer\Assets\dialog.bmp
```

### Requirements
- [ ] File exists
- [ ] File extension is `.bmp`
- [ ] Dimensions are exactly **493x312 pixels**
- [ ] Color depth is 24-bit or 32-bit RGB
- [ ] File opens correctly in image viewer
- [ ] No transparency (WiX installer limitation)

### Validation Methods
Same as banner validation (Method 1 or Method 2 above, adjusting dimensions to 493x312)

## Installer Icon (`icon.ico`)

### File Location
```
PixelPulse.Installer\Assets\icon.ico
```

### Requirements
- [ ] File exists
- [ ] File extension is `.ico`
- [ ] Multiple sizes embedded (same as application icon)
- [ ] File opens correctly

### Validation Methods
Same as application icon validation

## Automated Validation Script

Create a PowerShell script to validate all assets:

```powershell
# Asset Validation Script for Pixel Pulse
$errors = @()

# Check application icon
$appIcon = "PixelPulse\Resources\icon.ico"
if (-not (Test-Path $appIcon)) {
    $errors += "Application icon not found: $appIcon"
} else {
    Write-Host "✅ Application icon found"
}

# Check installer assets
$installerAssets = @(
    "PixelPulse.Installer\Assets\banner.bmp",
    "PixelPulse.Installer\Assets\dialog.bmp",
    "PixelPulse.Installer\Assets\icon.ico"
)

foreach ($asset in $installerAssets) {
    if (-not (Test-Path $asset)) {
        $errors += "Installer asset not found: $asset"
    } else {
        Write-Host "✅ Found: $asset"
    }
}

# Validate BMP dimensions (requires System.Drawing)
if ($PSVersionTable.PSVersion.Major -ge 5) {
    Add-Type -AssemblyName System.Drawing
    
    # Check banner
    $bannerPath = "PixelPulse.Installer\Assets\banner.bmp"
    if (Test-Path $bannerPath) {
        $banner = [System.Drawing.Image]::FromFile($bannerPath)
        if ($banner.Width -ne 493 -or $banner.Height -ne 58) {
            $errors += "Banner dimensions incorrect: $($banner.Width)x$($banner.Height) (expected 493x58)"
        } else {
            Write-Host "✅ Banner dimensions correct"
        }
        $banner.Dispose()
    }
    
    # Check dialog
    $dialogPath = "PixelPulse.Installer\Assets\dialog.bmp"
    if (Test-Path $dialogPath) {
        $dialog = [System.Drawing.Image]::FromFile($dialogPath)
        if ($dialog.Width -ne 493 -or $dialog.Height -ne 312) {
            $errors += "Dialog dimensions incorrect: $($dialog.Width)x$($dialog.Height) (expected 493x312)"
        } else {
            Write-Host "✅ Dialog dimensions correct"
        }
        $dialog.Dispose()
    }
}

# Report results
if ($errors.Count -eq 0) {
    Write-Host "`n✅ All assets validated successfully!"
} else {
    Write-Host "`n❌ Validation errors found:"
    foreach ($error in $errors) {
        Write-Host "  - $error"
    }
    exit 1
}
```

## Common Issues and Solutions

### Issue: ICO File Shows Only One Size
**Solution:** Use ICO editor to add multiple sizes. ICO files can contain multiple images.

### Issue: BMP Dimensions Don't Match
**Solution:** Resize image to exact dimensions using image editing software. WiX requires exact dimensions.

### Issue: BMP File Won't Open
**Solution:** 
1. Verify file is not corrupted
2. Check file format is BMP (not PNG renamed to BMP)
3. Try opening in different image viewer

### Issue: Colors Look Wrong in Installer
**Solution:** 
1. Ensure color depth is 24-bit or 32-bit RGB
2. Avoid transparency (WiX doesn't support it)
3. Use standard RGB color space

### Issue: File Too Large
**Solution:**
- Optimize images before saving
- For BMP: Use 24-bit instead of 32-bit if transparency not needed
- For ICO: Remove unnecessary sizes

## Pre-Integration Checklist

Before integrating assets into the project:

- [ ] All files exist in correct locations
- [ ] All file formats are correct (.ico, .bmp)
- [ ] All dimensions are correct
- [ ] All files open correctly
- [ ] ICO files have multiple sizes
- [ ] BMP files are correct color depth
- [ ] No corrupted files
- [ ] File names match exactly (case-sensitive)

## Post-Integration Verification

After placing files and building:

- [ ] Application icon appears in Windows Explorer
- [ ] Installer graphics appear in installer UI
- [ ] No build errors related to missing files
- [ ] No runtime errors related to graphics

## Tools for Asset Creation

### Free Tools
- **GIMP** - Full-featured image editor
- **Paint.NET** - Simple image editor for Windows
- **IcoFX** - Free ICO editor
- **Greenfish Icon Editor Pro** - Free icon editor

### Online Tools
- **Favicon.io** - ICO generator
- **Convertio** - File format converter
- **Online Image Resizer** - Dimension adjustment

### Paid Tools
- **Adobe Photoshop** - Professional image editing
- **Adobe Illustrator** - Vector graphics
- **Axialis IconWorkshop** - Professional icon editor

## Next Steps

After validation:
1. Follow `GRAPHICS_ASSET_INTEGRATION_WORKFLOW.md`
2. Integrate assets into project
3. Rebuild projects
4. Verify graphics display correctly
