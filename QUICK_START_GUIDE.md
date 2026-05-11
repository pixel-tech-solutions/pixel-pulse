# Quick Start Guide - Graphics Assets

## Overview

This guide provides a quick reference for creating the required graphics assets for Pixel Pulse.

## Required Assets

### 1. Application Icon
**Location:** `PixelPulse\Resources\icon.ico`

**Quick Steps:**
1. Create 256x256 pixel design
2. Use Purple (#6B46C1) and Blue (#3B82F6)
3. Design: Stylized "P" or pixel-themed icon
4. Export at sizes: 16, 32, 48, 256 pixels
5. Convert to ICO format
6. Save as `icon.ico`

**Tools:**
- Online: https://www.icoconverter.com/
- Desktop: IcoFX, Greenfish Icon Editor
- Design: GIMP, Photoshop, Inkscape

### 2. Installer Banner
**Location:** `PixelPulse.Installer\Assets\banner.bmp`

**Quick Steps:**
1. Create 493x58 pixel image
2. Horizontal gradient: Purple → Blue
3. Add text: "Pixel Tech Solutions" (white)
4. Export as 24-bit BMP
5. Save as `banner.bmp`

**Tools:**
- GIMP: File → Export As → .bmp (24-bit)
- Photoshop: Save As → BMP (Windows, 24-bit)
- Online: Convert PNG to BMP, then edit

### 3. Dialog Background
**Location:** `PixelPulse.Installer\Assets\dialog.bmp`

**Quick Steps:**
1. Create 493x312 pixel image
2. Subtle gradient using brand colors
3. Keep it very subtle (non-distracting)
4. Export as 24-bit BMP
5. Save as `dialog.bmp`

### 4. Installer Icon
**Location:** `PixelPulse.Installer\Assets\icon.ico`

**Quick Steps:**
1. Copy application icon OR create new one
2. Same format requirements as application icon
3. Save as `icon.ico` in Assets folder

## Brand Colors

- **Purple:** #6B46C1 (RGB: 107, 70, 193)
- **Blue:** #3B82F6 (RGB: 59, 130, 246)
- **Gold:** #F59E0B (RGB: 245, 158, 11)
- **Dark:** #1F2937 (RGB: 31, 41, 55)
- **Light:** #F9FAFB (RGB: 249, 250, 251)

## File Format Requirements

- **ICO files:** Must contain multiple sizes (16, 32, 48, 256)
- **BMP files:** Must be 24-bit format (required by WiX)
- **RTF files:** Already created ✓

## After Creating Assets

1. Place files in correct locations
2. Rebuild application: `dotnet build PixelPulse\PixelPulse.csproj`
3. Rebuild installer: `msbuild PixelPulse.Installer\PixelPulse.Installer.wixproj /p:Configuration=Release`
4. Test installer on clean system
5. Verify graphics display correctly

## Need Help?

- See `PixelPulse\Resources\ICON_CREATION_GUIDE.md` for detailed icon instructions
- See `PixelPulse.Installer\Assets\GRAPHICS_CREATION_GUIDE.md` for installer graphics
- See `BRAND_GUIDELINES.md` for brand specifications

## Quick Checklist

- [ ] icon.ico created and placed in `PixelPulse\Resources\`
- [ ] banner.bmp created and placed in `PixelPulse.Installer\Assets\`
- [ ] dialog.bmp created and placed in `PixelPulse.Installer\Assets\`
- [ ] icon.ico copied to `PixelPulse.Installer\Assets\`
- [ ] All files are correct format (ICO with multiple sizes, 24-bit BMP)
- [ ] Application builds successfully
- [ ] Installer builds successfully
- [ ] Graphics display correctly in installer
