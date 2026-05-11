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
; Updated for larger application size with new features
UsedUserAreasWarning=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Types]
Name: "full"; Description: "Full installation"
Name: "custom"; Description: "Custom installation"
Name: "compact"; Description: "Compact installation (KJV only)"

[Components]
Name: "kjv"; Description: "King James Version (KJV) - Traditional"; Types: full custom compact
Name: "asv"; Description: "American Standard Version (ASV) - Modern formal"; Types: full custom
Name: "web"; Description: "World English Bible (WEB) - Contemporary"; Types: full custom
Name: "ylt"; Description: "Young's Literal Translation (YLT) - Word-for-word"; Types: full custom
Name: "bbe"; Description: "Bible in Basic English (BBE) - Simplified"; Types: full custom
Name: "darby"; Description: "Darby English Bible - Study focused"; Types: full custom

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"
Name: "quicklaunchicon"; Description: "Create a &Quick Launch shortcut"; GroupDescription: "Additional icons:"
Name: "autostart"; Description: "&Start with Windows"; GroupDescription: "Startup options:"

[Files]
; Self-contained application executable (includes .NET 8.0 runtime)
Source: "publish\PixelPulse.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "PixelPulse\Resources\icon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion

; Database files will be created on first run or downloaded as needed
; Include quotes database if it exists
Source: "PixelPulse\Resources\quotes.db"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

; Include Bible version databases if they exist (for selective installation)
Source: "PixelPulse\Resources\bible_kjv.db"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist; Components: kjv
Source: "PixelPulse\Resources\bible_asv.db"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist; Components: asv
Source: "PixelPulse\Resources\bible_web.db"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist; Components: web
Source: "PixelPulse\Resources\bible_ylt.db"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist; Components: ylt
Source: "PixelPulse\Resources\bible_bbe.db"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist; Components: bbe
Source: "PixelPulse\Resources\bible_darby.db"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist; Components: darby

[Icons]
Name: "{group}\Pixel Pulse"; Filename: "{app}\PixelPulse.exe"; WorkingDir: "{app}"; IconFilename: "{app}\icon.ico"; Comment: "Pixel Pulse - Inspiration at Your Fingertips"
Name: "{group}\Uninstall Pixel Pulse"; Filename: "{uninstallexe}"; Comment: "Uninstall Pixel Pulse"
Name: "{userdesktop}\Pixel Pulse"; Filename: "{app}\PixelPulse.exe"; WorkingDir: "{app}"; IconFilename: "{app}\icon.ico"; Tasks: desktopicon; Comment: "Pixel Pulse - Inspiration at Your Fingertips"
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
  // Skip the "Select Components" page for non-custom installations
  else if PageID = wpSelectComponents then
    Result := not WizardSetupType('custom')
  else
    Result := False;
end;
