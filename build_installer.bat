@echo off
set PATH=%PATH%;C:\Users\ssego\Documents\Pixel-Pulse\wix314-binaries\tools
cd PixelPulse.Installer
echo Building installer...
candle.exe Product.wxs -out obj\
light.exe obj\Product.wixobj -out bin\Release\PixelPulseInstaller.msi
echo Installer build complete!
pause
