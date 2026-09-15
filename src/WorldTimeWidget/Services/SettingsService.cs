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
                // Настоящий первый запуск (нет файла вообще) — тема Windows должна взять свой
                // дизайн-дефолт прозрачности (78%), а не унаследованный из старого поля дефолт.
                return new AppSettings();
            }

            var json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            MigrateLegacyOpacity(settings);
            return settings;
        }
        catch
        {
            // Повреждённый/недоступный файл настроек не должен ронять запуск приложения —
            // откатываемся на настройки по умолчанию.
            return new AppSettings();
        }
    }

    /// <summary>
    /// До v1.3 прозрачность карточки хранилась одним общим числом (<c>AppSettings.BackgroundOpacityPercent</c>)
    /// вместо per-theme словаря. Для settings.json, сохранённых до этого обновления, переносим
    /// значение в запись темы "windows" (см. <c>ThemeCatalogService.DefaultThemeId</c>), чтобы у
    /// уже установленных пользователей внешний вид не менялся молча на новый дизайн-дефолт темы
    /// Windows (78%) — только настоящие первые запуски (без settings.json вообще, см. <see cref="Load"/>)
    /// получают дизайн-дефолт.
    /// </summary>
    private static void MigrateLegacyOpacity(AppSettings settings)
    {
        if (!settings.BackgroundOpacityByTheme.ContainsKey(ThemeCatalogService.DefaultThemeId))
        {
            settings.BackgroundOpacityByTheme[ThemeCatalogService.DefaultThemeId] = settings.BackgroundOpacityPercent;
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
