using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using WorldTimeWidget.Models;
using WorldTimeWidget.Services;

namespace WorldTimeWidget.ViewModels;

/// <summary>
/// Текущая активная тема оформления виджета — держит все переключаемые токены (см. spec.md,
/// «Доработка v1.3», «Архитектура») как bindable-свойства/кисти, а не статические ресурсы в
/// Colors.xaml.
///
/// Кисти (<see cref="CardBrush"/>, <see cref="PopupBrush"/> и т.д.) создаются один раз и
/// мутируются на месте (<see cref="SolidColorBrush.Color"/>) — тот же паттерн, что и
/// <c>CardBackgroundBrush</c> в MainViewModel до v1.3: WPF перерисовывает всё, что на них
/// ссылается, без пересоздания binding'ов. Единственный экземпляр этого класса передаётся не
/// только в MainViewModel, но и в дочерние view-model строк/результатов поиска
/// (<see cref="TimeZoneRowViewModel"/>, <see cref="AddCityItemViewModel"/>) под тем же именем
/// свойства <c>Theme</c> — это позволяет биндиться на токены темы прямо из вложенных
/// DataTemplate (строки списка, результаты поиска) без AncestorType-биндингов.
///
/// Геометрические/числовые токены (радиусы, толщина обводки, параметры тени, начертание шрифта) —
/// обычные bindable-свойства (SetField), т.к. их нельзя мутировать на месте так же, как кисти.
/// </summary>
public sealed class ThemeViewModel : ViewModelBase
{
    private double _cardRadius;
    private double _homeHighlightRadius;
    private CornerRadius _glossTopCornerRadius;
    private Thickness _cardBorderThickness;
    private Color _shadowColor;
    private double _shadowBlurRadius;
    private double _shadowOffsetY;
    private double _shadowOpacity;
    private FontWeight _cityNameFontWeight = FontWeights.Medium;
    private double _glossOpacity;
    private bool _allowsOpacityAdjustment = true;

    public ThemeViewModel()
    {
        Swatches = new ObservableCollection<ThemeSwatchViewModel>(
            ThemeCatalogService.All.Select(definition => new ThemeSwatchViewModel(definition)));
    }

    /// <summary>Токены-первоисточники текущей активной темы (см. <see cref="ThemeCatalogService"/>).</summary>
    public ThemeDefinition Definition { get; private set; } = ThemeCatalogService.GetById(ThemeCatalogService.DefaultThemeId);

    /// <summary>6 сваши для пикера темы в меню «⋯», в порядке из ThemeCatalogService.All.</summary>
    public ObservableCollection<ThemeSwatchViewModel> Swatches { get; }

    // ---- Кисти (мутируются на месте при Apply) ----

    /// <summary>Фон основной карточки — единственная кисть, альфа которой также меняется слайдером «Прозрачность» (см. SetCardOpacityPercent).</summary>
    public SolidColorBrush CardBrush { get; } = new(Colors.Transparent);

    /// <summary>Фон попапов (добавление города, меню «⋯») — фиксированная для темы прозрачность (popupOpacity), слайдером не регулируется.</summary>
    public SolidColorBrush PopupBrush { get; } = new(Colors.Transparent);

    public SolidColorBrush BorderBrush { get; } = new(Colors.Transparent);

    public SolidColorBrush HomeHighlightBrush { get; } = new(Colors.Transparent);

    public SolidColorBrush TextPrimaryBrush { get; } = new(Colors.Transparent);

    public SolidColorBrush TextSecondaryBrush { get; } = new(Colors.Transparent);

    public SolidColorBrush DividerBrush { get; } = new(Colors.Transparent);

    // ---- Геометрия / тень / шрифт (обычные bindable-свойства) ----

    public double CardRadius
    {
        get => _cardRadius;
        private set => SetField(ref _cardRadius, value);
    }

    /// <summary>Радиус подсветки домашней строки — CardRadius - 2, не меньше 2px.</summary>
    public double HomeHighlightRadius
    {
        get => _homeHighlightRadius;
        private set => SetField(ref _homeHighlightRadius, value);
    }

    /// <summary>Скругление только сверху — для глянцевого блика Liquid Glass (см. GlossOpacity).</summary>
    public CornerRadius GlossTopCornerRadius
    {
        get => _glossTopCornerRadius;
        private set => SetField(ref _glossTopCornerRadius, value);
    }

    /// <summary>Толщина обводки карточки/попапов сверху+по бокам (без низа — сохранён fluent-мотив «верхнего хайлайта» из v1.0-v1.2 во всех темах).</summary>
    public Thickness CardBorderThickness
    {
        get => _cardBorderThickness;
        private set => SetField(ref _cardBorderThickness, value);
    }

