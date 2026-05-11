# Pixel Pulse Installer - Ready for EXE Conversion
# Version: 1.0.0.0
# Author: Pixel Tech Solutions

param(
    [string]$InstallPath = "$env:LOCALAPPDATA\PixelPulse"
)

# Add required assemblies
Add-Type -AssemblyName System.Windows.Forms

# Display header
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "    Pixel Pulse Installer v1.0.0.0" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Inspiration at Your Fingertips" -ForegroundColor Gray
Write-Host "© 2026 Pixel Tech Solutions" -ForegroundColor Gray
Write-Host ""

# Installation options dialog
$form = New-Object System.Windows.Forms.Form
$form.Text = "Pixel Pulse Installer"
$form.Size = New-Object System.Drawing.Size(400, 300)
$form.StartPosition = "CenterScreen"
$form.FormBorderStyle = "FixedDialog"
$form.MaximizeBox = $false
$form.MinimizeBox = $false

# Desktop shortcut checkbox
$desktopCheck = New-Object System.Windows.Forms.CheckBox
$desktopCheck.Text = "Create desktop shortcut"
$desktopCheck.Location = New-Object System.Drawing.Point(20, 20)
$desktopCheck.Checked = $true
$form.Controls.Add($desktopCheck)

# Start Menu shortcut checkbox
$startMenuCheck = New-Object System.Windows.Forms.CheckBox
$startMenuCheck.Text = "Create Start Menu shortcut"
$startMenuCheck.Location = New-Object System.Drawing.Point(20, 50)
$startMenuCheck.Checked = $true
$form.Controls.Add($startMenuCheck)

# Auto-start checkbox
$autoStartCheck = New-Object System.Windows.Forms.CheckBox
$autoStartCheck.Text = "Start with Windows"
$autoStartCheck.Location = New-Object System.Drawing.Point(20, 80)
$form.Controls.Add($autoStartCheck)

# Install button
$installButton = New-Object System.Windows.Forms.Button
$installButton.Text = "Install"
$installButton.Location = New-Object System.Drawing.Point(20, 150)
$installButton.Size = New-Object System.Drawing.Size(100, 30)
$installButton.DialogResult = "OK"
$form.Controls.Add($installButton)

# Cancel button
$cancelButton = New-Object System.Windows.Forms.Button
$cancelButton.Text = "Cancel"
$cancelButton.Location = New-Object System.Drawing.Point(140, 150)
$cancelButton.Size = New-Object System.Drawing.Size(100, 30)
$cancelButton.DialogResult = "Cancel"
$form.Controls.Add($cancelButton)

# Show dialog
$result = $form.ShowDialog()

if ($result -eq "Cancel") {
    Write-Host "Installation cancelled by user." -ForegroundColor Yellow
    exit 0
}

# Get installation options
$createDesktop = $desktopCheck.Checked
$createStartMenu = $startMenuCheck.Checked
$autoStart = $autoStartCheck.Checked

# Create installation directory
Write-Host "Creating installation directory..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path $InstallPath | Out-Null
Write-Host "✓ Installation directory created" -ForegroundColor Green

# Copy application files
Write-Host "Copying application files..." -ForegroundColor Yellow
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
        Write-Host "✓ Copied: $($file.Dest)" -ForegroundColor Green
    } else {
        Write-Host "⚠ Warning: File not found: $($file.Source)" -ForegroundColor Yellow
    }
}

# Copy database if it exists
$dbSource = "$env:APPDATA\PixelPulse\quotes.db"
if (Test-Path $dbSource) {
    $dbDest = Join-Path $InstallPath "quotes.db"
    Copy-Item -Path $dbSource -Destination $dbDest -Force
    Write-Host "✓ Copied: quotes.db" -ForegroundColor Green
}

# Create desktop shortcut
if ($createDesktop) {
    Write-Host "Creating desktop shortcut..." -ForegroundColor Yellow
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
    Write-Host "✓ Desktop shortcut created" -ForegroundColor Green
}

