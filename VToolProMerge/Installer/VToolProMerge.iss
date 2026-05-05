; ====================================================================
;  VToolPro Merge — Inno Setup Script
;  Đóng gói WPF .NET 8 thành 1 file installer .exe chạy offline trên Windows.
;
;  Yêu cầu:
;    1) Cài Inno Setup 6+ (https://jrsoftware.org/isinfo.php).
;    2) Trước khi build installer, publish self-contained:
;         dotnet publish ..\VToolProMerge.csproj -c Release -r win-x64 ^
;             --self-contained true /p:PublishSingleFile=true ^
;             /p:IncludeNativeLibrariesForSelfExtract=true ^
;             -o .\publish
;    3) Mở file .iss này bằng Inno Setup -> Build -> Compile.
;
;  Output: Output\VToolProMergeSetup-x.y.z.exe
; ====================================================================

#define AppName       "VToolPro Merge"
#define AppShortName  "VToolProMerge"
#define AppVersion    "2.1.0.0"
#define AppPublisher  "VToolPro"
#define AppExeName    "VToolProMerge.exe"

[Setup]
AppId={{C0F4D2C8-4EE9-4F5F-9A1A-1A1A1A1A1A1A}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://example.local/
AppSupportURL=https://example.local/support
DefaultDirName={autopf}\{#AppShortName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableDirPage=auto
OutputDir=Output
OutputBaseFilename=VToolProMergeSetup-{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
UninstallDisplayIcon={app}\{#AppExeName}
SetupLogging=yes
ChangesAssociations=no

[Languages]
Name: "vietnamese"; MessagesFile: "compiler:Default.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Tạo shortcut trên Desktop"; GroupDescription: "Tùy chọn:"; Flags: unchecked
Name: "startmenuicon"; Description: "Tạo shortcut trong Start Menu"; GroupDescription: "Tùy chọn:";

[Files]
; Toàn bộ output của `dotnet publish` (self-contained)
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion
; Dữ liệu seed
Source: "publish\Data\*"; DestDir: "{app}\Data"; Flags: recursesubdirs createallsubdirs ignoreversion skipifsourcedoesntexist
; README để user xem
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: startmenuicon
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"; Tasks: startmenuicon
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Khởi chạy {#AppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\Data\Cache"