    public Color ShadowColor
    {
        get => _shadowColor;
        private set => SetField(ref _shadowColor, value);
    }

    public double ShadowBlurRadius
    {
        get => _shadowBlurRadius;
        private set => SetField(ref _shadowBlurRadius, value);
    }

    public double ShadowOffsetY
    {
        get => _shadowOffsetY;
        private set => SetField(ref _shadowOffsetY, value);
    }

    public double ShadowOpacity
    {
        get => _shadowOpacity;
        private set => SetField(ref _shadowOpacity, value);
    }

    /// <summary>Regular для Rainmeter Enigma-style, Medium для остальных тем.</summary>
    public FontWeight CityNameFontWeight
    {
        get => _cityNameFontWeight;
        private set => SetField(ref _cityNameFontWeight, value);
    }

    /// <summary>0/1 — видимость глянцевого блика Liquid Glass (Opacity вместо Visibility, см. устоявшийся в проекте паттерн).</summary>
    public double GlossOpacity
    {
        get => _glossOpacity;
        private set => SetField(ref _glossOpacity, value);
    }

    /// <summary>false только для Minimal Flat — слайдер «Прозрачность» в меню должен показываться задизейбленным.</summary>
    public bool AllowsOpacityAdjustment
    {
        get => _allowsOpacityAdjustment;
        private set => SetField(ref _allowsOpacityAdjustment, value);
    }

    /// <summary>
    /// Применяет тему целиком: пересчитывает все токены, мутирует общие кисти на месте и
    /// обновляет индикатор активности в <see cref="Swatches"/>.
    /// </summary>
    /// <param name="definition">Новая активная тема.</param>
    /// <param name="cardOpacityPercent">
    /// Уже выбранное для этой темы значение прозрачности карточки — сохранённое per-theme в
    /// settings.json (<c>AppSettings.BackgroundOpacityByTheme</c>) или её
    /// <see cref="ThemeDefinition.DefaultOpacityPercent"/>, если сохранённого значения ещё нет.
    /// </param>
    public void Apply(ThemeDefinition definition, int cardOpacityPercent)
    {
        Definition = definition;

        CardBrush.Color = ComputeAlphaColor(definition.Surface, cardOpacityPercent);
        PopupBrush.Color = ComputeAlphaColor(definition.Surface, definition.PopupOpacityPercent);
        BorderBrush.Color = definition.Border;
        HomeHighlightBrush.Color = definition.HomeHighlight;
        TextPrimaryBrush.Color = definition.TextPrimary;
        TextSecondaryBrush.Color = definition.TextSecondary;
        DividerBrush.Color = definition.Divider;

        CardRadius = definition.CornerRadius;
        HomeHighlightRadius = Math.Max(2, definition.CornerRadius - 2);
        GlossTopCornerRadius = new CornerRadius(definition.CornerRadius, definition.CornerRadius, 0, 0);
        CardBorderThickness = new Thickness(definition.BorderThickness, definition.BorderThickness, definition.BorderThickness, 0);

        var hasShadow = definition.ShadowColor.HasValue;
        ShadowColor = definition.ShadowColor ?? Colors.Black;
        // Minimal Flat (shadow: null): обнуляем блюр/смещение/непрозрачность вместе — визуально
        // полностью неотличимо от отсутствия эффекта, а не просто "почти прозрачная" тень.
        ShadowBlurRadius = hasShadow ? definition.ShadowBlurRadius : 0;
        ShadowOffsetY = hasShadow ? definition.ShadowOffsetY : 0;
        ShadowOpacity = hasShadow ? definition.ShadowOpacity : 0;

        CityNameFontWeight = definition.IsCityNameRegular ? FontWeights.Regular : FontWeights.Medium;
        GlossOpacity = definition.HasGlossHighlight ? 1.0 : 0.0;
        AllowsOpacityAdjustment = definition.AllowsOpacityAdjustment;

        foreach (var swatch in Swatches)
        {
            swatch.IsActive = swatch.Id == definition.Id;
        }

        OnPropertyChanged(nameof(Definition));
    }

    /// <summary>Обновляет только альфа-канал фона карточки (слайдер «Прозрачность» в меню) — остальные токены темы не трогает.</summary>
    public void SetCardOpacityPercent(int percent)
    {
        CardBrush.Color = ComputeAlphaColor(Definition.Surface, percent);
    }

    /// <summary>Пересчитывает процент (0-100) в цвет с альфа-каналом поверх переданного базового RGB.</summary>
    private static Color ComputeAlphaColor(Color baseColor, int percent)
    {
        var alpha = (byte)Math.Clamp((int)Math.Round(percent / 100.0 * 255.0), 0, 255);
        return Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
    }
}
