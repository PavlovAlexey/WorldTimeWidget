namespace WorldTimeWidget.Models;

/// <summary>
/// Персистентные настройки приложения — сериализуются в
/// <c>%AppData%\WorldTimeWidget\settings.json</c>.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Добавленные пользователем часовые пояса (Windows <see cref="TimeZoneInfo.Id"/>)
    /// в порядке отображения. Домашний (системный) пояс сюда не входит —
    /// он всегда выводится первой строкой отдельно.
    /// </summary>
    public List<string> TimeZoneIds { get; set; } = new();

    /// <summary>true — 24-часовой формат времени, false — 12-часовой (AM/PM).</summary>
    public bool Is24HourFormat { get; set; } = true;

    /// <summary>Режим "поверх всех окон". По умолчанию включён.</summary>
    public bool IsAlwaysOnTop { get; set; } = true;

    /// <summary>Автозапуск при входе в Windows (ключ HKCU\...\Run). По умолчанию выключен.</summary>
    public bool IsAutostartEnabled { get; set; }

    /// <summary>Последняя позиция окна на экране (X). null — использовать позицию по умолчанию.</summary>
    public double? WindowLeft { get; set; }

    /// <summary>Последняя позиция окна на экране (Y). null — использовать позицию по умолчанию.</summary>
    public double? WindowTop { get; set; }

    /// <summary>
    /// УСТАРЕЛО (v1.1-v1.2): единая прозрачность карточки для всех тем, до введения per-theme
    /// хранения в v1.3 (см. <see cref="BackgroundOpacityByTheme"/>). Оставлено только для
    /// миграции — при загрузке настроек, сохранённых до v1.3, значение однократно переносится в
    /// <c>BackgroundOpacityByTheme["windows"]</c> (см. <c>SettingsService.Load</c>), чтобы
    /// прозрачность у существующих пользователей не "прыгала" на новый дизайн-дефолт темы Windows
    /// (78%). Новый код это поле больше не читает и не пишет.
    /// </summary>
    public int BackgroundOpacityPercent { get; set; } = 88;

    /// <summary>
    /// Id активной темы оформления (см. <c>ThemeCatalogService</c>). Дефолт — Windows (Fluent/Mica).
    /// </summary>
    public string ThemeId { get; set; } = "windows";

    /// <summary>
    /// Прозрачность фона основной карточки, % (диапазон 20-100) — отдельно на каждую тему (ключ —
    /// id темы), а не одним общим числом, как раньше (см. spec.md, «Доработка v1.3»). Управляет
    /// только альфа-каналом фона карточки — попапы (свои <see cref="ThemeDefinition"/>.PopupOpacityPercent
    /// у каждой темы) и текст не затрагиваются. При первом переключении на тему, для которой ещё
    /// нет сохранённого значения, используется её <c>ThemeDefinition.DefaultOpacityPercent</c>.
    /// </summary>
    public Dictionary<string, int> BackgroundOpacityByTheme { get; set; } = new();

    /// <summary>
    /// Прозрачность для кликов (WS_EX_TRANSPARENT на hwnd главного окна) — клики проваливаются
    /// сквозь виджет к тому, что под ним. По умолчанию выключено. Единственный штатный способ
    /// выключить обратно, если включено — трей (см. TrayIconService), так как при включённом
    /// click-through сам виджет не реагирует на мышь вообще.
    /// </summary>
    public bool IsClickThrough { get; set; }
}
