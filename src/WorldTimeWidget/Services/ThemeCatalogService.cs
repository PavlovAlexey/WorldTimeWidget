using System.Windows.Media;
using WorldTimeWidget.Models;

namespace WorldTimeWidget.Services;

/// <summary>
/// Каталог из 6 утверждённых в Penpot тем оформления (см. spec.md, «Доработка v1.3», «Токены тем»).
/// Порядок элементов <see cref="All"/> — порядок сваши в пикере темы (меню «⋯»), слева направо/сверху вниз.
/// </summary>
public static class ThemeCatalogService
{
    public const string DefaultThemeId = "windows";

    public static IReadOnlyList<ThemeDefinition> All { get; } = new List<ThemeDefinition>
    {
        // 1. Windows (Fluent / Mica) — тема по умолчанию, совпадает с реализацией до v1.3.
        new()
        {
            Id = "windows",
            DisplayName = "Windows (Fluent / Mica)",
            Surface = Color.FromRgb(0xF3, 0xF3, 0xF3),
            DefaultOpacityPercent = 78,
            PopupOpacityPercent = 94,
            Border = Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF), // #FFFFFF@60%
            BorderThickness = 1,
            CornerRadius = 8,
            HomeHighlight = Color.FromArgb(0x73, 0xFF, 0xFF, 0xFF), // #FFFFFF@45%
            TextPrimary = Color.FromRgb(0x1B, 0x1B, 0x1B),
            TextSecondary = Color.FromRgb(0x6B, 0x6B, 0x6B),
            Divider = Color.FromArgb(0x0F, 0x00, 0x00, 0x00), // #000000@6%
            ShadowColor = Colors.Black,
            ShadowBlurRadius = 24,
            ShadowOffsetY = 8,
            ShadowOpacity = 0.16,
        },

        // 2. Liquid Glass — сильная прозрачность + глянцевый блик сверху карточки.
        new()
        {
            Id = "liquid-glass",
            DisplayName = "Liquid Glass",
            Surface = Color.FromRgb(0xFF, 0xFF, 0xFF),
            DefaultOpacityPercent = 32,
            PopupOpacityPercent = 55,
            Border = Color.FromArgb(0xD9, 0xFF, 0xFF, 0xFF), // #FFFFFF@85%
            BorderThickness = 1.5,
            CornerRadius = 22,
            HomeHighlight = Color.FromArgb(0x59, 0xFF, 0xFF, 0xFF), // #FFFFFF@35%
            TextPrimary = Color.FromRgb(0x1C, 0x1C, 0x1E),
            TextSecondary = Color.FromRgb(0x5B, 0x5B, 0x60),
            Divider = Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF), // #FFFFFF@50%
            ShadowColor = Colors.Black,
            ShadowBlurRadius = 36,
            ShadowOffsetY = 14,
            ShadowOpacity = 0.22,
            HasGlossHighlight = true,
        },

        // 3. macOS-виджет — референс: встроенный виджет «Мировые часы» в Центре уведомлений macOS.
        new()
        {
            Id = "macos",
            DisplayName = "macOS-виджет",
            Surface = Color.FromRgb(0xFF, 0xFF, 0xFF),
            DefaultOpacityPercent = 88,
            PopupOpacityPercent = 96,
            Border = Color.FromArgb(0x0F, 0x00, 0x00, 0x00), // #000000@6%
            BorderThickness = 1,
            CornerRadius = 16,
            HomeHighlight = Color.FromArgb(0x1A, 0x00, 0x7A, 0xFF), // #007AFF@10%
            TextPrimary = Color.FromRgb(0x1D, 0x1D, 0x1F),
            TextSecondary = Color.FromRgb(0x86, 0x86, 0x8B),
            Divider = Color.FromArgb(0x14, 0x00, 0x00, 0x00), // #000000@8%
            ShadowColor = Colors.Black,
            ShadowBlurRadius = 20,
            ShadowOffsetY = 6,
            ShadowOpacity = 0.14,
        },

        // 4. AMOLED Dark — тень заменена на неоновое свечение.
        new()
        {
            Id = "amoled-dark",
            DisplayName = "AMOLED Dark",
            Surface = Color.FromRgb(0x00, 0x00, 0x00),
            DefaultOpacityPercent = 92,
            PopupOpacityPercent = 96,
            Border = Color.FromArgb(0x40, 0x00, 0xE5, 0xFF), // #00E5FF@25%
            BorderThickness = 1,
            CornerRadius = 10,
            HomeHighlight = Color.FromArgb(0x14, 0xFF, 0xFF, 0xFF), // #FFFFFF@8%
            TextPrimary = Color.FromRgb(0xFF, 0xFF, 0xFF),
            TextSecondary = Color.FromRgb(0x9A, 0x9A, 0xA0),
            Divider = Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF), // #FFFFFF@10%
            ShadowColor = Color.FromRgb(0x00, 0xE5, 0xFF), // неоновая, не чёрная
            ShadowBlurRadius = 24,
            ShadowOffsetY = 0,
            ShadowOpacity = 0.35,
        },

        // 5. Minimal Flat — без теней, прозрачность зафиксирована на 100%.
        new()
        {
            Id = "minimal-flat",
            DisplayName = "Minimal Flat",
            Surface = Color.FromRgb(0xF5, 0xF5, 0xF5),
            DefaultOpacityPercent = 100,
            AllowsOpacityAdjustment = false,
            PopupOpacityPercent = 100,
            Border = Color.FromArgb(0x24, 0x00, 0x00, 0x00), // #000000@14%
            BorderThickness = 1,
            CornerRadius = 4,
            HomeHighlight = Color.FromArgb(0x0D, 0x00, 0x00, 0x00), // #000000@5%
            TextPrimary = Color.FromRgb(0x11, 0x11, 0x11),
            TextSecondary = Color.FromRgb(0x6B, 0x6B, 0x6B),
            Divider = Color.FromArgb(0x1A, 0x00, 0x00, 0x00), // #000000@10%
            ShadowColor = null, // тени нет вообще — не просто прозрачная тень, а её полное отсутствие.
        },

        // 6. Rainmeter Enigma-style — референс: скин Enigma (rainmeterui.com/skin/enigma).
        new()
        {
            Id = "rainmeter-enigma",
            DisplayName = "Rainmeter Enigma-style",
            Surface = Color.FromRgb(0x0B, 0x0B, 0x0B),
            DefaultOpacityPercent = 85,
            PopupOpacityPercent = 92,
            Border = Color.FromArgb(0x59, 0x2F, 0xB5, 0xA0), // #2FB5A0@35%
            BorderThickness = 1,
            CornerRadius = 2,
            HomeHighlight = Color.FromArgb(0x0F, 0xFF, 0xFF, 0xFF), // #FFFFFF@6%
            TextPrimary = Color.FromRgb(0xE8, 0xE8, 0xE8),
            TextSecondary = Color.FromRgb(0x7A, 0x7A, 0x7A),
            Divider = Color.FromArgb(0x14, 0xFF, 0xFF, 0xFF), // #FFFFFF@8%
            ShadowColor = Colors.Black,
            ShadowBlurRadius = 16,
            ShadowOffsetY = 4,
            ShadowOpacity = 0.30,
            IsCityNameRegular = true,
        },
    };

    /// <summary>Ищет тему по id; при отсутствии/повреждённом id из settings.json — откатывается на тему по умолчанию (Windows).</summary>
    public static ThemeDefinition GetById(string? id)
    {
        if (id is not null)
        {
            foreach (var theme in All)
            {
                if (theme.Id == id)
                {
                    return theme;
                }
            }
        }

        return All[0];
    }
}
