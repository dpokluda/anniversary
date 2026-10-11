; The Windows installer (spec 10), compiled by Build-Package.ps1 with Inno Setup 6:
;   ISCC.exe /DAppVersion=5.0.0 /DRid=win-x64 /DPublishFolder=<publish\win-x64> /DOutputFolder=<packages> Anniversary.iss
; It installs for the current user only, without admin rights, and never touches the app-data folder.

#ifndef AppVersion
  #error Pass /DAppVersion=<version>
#endif
#ifndef Rid
  #error Pass /DRid=win-x64 or /DRid=win-arm64
#endif
#ifndef PublishFolder
  #error Pass /DPublishFolder=<the folder with Anniversary.exe>
#endif
#ifndef OutputFolder
  #define OutputFolder "."
#endif
#ifndef Copyright
  #define Copyright "David Pokluda"
#endif

#if Rid == "win-arm64"
  #define Architectures "arm64"
#else
  ; x64compatible also lets Windows on ARM install the x64 build.
  #define Architectures "x64compatible"
#endif

[Setup]
; One AppId for every version and architecture, so an installer updates the installed app in place.
AppId={{7773EC74-CEFF-475D-B70F-64346BA207FA}
AppName=Anniversary
AppVersion={#AppVersion}
AppPublisher=David Pokluda
AppPublisherURL=https://github.com/dpokluda/anniversary
AppSupportURL=https://github.com/dpokluda/anniversary/issues
AppCopyright={#Copyright}
VersionInfoVersion={#AppVersion}
PrivilegesRequired=lowest
DefaultDirName={autopf}\Anniversary
DisableProgramGroupPage=yes
ArchitecturesAllowed={#Architectures}
ArchitecturesInstallIn64BitMode={#Architectures}
OutputDir={#OutputFolder}
OutputBaseFilename=Anniversary-{#AppVersion}-{#Rid}-setup
SetupIconFile=..\..\Anniversary.Shared\Assets\App.ico
UninstallDisplayIcon={app}\Anniversary.exe
WizardStyle=modern
Compression=lzma2/max
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishFolder}\Anniversary.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishFolder}\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishFolder}\THIRD-PARTY-NOTICES.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Anniversary"; Filename: "{app}\Anniversary.exe"
Name: "{autodesktop}\Anniversary"; Filename: "{app}\Anniversary.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Anniversary.exe"; Description: "{cm:LaunchProgram,Anniversary}"; Flags: nowait postinstall skipifsilent

[Code]
const
  RunKey = 'Software\Microsoft\Windows\CurrentVersion\Run';
  ApprovedKey = 'Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run';

// Start at Login (spec 7.12) is a Run value the app writes itself; it goes with the app if it starts this copy.
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  Command: String;
begin
  if (CurUninstallStep = usUninstall)
    and RegQueryStringValue(HKCU, RunKey, 'Anniversary', Command)
    and (CompareText(Command, '"' + ExpandConstant('{app}\Anniversary.exe') + '"') = 0) then
  begin
    RegDeleteValue(HKCU, RunKey, 'Anniversary');
    RegDeleteValue(HKCU, ApprovedKey, 'Anniversary');
  end;
end;
