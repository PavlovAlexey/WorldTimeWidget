using System.Globalization;
using System.Windows.Data;

namespace WorldTimeWidget.Converters;

/// <summary>true → приглушённая непрозрачность (0.45), false → 1.0. Используется для
/// пометки уже добавленных городов в панели поиска (борд 03).</summary>
public sealed class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? 0.45 : 1.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
