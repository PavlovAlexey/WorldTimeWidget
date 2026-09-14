using WorldTimeWidget.Models;

namespace WorldTimeWidget.Services;

/// <summary>
/// Статический курируемый каталог популярных городов для панели добавления (борд 03).
/// Каждый город замаплен на Windows-идентификатор <see cref="TimeZoneInfo"/>.
/// </summary>
public static class CityCatalogService
{
    public static IReadOnlyList<CityInfo> AllCities { get; } = new List<CityInfo>
    {
        // Россия и СНГ
        new("Москва", "Россия", "Russian Standard Time"),
        new("Калининград", "Россия", "Kaliningrad Standard Time"),
        new("Екатеринбург", "Россия", "Ekaterinburg Standard Time"),
        new("Новосибирск", "Россия", "N. Central Asia Standard Time"),
        new("Красноярск", "Россия", "North Asia Standard Time"),
        new("Иркутск", "Россия", "North Asia East Standard Time"),
        new("Якутск", "Россия", "Yakutsk Standard Time"),
        new("Владивосток", "Россия", "Vladivostok Standard Time"),
        new("Магадан", "Россия", "Magadan Standard Time"),
        new("Минск", "Беларусь", "Belarus Standard Time"),
        new("Киев", "Украина", "FLE Standard Time"),
        new("Алматы", "Казахстан", "Central Asia Standard Time"),
        new("Ташкент", "Узбекистан", "West Asia Standard Time"),
        new("Баку", "Азербайджан", "Azerbaijan Standard Time"),
        new("Ереван", "Армения", "Caucasus Standard Time"),
        new("Тбилиси", "Грузия", "Georgian Standard Time"),

        // Европа
        new("Лондон", "Великобритания", "GMT Standard Time"),
        new("Дублин", "Ирландия", "GMT Standard Time"),
        new("Лиссабон", "Португалия", "GMT Standard Time"),
        new("Париж", "Франция", "Romance Standard Time"),
        new("Мадрид", "Испания", "Romance Standard Time"),
        new("Брюссель", "Бельгия", "Romance Standard Time"),
        new("Копенгаген", "Дания", "Romance Standard Time"),
        new("Берлин", "Германия", "W. Europe Standard Time"),
        new("Рим", "Италия", "W. Europe Standard Time"),
        new("Амстердам", "Нидерланды", "W. Europe Standard Time"),
        new("Вена", "Австрия", "W. Europe Standard Time"),
        new("Стокгольм", "Швеция", "W. Europe Standard Time"),
        new("Осло", "Норвегия", "W. Europe Standard Time"),
        new("Цюрих", "Швейцария", "W. Europe Standard Time"),
        new("Варшава", "Польша", "Central European Standard Time"),
        new("Прага", "Чехия", "Central Europe Standard Time"),
        new("Будапешт", "Венгрия", "Central Europe Standard Time"),
        new("Белград", "Сербия", "Central Europe Standard Time"),
        new("Хельсинки", "Финляндия", "FLE Standard Time"),
        new("Рига", "Латвия", "FLE Standard Time"),
        new("Вильнюс", "Литва", "FLE Standard Time"),
        new("Таллин", "Эстония", "FLE Standard Time"),
        new("София", "Болгария", "FLE Standard Time"),
        new("Афины", "Греция", "GTB Standard Time"),
        new("Бухарест", "Румыния", "GTB Standard Time"),
        new("Стамбул", "Турция", "Turkey Standard Time"),

        // Ближний Восток и Африка
        new("Каир", "Египет", "Egypt Standard Time"),
        new("Иерусалим", "Израиль", "Israel Standard Time"),
        new("Дубай", "ОАЭ", "Arabian Standard Time"),
        new("Эр-Рияд", "Саудовская Аравия", "Arab Standard Time"),
        new("Тегеран", "Иран", "Iran Standard Time"),
        new("Найроби", "Кения", "E. Africa Standard Time"),
        new("Йоханнесбург", "ЮАР", "South Africa Standard Time"),
        new("Лагос", "Нигерия", "W. Central Africa Standard Time"),
        new("Касабланка", "Марокко", "Morocco Standard Time"),

        // Азия
        new("Токио", "Япония", "Tokyo Standard Time"),
        new("Сеул", "Южная Корея", "Korea Standard Time"),
        new("Пекин", "Китай", "China Standard Time"),
        new("Гонконг", "Китай", "China Standard Time"),
        new("Тайбэй", "Тайвань", "Taipei Standard Time"),
        new("Сингапур", "Сингапур", "Singapore Standard Time"),
        new("Куала-Лумпур", "Малайзия", "Singapore Standard Time"),
        new("Бангкок", "Таиланд", "SE Asia Standard Time"),
        new("Джакарта", "Индонезия", "SE Asia Standard Time"),
        new("Ханой", "Вьетнам", "SE Asia Standard Time"),
        new("Дели", "Индия", "India Standard Time"),
        new("Карачи", "Пакистан", "Pakistan Standard Time"),
        new("Дакка", "Бангладеш", "Bangladesh Standard Time"),
        new("Улан-Батор", "Монголия", "Ulaanbaatar Standard Time"),

        // Австралия и Океания
        new("Сидней", "Австралия", "AUS Eastern Standard Time"),
        new("Мельбурн", "Австралия", "AUS Eastern Standard Time"),
        new("Брисбен", "Австралия", "E. Australia Standard Time"),
        new("Перт", "Австралия", "W. Australia Standard Time"),
        new("Окленд", "Новая Зеландия", "New Zealand Standard Time"),

        // Северная Америка
        new("Нью-Йорк", "США", "Eastern Standard Time"),
        new("Торонто", "Канада", "Eastern Standard Time"),
        new("Чикаго", "США", "Central Standard Time"),
        new("Мехико", "Мексика", "Central Standard Time (Mexico)"),
        new("Денвер", "США", "Mountain Standard Time"),
        new("Лос-Анджелес", "США", "Pacific Standard Time"),
        new("Ванкувер", "Канада", "Pacific Standard Time"),
        new("Анкоридж", "США", "Alaskan Standard Time"),
        new("Гонолулу", "США", "Hawaiian Standard Time"),

        // Южная Америка
        new("Богота", "Колумбия", "SA Pacific Standard Time"),
        new("Лима", "Перу", "SA Pacific Standard Time"),
        new("Сантьяго", "Чили", "Pacific SA Standard Time"),
        new("Сан-Паулу", "Бразилия", "E. South America Standard Time"),
        new("Буэнос-Айрес", "Аргентина", "Argentina Standard Time"),
    };

    /// <summary>
    /// Поиск городов по подстроке в названии (регистронезависимо).
    /// Пустой запрос возвращает весь каталог.
    /// </summary>
    public static IEnumerable<CityInfo> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return AllCities;
        }

        return AllCities.Where(c =>
            c.City.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            c.Country.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Пытается найти название города в каталоге по идентификатору часового пояса
    /// (используется, чтобы подписать строки, добавленные ранее и хранящиеся в settings.json
    /// только как TimeZoneInfo.Id).
    /// </summary>
    public static CityInfo? FindByTimeZoneId(string timeZoneId)
    {
        return AllCities.FirstOrDefault(c => c.TimeZoneId == timeZoneId);
    }
}
