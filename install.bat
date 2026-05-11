@echo off
setlocal enabledelayedexpansion

echo ========================================
echo Pixel Pulse Installer
echo ========================================
echo.

set INSTALL_DIR=%LOCALAPPDATA%\PixelPulse
echo Creating installation directory...
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
echo Installation directory: %INSTALL_DIR%

echo.
echo Copying application files...
if exist "PixelPulse\bin\Debug\net8.0-windows\PixelPulse.exe" (
    copy "PixelPulse\bin\Debug\net8.0-windows\PixelPulse.exe" "%INSTALL_DIR%\" >nul
    echo   Copied: PixelPulse.exe
) else (
    echo   Warning: PixelPulse.exe not found
)

if exist "PixelPulse\bin\Debug\net8.0-windows\PixelPulse.dll" (
    copy "PixelPulse\bin\Debug\net8.0-windows\PixelPulse.dll" "%INSTALL_DIR%\" >nul
    echo   Copied: PixelPulse.dll
) else (
    echo   Warning: PixelPulse.dll not found
)

if exist "PixelPulse\Resources\icon.ico" (
    copy "PixelPulse\Resources\icon.ico" "%INSTALL_DIR%\" >nul
    echo   Copied: icon.ico
) else (
    echo   Warning: icon.ico not found
)

echo.
echo Creating Start Menu shortcut...
set START_MENU=%APPDATA%\Microsoft\Windows\Start Menu\Programs
echo Set WshShell = WScript.CreateObject("WScript.Shell") > "%TEMP%\create_shortcut.vbs"
echo Set oShortcut = WshShell.CreateShortcut("%START_MENU%\Pixel Pulse.lnk") >> "%TEMP%\create_shortcut.vbs"
echo oShortcut.TargetPath = "%INSTALL_DIR%\PixelPulse.exe" >> "%TEMP%\create_shortcut.vbs"
echo oShortcut.WorkingDirectory = "%INSTALL_DIR%" >> "%TEMP%\create_shortcut.vbs"
echo oShortcut.IconLocation = "%INSTALL_DIR%\icon.ico" >> "%TEMP%\create_shortcut.vbs"
echo oShortcut.Description = "Pixel Pulse - Inspiration at Your Fingertips" >> "%TEMP%\create_shortcut.vbs"
echo oShortcut.Save >> "%TEMP%\create_shortcut.vbs"
cscript //nologo "%TEMP%\create_shortcut.vbs"
del "%TEMP%\create_shortcut.vbs"

echo.
echo ========================================
echo Installation completed successfully!
echo ========================================
echo.
echo Installation directory: %INSTALL_DIR%
echo.
pause
