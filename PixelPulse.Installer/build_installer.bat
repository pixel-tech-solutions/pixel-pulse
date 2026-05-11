@echo off
echo ========================================
echo Building Pixel Pulse Professional Installer
echo ========================================
echo.

REM Check for WiX Toolset
set WIX_PATH=C:\Program Files (x86)\WiX Toolset v3.14\bin
if not exist "%WIX_PATH%" (
    set WIX_PATH=C:\Program Files\WiX Toolset v3.14\bin
)
if not exist "%WIX_PATH%" (
    echo ERROR: WiX Toolset not found. Please install WiX Toolset v3.14+
    echo Download from: https://wixtoolset.org/releases/
    pause
    exit /b 1
)

echo Found WiX Toolset at: %WIX_PATH%

REM Set paths
set PROJECT_ROOT=%~dp0
set SOURCE_DIR=%PROJECT_ROOT%\PixelPulse\bin\Release\net8.0-windows
set INSTALLER_DIR=%PROJECT_ROOT%\PixelPulse.Installer
set OUTPUT_DIR=%PROJECT_ROOT%\Output

echo Project Root: %PROJECT_ROOT%
echo Source Directory: %SOURCE_DIR%
echo Installer Directory: %INSTALLER_DIR%
echo Output Directory: %OUTPUT_DIR%

REM Check if source exists
if not exist "%SOURCE_DIR%\PixelPulse.exe" (
    echo ERROR: PixelPulse.exe not found. Please build the application first.
    echo Run: .\scripts\Build-Quick.ps1
    pause
    exit /b 1
)

REM Create output directory
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

echo.
echo Compiling installer...
echo.

REM Compile WiX installer
"%WIX_PATH%\candle.exe" ^
    "%INSTALLER_DIR%\ProfessionalInstaller.wxs" ^
    -ext WixUIExtension ^
    -ext WixNetFxExtension ^
    -out "%OUTPUT_DIR%\installer.wixobj"

if %ERRORLEVEL% neq 0 (
    echo ERROR: Candle (compiler) failed with exit code %ERRORLEVEL%
    pause
    exit /b 1
)

echo.
echo Linking installer...
echo.

"%WIX_PATH%\light.exe" ^
    "%OUTPUT_DIR%\installer.wixobj" ^
    -ext WixUIExtension ^
    -ext WixNetFxExtension ^
    -out "%OUTPUT_DIR%\PixelPulse-Setup.exe"

if %ERRORLEVEL% neq 0 (
    echo ERROR: Light (linker) failed with exit code %ERRORLEVEL%
    pause
    exit /b 1
)

echo.
echo Cleaning up temporary files...
del "%OUTPUT_DIR%\installer.wixobj"

echo.
echo ========================================
echo BUILD COMPLETE
echo ========================================
echo.
echo Installer created: %OUTPUT_DIR%\PixelPulse-Setup.exe
echo.

REM Show file size
for %%I in ("%OUTPUT_DIR%\PixelPulse-Setup.exe") do (
    set SIZE=%%~zI
    set /a SIZE_MB=!SIZE!/1024/1024
)
echo Installer size: !SIZE_MB! MB

REM Test if installer was created
if exist "%OUTPUT_DIR%\PixelPulse-Setup.exe" (
    echo ✓ Professional installer created successfully!
    echo.
    echo Next steps:
    echo 1. Test installer on clean machine
    echo 2. Test with/without .NET Runtime
    echo 3. Test automatic quote downloading
    echo 4. Create distribution package
    echo.
    echo The installer includes:
    echo - Automatic .NET Runtime installation
    echo - Automatic quote downloading
    echo - Professional first-time setup
    echo - Complete offline operation
) else (
    echo ✗ Installer creation failed
    pause
    exit /b 1
)

echo.
pause
