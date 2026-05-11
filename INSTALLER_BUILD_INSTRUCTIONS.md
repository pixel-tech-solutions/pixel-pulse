# WiX Installer Build Instructions

## Prerequisites

1. **WiX Toolset v3.x or later**
   - Download from: https://wixtoolset.org/releases/
   - Install the WiX Toolset build tools
   - Ensure WiX is added to your PATH

2. **Visual Studio 2022** (recommended)
   - Or use MSBuild from command line

3. **.NET 8.0 SDK**
   - Required for building the main application

## Building the Installer

### Option 1: Using Visual Studio

1. Open `PixelPulse.sln` in Visual Studio 2022
2. Right-click on `PixelPulse.Installer` project
3. Select "Build"
4. The installer MSI will be created in `PixelPulse.Installer\bin\Release\` or `Debug\`

### Option 2: Using MSBuild Command Line

```bash
cd PixelPulse.Installer
msbuild PixelPulse.Installer.wixproj /p:Configuration=Release
```

### Option 3: Using WiX Command Line

```bash
cd PixelPulse.Installer
candle.exe Product.wxs UI\*.wxs
light.exe Product.wixobj UI\*.wixobj -out PixelPulseInstaller.msi
```

## Required Assets

Before building, ensure you have created the following assets in `PixelPulse.Installer\Assets\`:

1. **banner.bmp** (493x58 pixels) - Installer banner
2. **dialog.bmp** (493x312 pixels) - Dialog background
3. **icon.ico** - Application icon (multiple sizes)

See `PixelPulse.Installer\Assets\README_ASSETS.md` for details.

## Installer Features

- **Install Location:** `%LocalAppData%\Pixel Tech Solutions\Pixel Pulse`
- **Start Menu Shortcuts:** Created automatically
- **Desktop Shortcut:** Optional (user choice)
- **Auto-Start:** Configured via application settings
- **Uninstaller:** Included automatically

## Testing the Installer

1. Build the installer MSI
2. Run the MSI file
3. Follow the installation wizard
4. Verify installation in Start Menu
5. Launch Pixel Pulse
6. Test uninstallation from Control Panel

## Troubleshooting

### WiX Not Found
- Ensure WiX Toolset is installed
- Check that WiX is in your PATH
- Verify WIX environment variables

### Missing Assets
- Create placeholder images if assets are missing
- The installer will build but may not look professional

### Build Errors
- Ensure all project references are correct
- Verify GUIDs match in solution file
- Check that PixelPulse project builds successfully first

## Customization

To customize the installer:

1. Edit `Product.wxs` for product information
2. Modify UI dialogs in `UI\` folder
3. Update license text in `Assets\license.rtf`
4. Replace graphics in `Assets\` folder

## Notes

- The installer uses WiX UI Extension for standard dialogs
- Custom UI can be added by modifying dialog files
- .NET 8.0 runtime detection should be added for production use
- Code signing should be added for distribution
