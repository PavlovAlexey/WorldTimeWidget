using System.IO;
using System.Text.Json;
using WorldTimeWidget.Models;

namespace WorldTimeWidget.Services;

/// <summary>
/// Загрузка и сохранение <see cref="AppSettings"/> в
/// <c>%AppData%\WorldTimeWidget\settings.json</c>.
/// </summary>
public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string _settingsDirectory;
    private readonly string _settingsFilePath;

    public SettingsService()
    {
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WorldTimeWidget");
        _settingsFilePath = Path.Combine(_settingsDirectory, "settings.json");
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return settings ?? new AppSettings();
        }
        catch
        {
            // Повреждённый/недоступный файл настроек не должен ронять запуск приложения —
            // откатываемся на настройки по умолчанию.
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(_settingsDirectory);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch
        {
            // Не удалось сохранить настройки (например, нет прав на запись) —
            // не критично для работы виджета в текущей сессии.
        }
    }
}
