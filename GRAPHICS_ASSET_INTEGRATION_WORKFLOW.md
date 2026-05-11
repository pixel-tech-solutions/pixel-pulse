# Graphics Asset Integration Workflow

This document outlines the step-by-step process for integrating graphics assets into the Pixel Pulse project.

## Prerequisites

Before starting, ensure you have:
- Created all graphics assets according to the guides:
  - `ICON_CREATION_GUIDE.md` - For application icons
  - `GRAPHICS_CREATION_GUIDE.md` - For installer graphics
- Image editing software (GIMP, Photoshop, Paint.NET, or similar)
- Basic understanding of file formats (ICO, BMP)

## Step 1: Create Graphics Assets

Follow the detailed guides to create each asset:

### 1.1 Application Icon
- **Location:** `PixelPulse\Resources\icon.ico`
- **Guide:** `PixelPulse\Resources\ICON_CREATION_GUIDE.md`
- **Requirements:**
  - Multiple sizes: 16x16, 32x32, 48x48, 256x256
  - Format: ICO
  - Design: Pixel Pulse branding with purple/blue gradient

### 1.2 Installer Graphics
- **Location:** `PixelPulse.Installer\Assets\`
- **Guide:** `PixelPulse.Installer\Assets\GRAPHICS_CREATION_GUIDE.md`
- **Files needed:**
  - `banner.bmp` - 493x58 pixels
  - `dialog.bmp` - 493x312 pixels
  - `icon.ico` - Multiple sizes

## Step 2: Verify File Formats

Before placing files, verify:

### ICO Files
- [ ] File extension is `.ico`
- [ ] Multiple sizes are embedded (check with ICO viewer)
- [ ] File opens correctly in Windows Explorer

### BMP Files
- [ ] File extension is `.bmp`
- [ ] Dimensions match exactly (493x58 for banner, 493x312 for dialog)
- [ ] Color depth is 24-bit or 32-bit RGB
- [ ] File opens correctly in image viewer

## Step 3: Place Files in Correct Locations

### Application Icon
```
PixelPulse\
  └── Resources\
      └── icon.ico  ← Place here
```

### Installer Assets
```
PixelPulse.Installer\
  └── Assets\
      ├── banner.bmp  ← Place here
      ├── dialog.bmp  ← Place here
      └── icon.ico    ← Place here
```

## Step 4: Verify Project References

### Check `PixelPulse.csproj`
The icon should be referenced:
```xml
<ApplicationIcon>Resources\icon.ico</ApplicationIcon>
```

### Check `PixelPulse.Installer.wixproj`
Assets should be listed as EmbeddedResource:
```xml
<EmbeddedResource Include="Assets\banner.bmp" />
<EmbeddedResource Include="Assets\dialog.bmp" />
<EmbeddedResource Include="Assets\icon.ico" />
```

## Step 5: Remove Placeholder Files

After placing actual assets, remove placeholder files:
- [ ] Delete `PixelPulse\Resources\PLACEHOLDER_README.txt`
- [ ] Delete `PixelPulse.Installer\Assets\PLACEHOLDER_README.txt`

## Step 6: Rebuild Projects

### 6.1 Clean Build
```powershell
dotnet clean PixelPulse\PixelPulse.csproj
dotnet clean PixelPulse.Installer\PixelPulse.Installer.wixproj
```

### 6.2 Restore Packages
```powershell
dotnet restore
```

### 6.3 Build Application
```powershell
dotnet build PixelPulse\PixelPulse.csproj --configuration Release
```

### 6.4 Build Installer
```powershell
msbuild PixelPulse.Installer\PixelPulse.Installer.wixproj /p:Configuration=Release
```

## Step 7: Verify Graphics Display

### 7.1 Application Icon
- [ ] Icon appears in Windows Explorer for `.exe` file
- [ ] Icon appears in taskbar when application is running
- [ ] Icon appears in system tray

### 7.2 Installer Graphics
- [ ] Banner appears on welcome page
- [ ] Dialog background appears on all installer pages
- [ ] Installer icon appears in Windows Explorer for `.msi` file

## Step 8: Test Application

Run the application and verify:
- [ ] Splash screen displays correctly
- [ ] About dialog displays correctly
- [ ] All UI elements render properly
- [ ] No missing image errors in console

## Troubleshooting

### Icon Not Appearing
- **Issue:** Icon doesn't show in Windows Explorer
- **Solution:** 
  1. Verify ICO file has multiple sizes embedded
  2. Rebuild project completely (`dotnet clean` then `dotnet build`)
  3. Check file path in `.csproj` is correct

### Installer Graphics Not Showing
- **Issue:** Graphics don't appear in installer UI
- **Solution:**
  1. Verify BMP files are exactly the correct dimensions
  2. Check file format is 24-bit or 32-bit RGB
  3. Verify files are listed in `.wixproj` as EmbeddedResource
  4. Rebuild installer project

### Build Errors
- **Issue:** Build fails with missing file errors
- **Solution:**
  1. Verify all files are in correct locations
  2. Check file names match exactly (case-sensitive)
  3. Ensure files are not corrupted
  4. Try cleaning and rebuilding

## Validation Checklist

Before considering integration complete:

- [ ] All graphics assets are created
- [ ] All files are in correct locations
- [ ] File formats are correct
- [ ] Project files reference assets correctly
- [ ] Application builds successfully
- [ ] Installer builds successfully
- [ ] Graphics display correctly in application
- [ ] Graphics display correctly in installer
- [ ] No build warnings related to graphics

## Next Steps

After successful integration:
1. Run full application tests (see `TESTING_PLAN.md`)
2. Verify installer functionality
3. Test on clean Windows installation
4. Prepare for distribution
