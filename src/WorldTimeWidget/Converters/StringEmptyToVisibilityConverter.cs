using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WorldTimeWidget.Converters;

/// <summary>Пустая/null строка → Visible (для плейсхолдера поля поиска), иначе Collapsed.</summary>
public sealed class StringEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
