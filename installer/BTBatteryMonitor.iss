; Inno Setup Script for BTBatteryMonitor
; Creator: DT-AIRA

#define MyAppName "BTBatteryMonitor"
#define MyAppVersion "1.1.0"
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
Compression=zip
SolidCompression=no
WizardStyle=modern
SetupIconFile=..\src\BTBatteryMonitor\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

; 詳細なバージョン・メタデータ（信頼性向上・誤検知防止）
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=BTBatteryMonitor Windows Installer
VersionInfoCopyright=Copyright (C) 2026 DT-AIRA
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

; 既存プロセスの自動終了・警告
CloseApplications=yes
InfoAfterFile=FinishedInfo.txt

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut (デスクトップにショートカットを作成)"; GroupDescription: "Additional options:"; Flags: unchecked

[Files]
Source: "..\publish_files\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
