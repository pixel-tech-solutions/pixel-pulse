@echo off
echo Creating Pixel Pulse Installer EXE...
echo.

REM Create a simple self-extracting installer using PowerShell
powershell -Command "& {
    # Read the PowerShell installer script
    $installerScript = Get-Content 'installer.ps1' -Raw
    
    # Create a self-contained PowerShell script that will be converted to exe
    $selfContainedScript = @'
# Pixel Pulse Self-Contained Installer
# This script will be packaged as an executable

# Extract embedded files and run installation
$installerPath = $PSScriptRoot
$tempPath = Join-Path $env:TEMP 'PixelPulse_Installer'
New-Item -ItemType Directory -Force -Path $tempPath | Out-Null

# Extract files (in a real scenario, these would be embedded resources)
Copy-Item -Path '$installerPath\PixelPulse\bin\Debug\net8.0-windows\*' -Destination $tempPath -Recurse -Force
Copy-Item -Path '$installerPath\PixelPulse\Resources\*' -Destination $tempPath -Force

# Run the actual installer
& '$tempPath\install.ps1' -InstallPath '$env:LOCALAPPDATA\PixelPulse'

# Cleanup
Start-Sleep -Seconds 5
Remove-Item $tempPath -Recurse -Force
'@
    
    # Save the self-contained script
    Set-Content -Path 'PixelPulse_Installer.ps1' -Value $selfContainedScript -Force
    
    Write-Host 'Self-contained installer script created: PixelPulse_Installer.ps1'
    Write-Host 'To create an EXE, use a PowerShell to EXE converter like:'
    Write-Host '1. PS2EXE (https://github.com/MScholtes/PS2EXE)'
    Write-Host '2. PowerShell App Deployment Toolkit'
    Write-Host '3. Convert-PS2EXE module'
}"

echo.
echo Self-contained installer script created: PixelPulse_Installer.ps1
echo.
echo To create an EXE file, you can use one of these methods:
echo.
echo Method 1 - Using PS2EXE (recommended):
echo   1. Install PS2EXE: Install-Module -Name PS2EXE
echo   2. Convert: Convert-PS2EXE -InputFile 'PixelPulse_Installer.ps1' -OutputFile 'PixelPulseInstaller.exe'
echo.
echo Method 2 - Online converter:
echo   Upload PixelPulse_Installer.ps1 to https://ps2exe.com/
echo.
echo Method 3 - PowerShell App Deployment Toolkit:
echo   Use PSAppDeployToolkit to package the application
echo.
pause
