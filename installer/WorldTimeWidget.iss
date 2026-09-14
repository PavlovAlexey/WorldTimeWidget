; Inno Setup script for WorldTimeWidget.
; Собирает per-user инсталлятор (без прав администратора) из опубликованного
; self-contained single-file exe.
;
; Перед компиляцией сначала опубликуйте приложение (см. README.md в корне репозитория):
;   dotnet publish -c Release -r win-x64 --self-contained true ^
;       -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
; Результат ожидается в:
;   ..\src\WorldTimeWidget\bin\Release\net8.0-windows\win-x64\publish\WorldTimeWidget.exe
;
; Компиляция инсталлятора (из этой папки или через ISCC.exe):
;   iscc WorldTimeWidget.iss

#define MyAppName "WorldTimeWidget"
#define MyAppVersion "1.1.0"
#define MyAppPublisher "WorldTimeWidget"
#define MyAppExeName "WorldTimeWidget.exe"
#define MyPublishDir "..\src\WorldTimeWidget\bin\Release\net8.0-windows\win-x64\publish"

[Setup]
; Уникальный AppId для корректных апдейтов/деинсталляции между версиями.
AppId={{8C6C6C1E-6E6B-4E9B-9C1E-6E1E6E1E6E1E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
; Установка per-user, без запроса прав администратора.
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=Output
OutputBaseFilename=WorldTimeWidget-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
; У виджета нет собственного окна с рамкой/иконкой в трее — единственный способ
; закрыть его после установки предусмотрен в самом приложении (пункт меню "Выход").
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#MyPublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[Code]
const
  RunKeyPath = 'Software\Microsoft\Windows\CurrentVersion\Run';
  RunValueName = 'WorldTimeWidget';

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  AppDataSettingsDir: string;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // Приложение само создаёт этот ключ автозапуска при включении соответствующей
    // опции в меню — инсталлятор его не создавал, поэтому Inno не удалит его
    // автоматически. Чистим вручную, чтобы не оставлять "битую" ссылку на удалённый exe.
    RegDeleteValue(HKEY_CURRENT_USER, RunKeyPath, RunValueName);

    // Настройки приложения (%AppData%\WorldTimeWidget) — удаляем при деинсталляции,
    // чтобы не оставлять мусора после полного удаления программы.
    AppDataSettingsDir := ExpandConstant('{userappdata}\WorldTimeWidget');
    DelTree(AppDataSettingsDir, True, True, True);
  end;
end;
