using WorldTimeWidget.Models;

namespace WorldTimeWidget.ViewModels;

/// <summary>
/// Элемент списка результатов поиска в панели добавления города (борд 03).
/// </summary>
public sealed class AddCityItemViewModel
{
    public AddCityItemViewModel(CityInfo city, string utcOffsetText, bool isAlreadyAdded, ThemeViewModel theme)
    {
        City = city;
        UtcOffsetText = utcOffsetText;
        IsAlreadyAdded = isAlreadyAdded;
        Theme = theme;
    }

    public CityInfo City { get; }

    public string CityName => City.City;

    public string CountryName => City.Country;

    public string UtcOffsetText { get; }

    /// <summary>true — этот часовой пояс уже добавлен в основной список.</summary>
    public bool IsAlreadyAdded { get; }

    /// <summary>См. <see cref="TimeZoneRowViewModel.Theme"/> — тот же общий экземпляр, для биндинга токенов темы из шаблона результата поиска (AddCityView.xaml).</summary>
    public ThemeViewModel Theme { get; }
}
