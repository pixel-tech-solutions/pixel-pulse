# Pixel Pulse Professional Installer
# Version 1.0.0.0
# Author: Pixel Tech Solutions

param(
    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "$env:LOCALAPPDATA\PixelPulse",
    
    [Parameter(Mandatory=$false)]
    [switch]$Silent = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$DesktopShortcut = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$StartMenuShortcut = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$AutoStart = $false
)

# Add required assemblies
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# Display header
if (-not $Silent) {
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "    Pixel Pulse Installer v1.0.0.0" -ForegroundColor White
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Inspiration at Your Fingertips" -ForegroundColor Gray
    Write-Host "© 2026 Pixel Tech Solutions" -ForegroundColor Gray
    Write-Host ""
}

# Function to show progress bar
function Show-Progress {
    param($Activity, $PercentComplete)
    Write-Progress -Activity $Activity -Status "Please wait..." -PercentComplete $PercentComplete
}


# Create installation directory
Show-Progress "Creating installation directory..." 10
New-Item -ItemType Directory -Force -Path $InstallPath | Out-Null

# Copy application files
Show-Progress "Copying application files..." 30
$appFiles = @(
    @{Source="PixelPulse\bin\Debug\net8.0-windows\PixelPulse.exe"; Dest="PixelPulse.exe"},
    @{Source="PixelPulse\bin\Debug\net8.0-windows\PixelPulse.dll"; Dest="PixelPulse.dll"},
    @{Source="PixelPulse\Resources\icon.ico"; Dest="icon.ico"},
    @{Source="LICENSE.txt"; Dest="LICENSE.txt"},
    @{Source="README.md"; Dest="README.md"}
)

foreach ($file in $appFiles) {
    if (Test-Path $file.Source) {
        $destPath = Join-Path $InstallPath $file.Dest
        Copy-Item -Path $file.Source -Destination $destPath -Force
    } else {
        if (-not $Silent) {
            Write-Warning "File not found: $($file.Source)"
        }
    }
}

# Copy database if it exists
$dbSource = "$env:APPDATA\PixelPulse\quotes.db"
if (Test-Path $dbSource) {
    $dbDest = Join-Path $InstallPath "quotes.db"
    Copy-Item -Path $dbSource -Destination $dbDest -Force
}

# Create desktop shortcut
if ($DesktopShortcut) {
    Show-Progress "Creating desktop shortcut..." 50
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
}

# Create Start Menu shortcut
if ($StartMenuShortcut) {
    Show-Progress "Creating Start Menu shortcut..." 60
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
}

# Set up auto-start if requested
if ($AutoStart) {
    Show-Progress "Setting up auto-start..." 70
    $regPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
    $exePath = Join-Path $InstallPath "PixelPulse.exe"
    Set-ItemProperty -Path $regPath -Name "PixelPulse" -Value $exePath -Force
}

# Create uninstaller
Show-Progress "Creating uninstaller..." 80
$uninstallScript = Join-Path $InstallPath "uninstall.ps1"
$uninstallScriptContent = @'
# Pixel Pulse Uninstaller
param([switch]$Silent = $false)

if (-not $Silent) {
    Write-Host "Uninstalling Pixel Pulse..." -ForegroundColor Yellow
}

# Remove shortcuts
$desktopPath = [Environment]::GetFolderPath("Desktop")
$desktopShortcut = Join-Path $desktopPath "Pixel Pulse.lnk"
if (Test-Path $desktopShortcut) {
    Remove-Item $desktopShortcut -Force
    if (-not $Silent) { Write-Host "Removed desktop shortcut" -ForegroundColor Green }
}

$startMenuPath = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs"
$startMenuShortcut = Join-Path $startMenuPath "Pixel Pulse.lnk"
if (Test-Path $startMenuShortcut) {
    Remove-Item $startMenuShortcut -Force
    if (-not $Silent) { Write-Host "Removed Start Menu shortcut" -ForegroundColor Green }
}

# Remove auto-start
$regPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
if (Get-ItemProperty $regPath -Name "PixelPulse" -ErrorAction SilentlyContinue) {
    Remove-ItemProperty -Path $regPath -Name "PixelPulse" -Force
    if (-not $Silent) { Write-Host "Removed auto-start" -ForegroundColor Green }
}

# Remove application directory
$appPath = Split-Path -Parent $PSCommandPath
if (Test-Path $appPath) {
    Remove-Item $appPath -Recurse -Force
    if (-not $Silent) { Write-Host "Removed application directory" -ForegroundColor Green }
}

# Remove uninstall registry entry
$uninstallPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PixelPulse"
if (Test-Path $uninstallPath) {
    Remove-Item $uninstallPath -Recurse -Force
    if (-not $Silent) { Write-Host "Removed uninstall registry entry" -ForegroundColor Green }
}

if (-not $Silent) {
    Write-Host "Pixel Pulse uninstalled successfully!" -ForegroundColor Green
    Start-Sleep -Seconds 2
}
'@

Set-Content -Path $uninstallScript -Value $uninstallScriptContent -Force

# Add to uninstall registry
Show-Progress "Registering uninstaller..." 90
$uninstallRegPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PixelPulse"
$exePath = Join-Path $InstallPath "PixelPulse.exe"

New-Item -Path $uninstallRegPath -Force | Out-Null
Set-ItemProperty -Path $uninstallRegPath -Name "DisplayName" -Value "Pixel Pulse" -Force
Set-ItemProperty -Path $uninstallRegPath -Name "DisplayVersion" -Value "1.0.0.0" -Force
Set-ItemProperty -Path $uninstallRegPath -Name "Publisher" -Value "Pixel Tech Solutions" -Force
Set-ItemProperty -Path $uninstallRegPath -Name "InstallLocation" -Value $InstallPath -Force
Set-ItemProperty -Path $uninstallRegPath -Name "UninstallString" -Value "powershell.exe -ExecutionPolicy Bypass -File `"$uninstallScript`"" -Force
Set-ItemProperty -Path $uninstallRegPath -Name "QuietUninstallString" -Value "powershell.exe -ExecutionPolicy Bypass -File `"$uninstallScript`" -Silent" -Force
Set-ItemProperty -Path $uninstallRegPath -Name "DisplayIcon" -Value $exePath -Force
Set-ItemProperty -Path $uninstallRegPath -Name "URLInfoAbout" -Value "https://www.pixeltechsolutions.com" -Force

# Complete installation
Show-Progress "Finalizing installation..." 100
Start-Sleep -Milliseconds 500

if (-not $Silent) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  Installation completed successfully!" -ForegroundColor White
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Installation directory: $InstallPath" -ForegroundColor Cyan
    Write-Host "To uninstall, use 'Add/Remove Programs' in Control Panel" -ForegroundColor Gray
    Write-Host ""
    
    # Ask if user wants to launch the application
    $result = [System.Windows.Forms.MessageBox]::Show(
        "Would you like to launch Pixel Pulse now?", 
        "Installation Complete", 
        "YesNo", 
        "Question"
    )
    
    if ($result -eq "Yes") {
        Start-Process -FilePath (Join-Path $InstallPath "PixelPulse.exe")
    }
}
