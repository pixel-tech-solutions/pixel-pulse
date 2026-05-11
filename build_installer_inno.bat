@echo off
echo Building Inno Setup installer...
"C:\Program Files (x86)\Inno Setup 6\iscc.exe" pixelPulseInstaller.iss
if %ERRORLEVEL% EQU 0 (
    echo Installer built successfully!
) else (
    echo Installer build failed with error code %ERRORLEVEL%
)
pause
