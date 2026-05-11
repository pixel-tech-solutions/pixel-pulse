# Pre-Build Checklist

This checklist should be completed before attempting to build the Pixel Pulse application.

## Code Verification

- [x] All project files are correctly configured
- [x] All using statements are present and correct
- [x] All namespace declarations are correct
- [x] XAML files compile correctly (event handlers match code-behind)
- [x] Database context is properly configured
- [x] Data ingestion classes have correct dependencies
- [x] Error handling is in place for critical operations

## Project Configuration

- [x] `PixelPulse.csproj` - Target framework is `net8.0-windows`
- [x] `PixelPulse.csproj` - `UseWPF` and `UseWindowsForms` are set
- [x] `PixelPulse.csproj` - Application icon path is correct (Resources\icon.ico)
- [x] `PixelPulse.csproj` - Assembly metadata is complete
- [x] `PixelPulse.DatabaseBuilder.csproj` - Target framework is `net8.0`
- [x] `PixelPulse.DatabaseBuilder.csproj` - Project reference to PixelPulse is correct
- [x] `PixelPulse.sln` - All projects are included

## Graphics Assets

- [ ] `PixelPulse\Resources\icon.ico` - Application icon created (16x16, 32x32, 48x48, 256x256 sizes)
- [ ] `PixelPulse.Installer\Assets\banner.bmp` - Installer banner created (493x58 pixels)
- [ ] `PixelPulse.Installer\Assets\dialog.bmp` - Dialog background created (493x312 pixels)
- [ ] `PixelPulse.Installer\Assets\icon.ico` - Installer icon created

**Note:** The application can build without graphics assets, but they should be created before release.

## Dependencies

- [ ] .NET 8.0 SDK is installed
- [ ] NuGet packages can be restored
- [ ] WiX Toolset v3.x or v4.x is installed (for installer build)

## Database

- [ ] Database builder can run successfully
- [ ] Database file will be created in `%AppData%\PixelPulse\quotes.db`
- [ ] Application can start without database (shows appropriate message)

## Build Process

1. Restore NuGet packages:
   ```powershell
   dotnet restore
   ```

2. Build PixelPulse:
   ```powershell
   dotnet build PixelPulse\PixelPulse.csproj --configuration Release
   ```

3. Build DatabaseBuilder:
   ```powershell
   dotnet build PixelPulse.DatabaseBuilder\PixelPulse.DatabaseBuilder.csproj --configuration Release
   ```

4. Build Installer (if WiX is installed):
   ```powershell
   msbuild PixelPulse.Installer\PixelPulse.Installer.wixproj /p:Configuration=Release
   ```

## Post-Build Verification

- [ ] Executable files are created in `bin\Release\net8.0-windows\`
- [ ] File sizes are reasonable (not suspiciously large or small)
- [ ] Icons are embedded (if assets exist)
- [ ] No critical build warnings
- [ ] Installer MSI is created (if WiX build succeeded)

## Known Limitations

- Graphics assets must be created manually (see `ICON_CREATION_GUIDE.md` and `GRAPHICS_CREATION_GUIDE.md`)
- WiX Toolset must be installed for installer build
- Database must be populated before quotes display
- Some features require graphics assets to be complete

## Next Steps

After completing this checklist:

1. Create graphics assets using provided guides
2. Integrate graphics assets into projects
3. Build release version
4. Test application functionality (see `TESTING_PLAN.md`)
5. Build installer
6. Test installation process
7. Prepare for distribution
