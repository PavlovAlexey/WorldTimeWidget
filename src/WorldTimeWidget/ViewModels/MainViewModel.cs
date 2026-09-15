using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using WorldTimeWidget.Models;
using WorldTimeWidget.Services;

namespace WorldTimeWidget.ViewModels;

/// <summary>
/// Главная view-model виджета: список часовых поясов, режим редактирования,
/// панель добавления города, меню и настройки.
/// </summary>
public sealed class MainViewModel : ViewModelBase
{
    public const int MinBackgroundOpacityPercent = 20;
    public const int MaxBackgroundOpacityPercent = 100;

    private readonly SettingsService _settingsService;
    private readonly AutostartService _autostartService;
    private readonly AppSettings _settings;
    private DispatcherTimer? _timer;

    private bool _isEditMode;
    private bool _isAddCityOpen;
    private bool _isMenuOpen;
    private bool _is24HourFormat;
    private bool _isAlwaysOnTop;
    private bool _isAutostartEnabled;
    private bool _isClickThrough;
    private int _backgroundOpacityPercent;
    private string _searchQuery = string.Empty;

    public MainViewModel(SettingsService settingsService, AutostartService autostartService)
    {
        _settingsService = settingsService;
        _autostartService = autostartService;
        _settings = _settingsService.Load();

        _is24HourFormat = _settings.Is24HourFormat;
        _isAlwaysOnTop = _settings.IsAlwaysOnTop;
        // Источник истины для автозапуска — реестр (мог быть изменён вручную),
        // но при расхождении с сохранённым намерением приводим реестр в соответствие настройкам.
        _isAutostartEnabled = _settings.IsAutostartEnabled;
        _isClickThrough = _settings.IsClickThrough;

        Theme = new ThemeViewModel();
        var themeDefinition = ThemeCatalogService.GetById(_settings.ThemeId);
        _backgroundOpacityPercent = ResolveOpacityForTheme(themeDefinition);
        Theme.Apply(themeDefinition, _backgroundOpacityPercent);

        Rows = new ObservableCollection<TimeZoneRowViewModel>();
        SearchResults = new ObservableCollection<AddCityItemViewModel>();

        var homeRow = new TimeZoneRowViewModel(TimeZoneInfo.Local.Id, GetHomeCityName(), isHome: true, Theme);
        Rows.Add(homeRow);

        foreach (var tzId in _settings.TimeZoneIds)
        {
            var cityName = CityCatalogService.FindByTimeZoneId(tzId)?.City ?? tzId;
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(tzId);
            }
            catch (TimeZoneNotFoundException)
            {
                continue;
            }
            catch (InvalidTimeZoneException)
            {
                continue;
            }

            Rows.Add(new TimeZoneRowViewModel(tzId, cityName, isHome: false, Theme));
        }

        ToggleEditModeCommand = new RelayCommand(ToggleEditMode);
        OpenAddCityCommand = new RelayCommand(OpenAddCity);
        CloseAddCityCommand = new RelayCommand(CloseAddCity);
        OpenMenuCommand = new RelayCommand(() =>
        {
            IsAddCityOpen = false;
            IsMenuOpen = true;
        });
        AddCityCommand = new RelayCommand(param => AddCity((AddCityItemViewModel)param!));
        RemoveCityCommand = new RelayCommand(param => RemoveCity((TimeZoneRowViewModel)param!));
        ToggleFormatCommand = new RelayCommand(() =>
        {
            Is24HourFormat = !Is24HourFormat;
            IsMenuOpen = false;
        });
        SelectThemeCommand = new RelayCommand(param => SetTheme((string)param!));
        ExitCommand = new RelayCommand(() => Application.Current.Shutdown());

        RefreshSearchResults();
        RefreshAllRows();
        StartAlignedTimer();

