# Build Process Summary

This document summarizes the complete build process for Pixel Pulse, including prerequisites, steps, and verification.

## Prerequisites

### Required Software
1. **.NET 8.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version` (should show 8.0.x)

2. **WiX Toolset v3.x or v4.x** (for installer build)
   - Download from: https://wixtoolset.org/releases/
   - Verify installation: `candle.exe` should be in PATH or WiX bin directory

3. **Visual Studio 2022** (optional, but recommended)
   - Community edition is free
   - Includes .NET SDK and build tools

### Graphics Assets
- Application icon: `PixelPulse\Resources\icon.ico`
- Installer banner: `PixelPulse.Installer\Assets\banner.bmp`
- Dialog background: `PixelPulse.Installer\Assets\dialog.bmp`
- Installer icon: `PixelPulse.Installer\Assets\icon.ico`

**Note:** Application can build without graphics assets, but they should be created before release.

## Build Steps

### Step 1: Restore NuGet Packages

```powershell
cd c:\Users\ssego\Documents\Pixel-Pulse
dotnet restore
```

**Expected Output:**
- NuGet packages downloaded
- Package references resolved
- No errors

### Step 2: Build Main Application

```powershell
dotnet build PixelPulse\PixelPulse.csproj --configuration Release
```

**Expected Output:**
- Build succeeds
- Executable created: `PixelPulse\bin\Release\net8.0-windows\PixelPulse.exe`
- No compilation errors

**If icon.ico is missing:**
- Build will succeed with warning
- Icon won't be embedded (can be added later)

### Step 3: Build Database Builder

```powershell
dotnet build PixelPulse.DatabaseBuilder\PixelPulse.DatabaseBuilder.csproj --configuration Release
```

**Expected Output:**
- Build succeeds
- Executable created: `PixelPulse.DatabaseBuilder\bin\Release\net8.0\PixelPulse.DatabaseBuilder.exe`

### Step 4: Build Installer (Optional)

**Prerequisites:** WiX Toolset must be installed

```powershell
msbuild PixelPulse.Installer\PixelPulse.Installer.wixproj /p:Configuration=Release
```

**Or using Visual Studio:**
1. Open `PixelPulse.sln`
2. Right-click `PixelPulse.Installer` project
3. Select "Build"

**Expected Output:**
- Installer MSI created: `PixelPulse.Installer\bin\Release\PixelPulse.msi`
- No build errors

**If graphics assets are missing:**
- Build may fail or show warnings
- Create assets or temporarily comment out references

## Build Output Locations

### Application
```
PixelPulse\bin\Release\net8.0-windows\
  ├── PixelPulse.exe
  ├── PixelPulse.dll
  ├── PixelPulse.deps.json
  ├── PixelPulse.runtimeconfig.json
  └── [Dependencies...]
```

### Database Builder
```
PixelPulse.DatabaseBuilder\bin\Release\net8.0\
  ├── PixelPulse.DatabaseBuilder.exe
  ├── PixelPulse.DatabaseBuilder.dll
  └── [Dependencies...]
```

### Installer
```
PixelPulse.Installer\bin\Release\
  └── PixelPulse.msi
```

## Verification Steps

### 1. Verify Application Build
- [ ] `PixelPulse.exe` exists
- [ ] File size is reasonable (typically 50-200 KB for .exe)
- [ ] Icon appears in Windows Explorer (if icon.ico exists)
- [ ] No build errors or warnings

### 2. Verify Database Builder Build
- [ ] `PixelPulse.DatabaseBuilder.exe` exists
- [ ] Can run without errors
- [ ] Can create database successfully

### 3. Verify Installer Build
- [ ] `PixelPulse.msi` exists
- [ ] File size is reasonable (typically 1-10 MB)
- [ ] Can install on test system
- [ ] Graphics appear in installer UI (if assets exist)

## Troubleshooting

### Build Errors

#### "dotnet is not recognized"
**Solution:** 
1. Install .NET 8.0 SDK
2. Add to PATH or use full path to dotnet.exe
3. Restart terminal/IDE

#### "Package restore failed"
**Solution:**
1. Check internet connection
2. Verify NuGet sources are accessible
3. Clear NuGet cache: `dotnet nuget locals all --clear`
4. Try restore again

#### "MSBuild not found" (for installer)
**Solution:**
1. Install WiX Toolset
2. Use Visual Studio Developer Command Prompt
3. Or add MSBuild to PATH

#### "Missing graphics assets"
**Solution:**
1. Create assets using guides
2. Place in correct locations
3. Rebuild project
4. Or temporarily comment out asset references

### Runtime Errors

#### "Application won't start"
**Solution:**
1. Check .NET 8.0 Desktop Runtime is installed
2. Check Windows version compatibility
3. Review error logs in Event Viewer

#### "Database errors"
**Solution:**
1. Run DatabaseBuilder to create database
2. Check database file permissions
3. Verify database path is accessible

## Build Configurations

### Debug Configuration
- Includes debug symbols
- Optimizations disabled
- Larger file sizes
- Better for development

**Build command:**
```powershell
dotnet build --configuration Debug
```

### Release Configuration
- Optimized code
- No debug symbols (unless specified)
- Smaller file sizes
- Better for distribution

**Build command:**
```powershell
dotnet build --configuration Release
```

## Continuous Integration (CI) Setup

### GitHub Actions Example
```yaml
name: Build Pixel Pulse

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --configuration Release --no-restore
      - name: Publish artifacts
        uses: actions/upload-artifact@v3
        with:
          name: PixelPulse
          path: PixelPulse/bin/Release/net8.0-windows/
```

## Next Steps After Build

1. **Test Application**
   - Follow `TESTING_PLAN.md`
   - Verify all functionality works
   - Test on clean Windows installation

2. **Create Database**
   - Run `PixelPulse.DatabaseBuilder.exe`
   - Wait for import to complete
   - Verify quotes are available

3. **Test Installer**
   - Install on test system
   - Verify installation works
   - Test uninstallation

4. **Prepare Distribution**
   - Package application and installer
   - Create distribution package
   - Prepare release notes

## Build Time Estimates

- **Restore packages:** 30-60 seconds
- **Build application:** 10-30 seconds
- **Build database builder:** 5-15 seconds
- **Build installer:** 30-60 seconds (if WiX installed)

**Total build time:** Approximately 2-3 minutes

## Success Criteria

✅ Build completes without errors  
✅ All executables are created  
✅ File sizes are reasonable  
✅ Application runs successfully  
✅ Installer installs correctly  
✅ Graphics display correctly (if assets exist)

## Support

If you encounter build issues:
1. Check `CODE_VERIFICATION_REPORT.md` for known issues
2. Review `PRE_BUILD_CHECKLIST.md` for prerequisites
3. Check build logs for specific errors
4. Verify all prerequisites are installed
