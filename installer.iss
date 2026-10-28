; Inno Setup script: packs dist\ into a single UxPlayTray-Setup.exe.
[Setup]
AppName=UxPlay Tray
AppVersion=1.0
AppPublisher=UxPlay Tray (UxPlay by FDH2, GPLv3)
DefaultDirName={autopf}\UxPlayTray
DefaultGroupName=UxPlay Tray
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputBaseFilename=UxPlayTray-Setup
Compression=lzma2/ultra64
SolidCompression=yes
LicenseFile=dist\uxplay\LICENSE-UxPlay.txt
UninstallDisplayIcon={app}\UxPlayTray.exe

[Languages]
Name: "fr"; MessagesFile: "compiler:Languages\French.isl"

[Files]
Source: "dist\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\UxPlay Tray"; Filename: "{app}\UxPlayTray.exe"

[Run]
; Without this, iPhones do not see the PC (Windows blocks the inbound mDNS/AirPlay traffic). Private networks only.
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""UxPlay"" dir=in action=allow program=""{app}\uxplay\bin\uxplay.exe"" profile=private enable=yes"; Flags: runhidden
Filename: "{app}\UxPlayTray.exe"; Description: "Lancer UxPlay Tray"; Flags: postinstall nowait skipifsilent runasoriginaluser

[UninstallRun]
Filename: "{sys}\taskkill.exe"; Parameters: "/f /im UxPlayTray.exe"; Flags: runhidden; RunOnceId: "KillTray"
Filename: "{sys}\taskkill.exe"; Parameters: "/f /im uxplay.exe"; Flags: runhidden; RunOnceId: "KillUxPlay"
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""UxPlay"""; Flags: runhidden; RunOnceId: "DelFirewall"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "UxPlayTray"; Flags: dontcreatekey uninsdeletevalue
Root: HKCU; Subkey: "Software\UxPlayTray"; Flags: dontcreatekey uninsdeletekey
