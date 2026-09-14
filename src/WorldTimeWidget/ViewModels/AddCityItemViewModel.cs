using WorldTimeWidget.Models;

namespace WorldTimeWidget.ViewModels;

/// <summary>
/// Элемент списка результатов поиска в панели добавления города (борд 03).
/// </summary>
public sealed class AddCityItemViewModel
{
    public AddCityItemViewModel(CityInfo city, string utcOffsetText, bool isAlreadyAdded)
    {
        City = city;
        UtcOffsetText = utcOffsetText;
        IsAlreadyAdded = isAlreadyAdded;
    }

    public CityInfo City { get; }

    public string CityName => City.City;

    public string CountryName => City.Country;

    public string UtcOffsetText { get; }

    /// <summary>true — этот часовой пояс уже добавлен в основной список.</summary>
    public bool IsAlreadyAdded { get; }
}
