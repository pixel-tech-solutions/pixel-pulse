[Setup]
AppName=Pixel Pulse
AppVersion=1.0.0.0
AppPublisher=Pixel Tech Solutions
AppPublisherURL=https://www.pixeltechsolutions.com
AppSupportURL=https://www.pixeltechsolutions.com/support
AppUpdatesURL=https://www.pixeltechsolutions.com/updates
AppCopyright=Copyright © 2026 Pixel Tech Solutions. All rights reserved.
DefaultDirName={localappdata}\PixelPulse
DefaultGroupName=Pixel Pulse
AllowNoIcons=yes
LicenseFile=LICENSE.txt
OutputDir=InstallerOutput
OutputBaseFilename=PixelPulseInstaller
SetupIconFile=PixelPulse\Resources\icon.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"
Name: "quicklaunchicon"; Description: "Create a &Quick Launch shortcut"; GroupDescription: "Additional icons:"
Name: "autostart"; Description: "&Start with Windows"; GroupDescription: "Startup options:"

[Files]
Source: "PixelPulse\bin\Debug\net8.0-windows\PixelPulse.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "PixelPulse\bin\Debug\net8.0-windows\PixelPulse.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "PixelPulse\Resources\icon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion

; Copy database if it exists in development location
Source: "%APPDATA%\PixelPulse\quotes.db"; DestDir: "{app}"; Flags: external skipifsourcedoesntexist ignoreversion

[Icons]
Name: "{group}\Pixel Pulse"; Filename: "{app}\PixelPulse.exe"; WorkingDir: "{app}"; IconFilename: "{app}\icon.ico"; Comment: "Pixel Pulse - Inspiration at Your Fingertips"
Name: "{group}\Uninstall Pixel Pulse"; Filename: "{uninstallexe}"; Comment: "Uninstall Pixel Pulse"
Name: "{commondesktop}\Pixel Pulse"; Filename: "{app}\PixelPulse.exe"; WorkingDir: "{app}"; IconFilename: "{app}\icon.ico"; Tasks: desktopicon; Comment: "Pixel Pulse - Inspiration at Your Fingertips"
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\Pixel Pulse"; Filename: "{app}\PixelPulse.exe"; WorkingDir: "{app}"; IconFilename: "{app}\icon.ico"; Tasks: quicklaunchicon; Comment: "Pixel Pulse"

[Run]
Filename: "{app}\PixelPulse.exe"; Description: "Launch Pixel Pulse"; Flags: nowait postinstall skipifsilent

[Registry]
; Auto-start registry entry
Root: HKCU; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "PixelPulse"; ValueData: "{app}\PixelPulse.exe"; Tasks: autostart; Flags: uninsdeletevalue

[UninstallDelete]
Type: filesandordirs; Name: "{app}\quotes.db"

[Code]
function GetUninstallString(): String;
var
  UninstallString: String;
begin
  UninstallString := ExpandConstant('{uninstallexe}');
  Result := UninstallString;
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  // Skip the "Select Start Menu Folder" page if we're not creating shortcuts
  if PageID = wpSelectProgramGroup then
    Result := not WizardIsTaskSelected('desktopicon') and not WizardIsTaskSelected('quicklaunchicon')
  else
    Result := False;
end;