# Create Start Menu shortcut
if ($createStartMenu) {
    Write-Host "Creating Start Menu shortcut..." -ForegroundColor Yellow
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
    Write-Host "✓ Start Menu shortcut created" -ForegroundColor Green
}

# Set up auto-start if requested
if ($autoStart) {
    Write-Host "Setting up auto-start..." -ForegroundColor Yellow
    $regPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
    $exePath = Join-Path $InstallPath "PixelPulse.exe"
    Set-ItemProperty -Path $regPath -Name "PixelPulse" -Value $exePath -Force
    Write-Host "✓ Auto-start enabled" -ForegroundColor Green
}

# Create uninstaller
Write-Host "Creating uninstaller..." -ForegroundColor Yellow
$uninstallScript = Join-Path $InstallPath "uninstall.ps1"
$uninstallScriptContent = @'
# Pixel Pulse Uninstaller
param()

Write-Host "Uninstalling Pixel Pulse..." -ForegroundColor Yellow

# Remove shortcuts
$desktopPath = [Environment]::GetFolderPath("Desktop")
$desktopShortcut = Join-Path $desktopPath "Pixel Pulse.lnk"
if (Test-Path $desktopShortcut) {
    Remove-Item $desktopShortcut -Force
    Write-Host "✓ Removed desktop shortcut" -ForegroundColor Green
}

$startMenuPath = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs"
$startMenuShortcut = Join-Path $startMenuPath "Pixel Pulse.lnk"
if (Test-Path $startMenuShortcut) {
    Remove-Item $startMenuShortcut -Force
    Write-Host "✓ Removed Start Menu shortcut" -ForegroundColor Green
}

# Remove auto-start
$regPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
if (Get-ItemProperty $regPath -Name "PixelPulse" -ErrorAction SilentlyContinue) {
    Remove-ItemProperty -Path $regPath -Name "PixelPulse" -Force
    Write-Host "✓ Removed auto-start" -ForegroundColor Green
}

# Remove application directory
$appPath = Split-Path -Parent $PSCommandPath
if (Test-Path $appPath) {
    Remove-Item $appPath -Recurse -Force
    Write-Host "✓ Removed application directory" -ForegroundColor Green
}

# Remove uninstall registry entry
$uninstallPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PixelPulse"
if (Test-Path $uninstallPath) {
    Remove-Item $uninstallPath -Recurse -Force
    Write-Host "✓ Removed uninstall registry entry" -ForegroundColor Green
}

Write-Host "Pixel Pulse uninstalled successfully!" -ForegroundColor Green
Start-Sleep -Seconds 2
'@

Set-Content -Path $uninstallScript -Value $uninstallScriptContent -Force

# Add to uninstall registry
Write-Host "Registering uninstaller..." -ForegroundColor Yellow
$uninstallRegPath = "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PixelPulse"
$exePath = Join-Path $InstallPath "PixelPulse.exe"

New-Item -Path $uninstallRegPath -Force | Out-Null
Set-ItemProperty -Path $uninstallRegPath -Name "DisplayName" -Value "Pixel Pulse" -Force
Set-ItemProperty -Path $uninstallRegPath -Name "DisplayVersion" -Value "1.0.0.0" -Force
Set-ItemProperty -Path $uninstallRegPath -Name "Publisher" -Value "Pixel Tech Solutions" -Force
Set-ItemProperty -Path $uninstallRegPath -Name "InstallLocation" -Value $InstallPath -Force
Set-ItemProperty -Path $uninstallRegPath -Name "UninstallString" -Value "powershell.exe -ExecutionPolicy Bypass -File `"$uninstallScript`"" -Force
Set-ItemProperty -Path $uninstallRegPath -Name "DisplayIcon" -Value $exePath -Force

# Complete installation
Write-Host "" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Installation completed successfully!" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Green
Write-Host "" -ForegroundColor White
Write-Host "Installation directory: $InstallPath" -ForegroundColor Cyan
Write-Host "To uninstall, use 'Add/Remove Programs' in Control Panel" -ForegroundColor Gray
Write-Host "" -ForegroundColor White

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
