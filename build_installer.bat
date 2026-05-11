@echo off
echo Building Pixel Pulse installer with Inno Setup...
echo.

REM Check if Inno Setup is installed
if not exist "C:\Program Files (x86)\Inno Setup 6\iscc.exe" (
    echo ERROR: Inno Setup 6 is not installed!
    echo Please install Inno Setup 6 from: https://jrsoftware.org/isinfo.php
    echo.
    pause
    exit /b 1
)

REM Build the application first
echo Building application...
cd PixelPulse
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Application build failed!
    pause
    exit /b 1
)
cd ..

REM Build the installer
echo Building installer...
"C:\Program Files (x86)\Inno Setup 6\iscc.exe" PixelPulseInstaller.iss
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   Installer built successfully!
    echo ========================================
    echo Output: InstallerOutput\PixelPulseInstaller.exe
    echo.
) else (
    echo ERROR: Installer build failed with error code %ERRORLEVEL%
    pause
    exit /b 1
)
