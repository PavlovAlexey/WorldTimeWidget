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
    /// Прозрачность фона основной карточки, % (диапазон 40-100). Управляет только альфа-каналом
    /// фона карточки (<c>RootBorder.Background</c>) — попапы и текст не затрагиваются.
    /// Дефолт 88 соответствует ранее зашитому значению <c>#E0F3F3F3</c> (альфа 0xE0 ≈ 88%),
    /// чтобы для существующих пользователей без settings.json внешний вид не изменился.
    /// </summary>
    public int BackgroundOpacityPercent { get; set; } = 88;
}
