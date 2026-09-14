namespace WorldTimeWidget.Models;

/// <summary>
/// Запись курируемого каталога городов для панели добавления (борд 03).
/// </summary>
/// <param name="City">Название города на русском.</param>
/// <param name="Country">Название страны на русском.</param>
/// <param name="TimeZoneId">Windows-идентификатор <see cref="TimeZoneInfo"/> (например, "Russian Standard Time").</param>
public sealed record CityInfo(string City, string Country, string TimeZoneId);
