using System.Globalization;
using WorldTimeWidget.Services;

namespace WorldTimeWidget.ViewModels;

/// <summary>
/// Строка списка городов (борд 01/02/04): часы, дата, индикатор день/ночь.
/// </summary>
public sealed class TimeZoneRowViewModel : ViewModelBase
{
    private string _timeText = string.Empty;
    private string _dateText = string.Empty;
    private string _utcOffsetText = string.Empty;
    private bool _isNight;

    public TimeZoneRowViewModel(string timeZoneId, string cityName, bool isHome)
    {
        TimeZoneId = timeZoneId;
        CityName = cityName;
        IsHome = isHome;

        try
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Пояс мог исчезнуть из системы (редко) — откатываемся на UTC,
            // чтобы строка не ломала весь список.
            TimeZone = TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            TimeZone = TimeZoneInfo.Utc;
        }
    }

    /// <summary>Windows-идентификатор часового пояса (см. <see cref="TimeZoneInfo.Id"/>).</summary>
    public string TimeZoneId { get; }

    public TimeZoneInfo TimeZone { get; }

    /// <summary>Название города для отображения в строке.</summary>
    public string CityName { get; }

    /// <summary>true — домашний (системный) пояс: первая строка, подсвечена, неудаляема.</summary>
    public bool IsHome { get; }

    public string TimeText
    {
        get => _timeText;
        private set => SetField(ref _timeText, value);
    }

    public string DateText
    {
        get => _dateText;
        private set => SetField(ref _dateText, value);
    }

    public string UtcOffsetText
    {
        get => _utcOffsetText;
        private set => SetField(ref _utcOffsetText, value);
    }

    /// <summary>true — ночь в этом поясе (22:00–05:59), для цвета индикатора-точки.</summary>
    public bool IsNight
    {
        get => _isNight;
        private set => SetField(ref _isNight, value);
    }

    /// <summary>
    /// Пересчитывает отображаемые время/дату/смещение для текущего момента.
    /// </summary>
    public void Refresh(DateTime utcNow, bool is24HourFormat)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZone);

        if (is24HourFormat)
        {
            TimeText = local.ToString("HH:mm", CultureInfo.CurrentCulture);
        }
        else
        {
            // Не полагаемся на "tt": у культуры ru-RU AM/PM-обозначения пустые,
            // из-за чего 12-часовой формат становится неоднозначным (2:18 — утра или вечера?).
            var hour12 = local.Hour % 12 == 0 ? 12 : local.Hour % 12;
            var period = local.Hour < 12 ? "AM" : "PM";
            TimeText = $"{hour12}:{local.Minute:00} {period}";
        }

        DateText = local.ToString("ddd, d MMM", CultureInfo.CurrentCulture);

        var offset = TimeZone.GetUtcOffset(utcNow);
        var sign = offset < TimeSpan.Zero ? "-" : "+";
        UtcOffsetText = offset.Minutes == 0
            ? $"UTC{sign}{Math.Abs(offset.Hours)}"
            : $"UTC{sign}{Math.Abs(offset.Hours)}:{Math.Abs(offset.Minutes):00}";

        var hour = local.Hour;
        IsNight = hour is >= 22 or < 6;
    }
}