        // Приводим состояние реестра автозапуска в соответствие с сохранёнными настройками
        // (на случай, если реестр был изменён вне приложения).
        try
        {
            if (_autostartService.IsEnabled() != _isAutostartEnabled)
            {
                _autostartService.SetEnabled(_isAutostartEnabled);
            }
        }
        catch
        {
            // Нет доступа к реестру — не критично для запуска.
        }
    }

    public ObservableCollection<TimeZoneRowViewModel> Rows { get; }

    public ObservableCollection<AddCityItemViewModel> SearchResults { get; }

    /// <summary>Текущая активная тема оформления (см. spec.md, «Доработка v1.3»). Один и тот же экземпляр проброшен в дочерние view-model строк/результатов поиска.</summary>
    public ThemeViewModel Theme { get; }

    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            if (SetField(ref _isEditMode, value))
            {
                OnPropertyChanged(nameof(EditModeMenuLabel));
            }
        }
    }

    /// <summary>Текст пункта меню «Изменить список»/«Готово» в зависимости от режима.</summary>
    public string EditModeMenuLabel => IsEditMode ? "Готово" : "Изменить список";

    public bool IsAddCityOpen
    {
        get => _isAddCityOpen;
        set
        {
            if (SetField(ref _isAddCityOpen, value) && value)
            {
                SearchQuery = string.Empty;
                RefreshSearchResults();
                IsMenuOpen = false;
            }
        }
    }

    public bool IsMenuOpen
    {
        get => _isMenuOpen;
        set => SetField(ref _isMenuOpen, value);
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetField(ref _searchQuery, value))
            {
                RefreshSearchResults();
            }
        }
    }

    public bool Is24HourFormat
    {
        get => _is24HourFormat;
        set
        {
            if (SetField(ref _is24HourFormat, value))
            {
                OnPropertyChanged(nameof(FormatLabel));
                _settings.Is24HourFormat = value;
                RefreshAllRows();
                Save();
            }
        }
    }

    public string FormatLabel => Is24HourFormat ? "24ч" : "12ч";

    public bool IsAlwaysOnTop
    {
        get => _isAlwaysOnTop;
        set
        {
            if (SetField(ref _isAlwaysOnTop, value))
            {
                _settings.IsAlwaysOnTop = value;
                Save();
            }
        }
    }

    public bool IsAutostartEnabled
    {
        get => _isAutostartEnabled;
        set
        {
            if (SetField(ref _isAutostartEnabled, value))
            {
                try
                {
                    _autostartService.SetEnabled(value);
                }
                catch
                {
                    // Нет доступа к реестру — оставляем настройку как желаемую,
                    // но фактическое состояние автозапуска могло не примениться.
                }

                _settings.IsAutostartEnabled = value;
                Save();
            }
        }
    }

    /// <summary>
    /// Прозрачность для кликов (WS_EX_TRANSPARENT на hwnd главного окна, см. <c>ClickThroughService</c>
    /// и <c>MainWindow.xaml.cs</c>, который реагирует на изменение этого свойства). Пока включено,
    /// сам виджет не реагирует на мышь вообще — выключить обратно можно только через трей
    /// (<c>TrayIconService</c>) либо (если ещё не включено) этим же тумблером в меню.
    /// </summary>
    public bool IsClickThrough
    {
        get => _isClickThrough;
        set
        {
            if (SetField(ref _isClickThrough, value))
            {
                _settings.IsClickThrough = value;
                Save();
            }
        }
    }

    /// <summary>
    /// Прозрачность фона карточки для ТЕКУЩЕЙ активной темы, % (<see cref="MinBackgroundOpacityPercent"/>-
    /// <see cref="MaxBackgroundOpacityPercent"/>) — хранится отдельно на каждую тему, см.
    /// <c>AppSettings.BackgroundOpacityByTheme</c>. Задизейблено (см. <see cref="ThemeViewModel.AllowsOpacityAdjustment"/>)
    /// и зафиксировано на 100% для Minimal Flat.
    /// </summary>
    public int BackgroundOpacityPercent
    {
        get => _backgroundOpacityPercent;
        set
        {
            if (!Theme.AllowsOpacityAdjustment)
            {
                // Minimal Flat: слайдер задизейблен в UI, но на всякий случай не даём значению
                // отклониться от 100% и при программном вызове сеттера.
                return;
            }

            var clamped = Math.Clamp(value, MinBackgroundOpacityPercent, MaxBackgroundOpacityPercent);
            if (SetField(ref _backgroundOpacityPercent, clamped))
            {
                Theme.SetCardOpacityPercent(clamped);
                _settings.BackgroundOpacityByTheme[Theme.Definition.Id] = clamped;
                Save();
            }
        }
    }

    public RelayCommand ToggleEditModeCommand { get; }
    public RelayCommand OpenAddCityCommand { get; }
    public RelayCommand CloseAddCityCommand { get; }
    public RelayCommand OpenMenuCommand { get; }
    public RelayCommand AddCityCommand { get; }
    public RelayCommand RemoveCityCommand { get; }
    public RelayCommand ToggleFormatCommand { get; }
    public RelayCommand SelectThemeCommand { get; }
    public RelayCommand ExitCommand { get; }

    /// <summary>Вызывается из code-behind при drag-перестановке строк в режиме редактирования.</summary>
    public void MoveRow(int oldIndex, int newIndex)
    {
        // Домашняя строка (индекс 0) не участвует в перестановке.
        if (oldIndex <= 0 || newIndex <= 0)
        {
            return;
        }

        if (oldIndex == newIndex || oldIndex >= Rows.Count || newIndex >= Rows.Count)
        {
            return;
        }

        Rows.Move(oldIndex, newIndex);
        PersistCityOrder();
    }

    /// <summary>Сохраняет позицию окна (вызывается из code-behind при перемещении/закрытии).</summary>
    public void UpdateWindowPosition(double left, double top)
    {
        _settings.WindowLeft = left;
        _settings.WindowTop = top;
        Save();
    }

    public double? SavedWindowLeft => _settings.WindowLeft;

    public double? SavedWindowTop => _settings.WindowTop;

    private void ToggleEditMode()
    {
        // Пункт меню «Изменить список»/«Готово» — единственный способ войти/выйти
        // из режима редактирования через меню (см. критерий приёмки); после клика
        // меню закрывается, как и остальные однократные пункты.
        IsEditMode = !IsEditMode;
        IsMenuOpen = false;
        IsAddCityOpen = false;
    }

    private void OpenAddCity()
    {
        IsAddCityOpen = true;
    }

    private void CloseAddCity()
    {
        IsAddCityOpen = false;
    }

    private void AddCity(AddCityItemViewModel item)
    {
        if (item.IsAlreadyAdded)
        {
            return;
        }

        Rows.Add(new TimeZoneRowViewModel(item.City.TimeZoneId, item.City.City, isHome: false, Theme));
        PersistCityOrder();
        RefreshAllRows();
        RefreshSearchResults();
        IsAddCityOpen = false;
    }

    private void RemoveCity(TimeZoneRowViewModel row)
    {
        if (row.IsHome)
        {
            return;
        }

        Rows.Remove(row);
        PersistCityOrder();
        RefreshSearchResults();
    }

    private void PersistCityOrder()
    {
        _settings.TimeZoneIds = Rows.Skip(1).Select(r => r.TimeZoneId).ToList();
        Save();
    }

    private void RefreshSearchResults()
    {
        SearchResults.Clear();
        var addedIds = Rows.Select(r => r.TimeZoneId).ToHashSet();
        var now = DateTime.UtcNow;

        foreach (var city in CityCatalogService.Search(SearchQuery))
        {
            string offsetText;
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(city.TimeZoneId);
                var offset = tz.GetUtcOffset(now);
                var sign = offset < TimeSpan.Zero ? "-" : "+";
                offsetText = offset.Minutes == 0
                    ? $"UTC{sign}{Math.Abs(offset.Hours)}"
                    : $"UTC{sign}{Math.Abs(offset.Hours)}:{Math.Abs(offset.Minutes):00}";
            }
            catch (TimeZoneNotFoundException)
            {
                offsetText = string.Empty;
            }
            catch (InvalidTimeZoneException)
            {
                offsetText = string.Empty;
            }

            var alreadyAdded = addedIds.Contains(city.TimeZoneId);
            SearchResults.Add(new AddCityItemViewModel(city, offsetText, alreadyAdded, Theme));
        }
    }

    /// <summary>Переключает активную тему (пикер в меню «⋯», см. spec.md «UI переключения темы») — мгновенно, без перезапуска.</summary>
    private void SetTheme(string themeId)
    {
        var definition = ThemeCatalogService.GetById(themeId);
        if (ReferenceEquals(definition, Theme.Definition))
        {
            return;
        }

        var opacity = ResolveOpacityForTheme(definition);
        Theme.Apply(definition, opacity);

        _backgroundOpacityPercent = opacity;
        OnPropertyChanged(nameof(BackgroundOpacityPercent));

        _settings.ThemeId = definition.Id;
        if (definition.AllowsOpacityAdjustment)
        {
            _settings.BackgroundOpacityByTheme[definition.Id] = opacity;
        }

        Save();
    }

    /// <summary>Сохранённое для темы значение прозрачности карточки, либо её дизайн-дефолт, если пользователь ещё не менял слайдер для этой темы.</summary>
    private int ResolveOpacityForTheme(ThemeDefinition definition)
    {
        if (!definition.AllowsOpacityAdjustment)
        {
            return definition.DefaultOpacityPercent;
        }

        if (_settings.BackgroundOpacityByTheme.TryGetValue(definition.Id, out var saved))
        {
            return Math.Clamp(saved, MinBackgroundOpacityPercent, MaxBackgroundOpacityPercent);
        }

        return definition.DefaultOpacityPercent;
    }

    private void RefreshAllRows()
    {
        var now = DateTime.UtcNow;
        foreach (var row in Rows)
        {
            row.Refresh(now, Is24HourFormat);
        }
    }

    private void StartAlignedTimer()
    {
        var now = DateTime.Now;
        var delay = TimeSpan.FromSeconds(60 - now.Second) - TimeSpan.FromMilliseconds(now.Millisecond);
        if (delay <= TimeSpan.Zero)
        {
            delay = TimeSpan.FromMilliseconds(1);
        }

        var alignTimer = new DispatcherTimer { Interval = delay };
        alignTimer.Tick += (_, _) =>
        {
            alignTimer.Stop();
            RefreshAllRows();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            _timer.Tick += (_, _) => RefreshAllRows();
            _timer.Start();
        };
        alignTimer.Start();
    }

    private void Save()
    {
        _settingsService.Save(_settings);
    }

    private static string GetHomeCityName()
    {
        var local = TimeZoneInfo.Local;
        var byId = CityCatalogService.FindByTimeZoneId(local.Id);
        if (byId is not null)
        {
            return byId.City;
        }

        // Фолбэк: вытаскиваем первый город из DisplayName вида "(UTC+03:00) Москва, Санкт-Петербург".
        var displayName = local.DisplayName;
        var closeParen = displayName.IndexOf(')');
        if (closeParen >= 0 && closeParen + 1 < displayName.Length)
        {
            var rest = displayName[(closeParen + 1)..].Trim();
            var comma = rest.IndexOf(',');
            return comma >= 0 ? rest[..comma] : rest;
        }

        return displayName;
    }
}
