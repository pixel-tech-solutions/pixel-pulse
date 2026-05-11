# Pixel Pulse Installer
# PowerShell-based installer for Pixel Pulse application

param(
    [string]$InstallPath = "$env:LOCALAPPDATA\PixelPulse",
    [switch]$CreateDesktopShortcut = $false,
    [switch]$CreateStartMenuShortcut = $true,
    [switch]$AutoStart = $false
)

Write-Host "========================================"
Write-Host "Pixel Pulse Installer"
Write-Host "========================================"
Write-Host ""

# Create installation directory
Write-Host "Creating installation directory..."
New-Item -ItemType Directory -Force -Path $InstallPath | Out-Null
Write-Host "Installation directory: $InstallPath"

# Copy application files
Write-Host "Copying application files..."
$appFiles = @(
    "PixelPulse\bin\Debug\net8.0-windows\PixelPulse.exe",
    "PixelPulse\bin\Debug\net8.0-windows\PixelPulse.dll",
    "PixelPulse\Resources\icon.ico"
)

foreach ($file in $appFiles) {
    if (Test-Path $file) {
        $fileName = Split-Path $file -Leaf
        $destPath = Join-Path $InstallPath $fileName
        Copy-Item -Path $file -Destination $destPath -Force
        Write-Host "  Copied: $fileName"
    } else {
        Write-Host "  Warning: File not found: $file"
    }
}

# Copy database if it exists
$dbPath = "$env:APPDATA\PixelPulse\quotes.db"
if (Test-Path $dbPath) {
    $dbDestPath = Join-Path $InstallPath "quotes.db"
    Copy-Item -Path $dbPath -Destination $dbDestPath -Force
    Write-Host "  Copied: quotes.db"
}

# Create desktop shortcut
if ($CreateDesktopShortcut) {
    Write-Host "Creating desktop shortcut..."
    $desktopPath = [Environment]::GetFolderPath("Desktop")
    $shortcutPath = Join-Path $desktopPath "Pixel Pulse.lnk"
    $exePath = Join-Path $InstallPath "PixelPulse.exe"
    
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $exePath
    $shortcut.WorkingDirectory = $InstallPath
    $shortcut.IconLocation = Join-Path $InstallPath "icon.ico"
    $shortcut.Description = "Pixel Pulse - Inspiration at Your Fingertips"
    $shortcut.Save()
    Write-Host "  Desktop shortcut created"
}

# Create Start Menu shortcut
if ($CreateStartMenuShortcut) {
    Write-Host "Creating Start Menu shortcut..."
    $startMenuPath = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs"
    $shortcutPath = Join-Path $startMenuPath "Pixel Pulse.lnk"
    $exePath = Join-Path $InstallPath "PixelPulse.exe"
    
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $exePath
    $shortcut.WorkingDirectory = $InstallPath
    $shortcut.IconLocation = Join-Path $InstallPath "icon.ico"
    $shortcut.Description = "Pixel Pulse - Inspiration at Your Fingertips"
    $shortcut.Save()
    Write-Host "  Start Menu shortcut created"
}

# Set up auto-start if requested
if ($AutoStart) {
    Write-Host "Setting up auto-start..."
    $regPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
    $exePath = Join-Path $InstallPath "PixelPulse.exe"
    Set-ItemProperty -Path $regPath -Name "PixelPulse" -Value $exePath -Force
    Write-Host "  Auto-start enabled"
}

# Add to uninstall registry
Write-Host "Adding to uninstall registry..."
$uninstallPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PixelPulse"
$exePath = Join-Path $InstallPath "PixelPulse.exe"
$uninstallScript = Join-Path $InstallPath "uninstall.ps1"

# Create uninstall script
$uninstallScriptContent = @'
# Pixel Pulse Uninstaller
param()

Write-Host "Uninstalling Pixel Pulse..."

# Remove shortcuts
$desktopPath = [Environment]::GetFolderPath("Desktop")
$desktopShortcut = Join-Path $desktopPath "Pixel Pulse.lnk"
if (Test-Path $desktopShortcut) {
    Remove-Item $desktopShortcut -Force
    Write-Host "Removed desktop shortcut"
}

$startMenuPath = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs"
$startMenuShortcut = Join-Path $startMenuPath "Pixel Pulse.lnk"
if (Test-Path $startMenuShortcut) {
    Remove-Item $startMenuShortcut -Force
    Write-Host "Removed Start Menu shortcut"
}

# Remove auto-start
$regPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
if (Get-ItemProperty $regPath -Name "PixelPulse" -ErrorAction SilentlyContinue) {
    Remove-ItemProperty -Path $regPath -Name "PixelPulse" -Force
    Write-Host "Removed auto-start"
}

# Remove application directory
$appPath = Split-Path -Parent $PSCommandPath
if (Test-Path $appPath) {
    Remove-Item $appPath -Recurse -Force
    Write-Host "Removed application directory"
}

# Remove uninstall registry entry
$uninstallPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PixelPulse"
if (Test-Path $uninstallPath) {
    Remove-Item $uninstallPath -Recurse -Force
    Write-Host "Removed uninstall registry entry"
}

Write-Host "Pixel Pulse uninstalled successfully!"
'@

Set-Content -Path $uninstallScript -Value $uninstallScriptContent -Force

New-Item -Path $uninstallPath -Force | Out-Null
Set-ItemProperty -Path $uninstallPath -Name "DisplayName" -Value "Pixel Pulse" -Force
Set-ItemProperty -Path $uninstallPath -Name "DisplayVersion" -Value "1.0.0.0" -Force
Set-ItemProperty -Path $uninstallPath -Name "Publisher" -Value "Pixel Tech Solutions" -Force
Set-ItemProperty -Path $uninstallPath -Name "InstallLocation" -Value $InstallPath -Force
Set-ItemProperty -Path $uninstallPath -Name "UninstallString" -Value "powershell.exe -ExecutionPolicy Bypass -File `"$uninstallScript`"" -Force
Set-ItemProperty -Path $uninstallPath -Name "DisplayIcon" -Value $exePath -Force

Write-Host ""
Write-Host "========================================"
Write-Host "Installation completed successfully!"
Write-Host "========================================"
Write-Host ""
Write-Host "Installation directory: $InstallPath"
Write-Host "To uninstall, use 'Add/Remove Programs' in Control Panel"
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
