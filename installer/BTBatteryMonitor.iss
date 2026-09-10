; Inno Setup Script for BTBatteryMonitor
; Creator: DT-AIRA

#define MyAppName "BTBatteryMonitor"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "DT-AIRA"
#define MyAppURL "https://github.com/DT-AIRA/BTBatteryMonitor"
#define MyAppExeName "BTBatteryMonitor.exe"

[Setup]
AppId={{D6BC6641-83C3-452B-BB9B-A878C7981049}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; 一般ユーザー権限（管理者不要）でインストール
PrivilegesRequired=lowest
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes

LicenseFile=..\LICENSE
OutputDir=..\dist
OutputBaseFilename=BTBatteryMonitor_Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

; 既存プロセスの自動終了・警告
CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "startup"; Description: "Launch at Windows startup (Windows起動時に自動起動)"; GroupDescription: "Additional options:"
Name: "desktopicon"; Description: "Create a desktop shortcut (デスクトップにショートカットを作成)"; GroupDescription: "Additional options:"; Flags: unchecked

[Files]
Source: "..\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
