using Microsoft.Win32;

namespace WorldTimeWidget.Services;

/// <summary>
/// Управление автозапуском при входе в Windows через
/// <c>HKCU\Software\Microsoft\Windows\CurrentVersion\Run</c> (per-user, без прав администратора).
/// </summary>
public sealed class AutostartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "WorldTimeWidget";

    /// <summary>Текущее состояние ключа автозапуска в реестре.</summary>
    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        return key?.GetValue(ValueName) is not null;
    }

    public void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
                         ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);

        if (enabled)
        {
            var exePath = GetExecutablePath();
            key.SetValue(ValueName, $"\"{exePath}\"", RegistryValueKind.String);
        }
        else
        {
            if (key.GetValue(ValueName) is not null)
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }
        }
    }

    private static string GetExecutablePath()
    {
        // Environment.ProcessPath корректно отражает путь к opубликованному
        // self-contained single-file exe (в отличие от Assembly.Location,
        // которая для single-file публикации указывает на временную распаковку).
        return Environment.ProcessPath
               ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
               ?? throw new InvalidOperationException("Не удалось определить путь к исполняемому файлу.");
    }
}
