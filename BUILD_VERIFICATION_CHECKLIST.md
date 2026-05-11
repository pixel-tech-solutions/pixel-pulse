# Build Verification Checklist

## Prerequisites

- [ ] .NET 8.0 SDK installed
- [ ] WiX Toolset v3.x or later installed
- [ ] Visual Studio 2022 (optional, can use MSBuild)
- [ ] All graphics assets created (see below)

## Graphics Assets Checklist

### Application Assets
- [ ] `PixelPulse\Resources\icon.ico` exists
  - Contains multiple sizes (16, 32, 48, 256)
  - Uses brand colors (Purple/Blue)
  - Professional design

### Installer Assets
- [ ] `PixelPulse.Installer\Assets\banner.bmp` exists
  - Dimensions: 493x58 pixels
  - Format: 24-bit BMP
  - Contains "Pixel Tech Solutions" branding
  
- [ ] `PixelPulse.Installer\Assets\dialog.bmp` exists
  - Dimensions: 493x312 pixels
  - Format: 24-bit BMP
  - Subtle background design
  
- [ ] `PixelPulse.Installer\Assets\icon.ico` exists
  - Same as application icon (or copy)
  - Multiple sizes embedded

- [ ] `PixelPulse.Installer\Assets\license.rtf` exists
  - Company information correct
  - CEO credit included
  - Valid RTF format

## Application Build Checklist

### Pre-Build
- [ ] All project files are present
- [ ] NuGet packages restored (`dotnet restore`)
- [ ] No compilation errors
- [ ] Icon file path is correct in csproj

### Build Steps
1. [ ] Build PixelPulse project: `dotnet build PixelPulse\PixelPulse.csproj`
2. [ ] Verify build succeeds without errors
3. [ ] Check output: `PixelPulse\bin\Release\PixelPulse.exe` exists
4. [ ] Verify icon is embedded in executable

### Post-Build Verification
- [ ] Application executable runs
- [ ] Splash screen displays correctly
- [ ] Main window appears
- [ ] About dialog accessible
- [ ] System tray icon appears
- [ ] Application icon displays in taskbar

## Installer Build Checklist

### Pre-Build
- [ ] WiX Toolset is installed and in PATH
- [ ] All installer assets exist (see Graphics Assets)
- [ ] Product.wxs references are correct
- [ ] Project references are correct

### Build Steps
1. [ ] Build PixelPulse project first (dependency)
2. [ ] Build installer: `msbuild PixelPulse.Installer\PixelPulse.Installer.wixproj /p:Configuration=Release`
3. [ ] Verify build succeeds
4. [ ] Check output: `PixelPulse.Installer\bin\Release\PixelPulseInstaller.msi` exists

### Post-Build Verification
- [ ] MSI file size is reasonable (not 0 bytes)
- [ ] MSI file can be opened
- [ ] Installer properties show correct information:
  - Product Name: "Pixel Pulse"
  - Manufacturer: "Pixel Tech Solutions"
  - Version: 1.0.0

## Installation Testing Checklist

### Fresh Installation
- [ ] Run installer on clean system (or VM)
- [ ] Welcome page displays correctly
- [ ] Banner graphic appears
- [ ] License agreement displays
- [ ] Install directory selection works
- [ ] Installation completes successfully
- [ ] Application launches after installation
- [ ] Start Menu shortcut created
- [ ] Desktop shortcut created (if selected)
- [ ] Application appears in "Add/Remove Programs"

### Application Functionality
- [ ] Application starts correctly
- [ ] Splash screen displays
- [ ] Main window appears with quotes
- [ ] Settings window opens
- [ ] About dialog displays company info
- [ ] System tray icon works
- [ ] Right-click context menu works
- [ ] All features function correctly

### Uninstallation Testing
- [ ] Uninstaller launches from Control Panel
- [ ] Uninstallation completes successfully
- [ ] Application files removed
- [ ] Start Menu shortcuts removed
- [ ] Desktop shortcut removed (if created)
- [ ] Registry entries cleaned (if any)
- [ ] AppData folder remains (user data preserved)

## Branding Verification Checklist

### Application Branding
- [ ] Company name appears in About dialog
- [ ] CEO credit visible in About dialog
- [ ] Contact information correct
- [ ] Brand colors used consistently
- [ ] Splash screen has branding
- [ ] Application title includes "Pixel Pulse"

### Installer Branding
- [ ] Banner displays "Pixel Tech Solutions"
- [ ] Dialog backgrounds use brand colors
- [ ] Installer title shows "Pixel Pulse"
- [ ] Manufacturer shows "Pixel Tech Solutions"
- [ ] License agreement includes company info

### Consistency Check
- [ ] Colors match across all elements
- [ ] Typography is consistent
- [ ] Company name spelling is consistent
- [ ] Contact information matches everywhere

## Performance Checklist

- [ ] Application starts quickly (< 3 seconds)
- [ ] Splash screen doesn't delay startup excessively
- [ ] Memory usage is reasonable
- [ ] No memory leaks during extended use
- [ ] Database queries are efficient
- [ ] UI remains responsive

## Error Handling Checklist

- [ ] Graceful handling of missing database
- [ ] Error messages are user-friendly
- [ ] Application doesn't crash on errors
- [ ] Logging works (if implemented)
- [ ] Settings file corruption handled

## Documentation Checklist

- [ ] README.md is complete
- [ ] BRAND_GUIDELINES.md exists
- [ ] CHANGELOG.md is up to date
- [ ] LICENSE.txt is present
- [ ] INSTALLER_BUILD_INSTRUCTIONS.md is complete
- [ ] Graphics creation guides exist

## Final Release Checklist

- [ ] All graphics assets are final versions
- [ ] All tests pass
- [ ] Documentation is complete
- [ ] Version number is correct (1.0.0)
- [ ] Copyright year is correct (2026)
- [ ] Release notes prepared
- [ ] Installer MSI is signed (optional but recommended)
- [ ] Distribution package prepared

## Sign-Off

**Build Verified By:** _________________  
**Date:** _________________  
**Version:** 1.0.0  
**Status:** ☐ Ready for Release  ☐ Needs Work
