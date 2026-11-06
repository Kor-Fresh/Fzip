[Setup]
AppId={{753B5A78-3EB8-41CC-9C9D-2A3A6D66F102}
AppName=Fzip
AppVersion=0.1.1
AppPublisher=KorFresh
DefaultDirName={localappdata}\Programs\Fzip
DefaultGroupName=Fzip
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\artifacts\installer
OutputBaseFilename=Fzip-0.1.1-Setup-x64
SetupIconFile=..\design\branding\fzip-app-v1.ico
UninstallDisplayIcon={app}\Fzip.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ChangesAssociations=yes
CloseApplications=yes

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "바탕화면 바로가기 만들기"; Flags: unchecked
Name: "contextmenu"; Description: "탐색기 우클릭 메뉴에 Fzip 추가 (Windows 11: 더 많은 옵션 표시)"
Name: "modernmenu"; Description: "Windows 11 우클릭 메뉴에 Fzip 추가"; Check: IsWindows11

[Files]
Source: "..\artifacts\app\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Fzip"; Filename: "{app}\Fzip.exe"
Name: "{autodesktop}\Fzip"; Filename: "{app}\Fzip.exe"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Fzip"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Fzip\Capabilities"; ValueType: string; ValueName: "ApplicationName"; ValueData: "Fzip"
Root: HKCU; Subkey: "Software\Fzip\Capabilities"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "무료 압축 · 압축해제 프로그램"
Root: HKCU; Subkey: "Software\RegisteredApplications"; ValueType: string; ValueName: "Fzip"; ValueData: "Software\Fzip\Capabilities"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\Fzip.zip"; ValueType: string; ValueData: "Fzip ZIP 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.zip\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.zip\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.zip\OpenWithProgids"; ValueType: string; ValueName: "Fzip.zip"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".zip"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".zip"; ValueData: "Fzip.zip"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.zip\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.zip\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.zip\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Fzip.7z"; ValueType: string; ValueData: "Fzip 7Z 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.7z\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.7z\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.7z\OpenWithProgids"; ValueType: string; ValueName: "Fzip.7z"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".7z"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".7z"; ValueData: "Fzip.7z"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.7z\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.7z\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.7z\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Fzip.rar"; ValueType: string; ValueData: "Fzip RAR 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.rar\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.rar\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.rar\OpenWithProgids"; ValueType: string; ValueName: "Fzip.rar"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".rar"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".rar"; ValueData: "Fzip.rar"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.rar\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.rar\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.rar\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Fzip.arj"; ValueType: string; ValueData: "Fzip ARJ 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.arj\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.arj\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.arj\OpenWithProgids"; ValueType: string; ValueName: "Fzip.arj"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".arj"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".arj"; ValueData: "Fzip.arj"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.arj\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.arj\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.arj\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Fzip.alz"; ValueType: string; ValueData: "Fzip ALZ 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.alz\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.alz\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.alz\OpenWithProgids"; ValueType: string; ValueName: "Fzip.alz"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".alz"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".alz"; ValueData: "Fzip.alz"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.alz\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.alz\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.alz\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Fzip.egg"; ValueType: string; ValueData: "Fzip EGG 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.egg\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.egg\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.egg\OpenWithProgids"; ValueType: string; ValueName: "Fzip.egg"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".egg"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".egg"; ValueData: "Fzip.egg"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.egg\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.egg\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.egg\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Fzip.cab"; ValueType: string; ValueData: "Fzip CAB 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.cab\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.cab\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.cab\OpenWithProgids"; ValueType: string; ValueName: "Fzip.cab"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".cab"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".cab"; ValueData: "Fzip.cab"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.cab\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.cab\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.cab\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Fzip.lzh"; ValueType: string; ValueData: "Fzip LZH 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.lzh\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.lzh\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.lzh\OpenWithProgids"; ValueType: string; ValueName: "Fzip.lzh"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".lzh"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".lzh"; ValueData: "Fzip.lzh"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.lzh\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.lzh\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.lzh\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Fzip.tar"; ValueType: string; ValueData: "Fzip TAR 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.tar\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.tar\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.tar\OpenWithProgids"; ValueType: string; ValueName: "Fzip.tar"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".tar"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".tar"; ValueData: "Fzip.tar"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.tar\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.tar\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.tar\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Fzip.gz"; ValueType: string; ValueData: "Fzip GZ 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.gz\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.gz\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.gz\OpenWithProgids"; ValueType: string; ValueName: "Fzip.gz"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".gz"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".gz"; ValueData: "Fzip.gz"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.gz\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.gz\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.gz\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Fzip.bz2"; ValueType: string; ValueData: "Fzip BZ2 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.bz2\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.bz2\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.bz2\OpenWithProgids"; ValueType: string; ValueName: "Fzip.bz2"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".bz2"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".bz2"; ValueData: "Fzip.bz2"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.bz2\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.bz2\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.bz2\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Fzip.xz"; ValueType: string; ValueData: "Fzip XZ 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.xz\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.xz\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.xz\OpenWithProgids"; ValueType: string; ValueName: "Fzip.xz"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".xz"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".xz"; ValueData: "Fzip.xz"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.xz\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.xz\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.xz\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Fzip.001"; ValueType: string; ValueData: "Fzip 001 압축파일"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Fzip.001\DefaultIcon"; ValueType: string; ValueData: """{app}\assets\archive.ico"",0"
Root: HKCU; Subkey: "Software\Classes\Fzip.001\shell\open\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.001\OpenWithProgids"; ValueType: string; ValueName: "Fzip.001"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Fzip.exe\SupportedTypes"; ValueType: string; ValueName: ".001"; ValueData: ""
Root: HKCU; Subkey: "Software\Fzip\Capabilities\FileAssociations"; ValueType: string; ValueName: ".001"; ValueData: "Fzip.001"
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.001\shell\Fzip.Extract"; ValueType: string; ValueData: "Fzip으로 압축풀기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.001\shell\Fzip.Extract"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\SystemFileAssociations\.001\shell\Fzip.Extract\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --extract ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\*\shell\Fzip.Compress"; ValueType: string; ValueData: "Fzip으로 압축하기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\*\shell\Fzip.Compress"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\*\shell\Fzip.Compress"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\*\shell\Fzip.Compress\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --compress ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Directory\shell\Fzip.Compress"; ValueType: string; ValueData: "Fzip으로 압축하기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Directory\shell\Fzip.Compress"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Directory\shell\Fzip.Compress"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Directory\shell\Fzip.Compress\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --compress ""%1"""; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Directory\Background\shell\Fzip.Compress"; ValueType: string; ValueData: "Fzip으로 압축하기…"; Flags: uninsdeletekey; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Directory\Background\shell\Fzip.Compress"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\assets\archive.ico"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Directory\Background\shell\Fzip.Compress"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Single"; Tasks: contextmenu
Root: HKCU; Subkey: "Software\Classes\Directory\Background\shell\Fzip.Compress\command"; ValueType: string; ValueData: """{app}\Fzip.exe"" --compress "; Tasks: contextmenu

[Run]
Filename: "{app}\Fzip.exe"; Description: "Fzip 실행"; Flags: nowait postinstall skipifsilent
Filename: "ms-settings:defaultapps"; Description: "Windows에서 Fzip을 기본 앱으로 선택"; Flags: shellexec postinstall skipifsilent unchecked

[UninstallRun]
Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; Parameters: "-NoLogo -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File ""{app}\shell\Register-Shell.ps1"" -Remove"; Flags: runhidden waituntilterminated; RunOnceId: "FzipRemoveShell"

[Code]
function IsWindows11: Boolean;
var
  Version: TWindowsVersion;
begin
  GetWindowsVersionEx(Version);
  Result := (Version.Major >= 10) and (Version.Build >= 22000);
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  Params: String;
begin
  if CurStep = ssPostInstall then
  begin
    Params := '-NoLogo -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File "' + ExpandConstant('{app}\shell\Register-Shell.ps1') + '"';
    if not (IsWindows11 and WizardIsTaskSelected('modernmenu')) then
      Params := Params + ' -Remove';
    if not Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'), Params, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
      ResultCode := -1;
    if ResultCode <> 0 then
    begin
      Log('Fzip modern menu registration failed: ' + IntToStr(ResultCode));
      if not WizardSilent then
        MsgBox('Fzip 설치가 완료되었습니다.' + #13#10 + 'Windows 11 우클릭 메뉴를 추가하지 못했습니다. Fzip을 직접 실행하여 이용해 주세요.', mbInformation, MB_OK);
    end;
  end;
end;

