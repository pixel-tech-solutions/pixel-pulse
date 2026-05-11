# Pixel Pulse

**Inspiration at Your Fingertips**

A beautiful Windows desktop application that displays floating quotes in the top-right corner with transparency, click-through mode, fade animations, and comprehensive quote database integration.

**Developed by Pixel Tech Solutions**  
**CEO:** Stephen Ssegonga

---

## About Pixel Tech Solutions

Pixel Tech Solutions is a software development company focused on creating innovative, user-friendly applications that enhance productivity and inspiration. Pixel Pulse represents our commitment to delivering high-quality software with professional design and exceptional user experience.

## Features

- **Floating Quotes**: Borderless, transparent window displaying quotes
- **10 Quote Categories**: Motivational, Bible, Love, Tech, Business, Success, Leadership, Wisdom, Encouragement, Life
- **Quote Matching**: Combine quotes from different categories (e.g., Motivational + Bible)
- **Customizable Display**: Font, size, color, position, fade timing
- **Click-Through Mode**: Mouse passes through the window (configurable)
- **System Tray Integration**: Minimize to tray, right-click menu
- **Auto-Start**: Option to start with Windows
- **Offline Database**: SQLite database with 100,000+ quotes

## Requirements

- Windows 10/11
- .NET 8.0 Desktop Runtime

## Installation

### Using the Installer (Recommended)

1. Download the `PixelPulseInstaller.msi` installer
2. Run the installer and follow the setup wizard
3. The installer will:
   - Install Pixel Pulse to your local AppData folder
   - Create Start Menu shortcuts
   - Optionally create a Desktop shortcut
   - Set up auto-start (if selected)
4. Launch Pixel Pulse from the Start Menu or Desktop shortcut

### Manual Building

1. Open the solution in Visual Studio 2022 or use the .NET CLI:
   ```bash
   dotnet restore
   dotnet build
   ```

2. Build the database (first time only):
   ```bash
   cd PixelPulse.DatabaseBuilder
   dotnet run
   ```
   This will download and import quotes from multiple sources (may take 30+ minutes).

3. Run the application:
   ```bash
   cd PixelPulse
   dotnet run
   ```

### Build and Utility Scripts (PowerShell)

From the repository root, run scripts in the `scripts` folder. Requires PowerShell 5.1+ and .NET 8.0 SDK.

| Script | Description |
|--------|-------------|
| `.\scripts\Setup-Project.ps1` | Verify .NET SDK, restore packages, create required directories. Run first on a new clone. |
| `.\scripts\Build-All.ps1` | Full build: restore, build PixelPulse and DatabaseBuilder (Release), and installer if WiX is installed. Use `-Configuration Debug` for debug build, `-SkipInstaller` to skip MSI. |
| `.\scripts\Build-Quick.ps1` | Quick build of PixelPulse only (Debug). Use during development for fast feedback. |
| `.\scripts\Clean-Build.ps1` | Remove `bin`/`obj` folders. Use `-Force` to skip confirmation, `-RemoveDatabase` to delete the local quotes database, `-IncludeNuGetCache` to clear NuGet cache. |
| `.\scripts\Validate-Assets.ps1` | Check graphics assets exist and (for BMPs) have correct dimensions. Use `-ReportPath report.json` for a JSON report. |
| `.\scripts\Build-Database.ps1` | Build and run DatabaseBuilder to populate the quotes database. Use `-SkipBuild` to only run an existing build. |
| `.\scripts\Reset-Database.ps1` | Remove the local database; use `-Backup` to copy it first, `-Rebuild` to run Build-Database.ps1 after. |
| `.\scripts\Run-Tests.ps1` | Smoke tests: verify exe exists, app data folder, and optionally launch app briefly. Use `-LaunchApp` and `-LaunchSeconds 5` for startup test. |
| `.\scripts\Test-Installer.ps1` | Verify MSI exists; optionally run silent install/uninstall test. Use `-SkipSilentInstall` to only check file. |
| `.\scripts\Update-Version.ps1` | Set version in .csproj and Product.wxs. Example: `.\scripts\Update-Version.ps1 -Version 1.0.1`. Use `-UpdateChangelog` to append a new CHANGELOG section. |

Configuration for scripts is in `scripts\config.json` (paths, dimensions, defaults). CI uses `.github\workflows\build.yml` (build and publish artifacts on push/PR).

## Usage

- **Right-click** on the quote window to access Settings, Refresh Quote, or Exit
- **Hover** over the quote to brighten it (if enabled)
- Configure all settings through the Settings window
- The quote refreshes automatically based on your refresh interval setting

## Data Sources

- Quote Garden API (75,000+ quotes)
- BibleInJson (31,000+ Bible verses)
- Quotable API
- Various GitHub repositories

## Project Structure

- `PixelPulse/` - Main WPF application
- `PixelPulse.DatabaseBuilder/` - Console app for importing quotes
- `PixelPulse.Installer/` - WiX installer project
- `scripts/` - PowerShell build, validation, database, and test scripts (see table above)
- `scripts/config.json` - Script configuration (paths, asset dimensions)
- `.github/workflows/` - GitHub Actions CI (build and artifacts)
- Database stored in `%AppData%\PixelPulse\quotes.db`
- Settings stored in `%AppData%\PixelPulse\settings.json`

## Version History

See [CHANGELOG.md](CHANGELOG.md) for detailed version history and release notes.

## Brand Guidelines

See [BRAND_GUIDELINES.md](BRAND_GUIDELINES.md) for Pixel Tech Solutions branding guidelines and color palette.

## Contact & Support

**Pixel Tech Solutions**  
**Email:** contact@pixeltechsolutions.com  
**Website:** www.pixeltechsolutions.com  
**CEO:** Stephen Ssegonga

For support, feature requests, or inquiries, please contact us at the email above.

## License

Copyright © 2026 Pixel Tech Solutions. All rights reserved.

This software is proprietary and protected by copyright laws. See LICENSE.txt for full license terms.

**Note:** This project uses data from various open-source quote databases. Please respect their individual licenses.
