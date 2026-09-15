using System.Globalization;
using System.Windows.Data;

namespace WorldTimeWidget.Converters;

/// <summary>true → 1.0 (полная непрозрачность), false → 0.45 (приглушённая, "задизейбленная" непрозрачность).
/// Используется для визуально задизейбленного слайдера «Прозрачность» на теме Minimal Flat
/// (см. spec.md, «Доработка v1.3», ThemeDefinition.AllowsOpacityAdjustment).</summary>
public sealed class BoolToEnabledOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? 1.0 : 0.45;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
