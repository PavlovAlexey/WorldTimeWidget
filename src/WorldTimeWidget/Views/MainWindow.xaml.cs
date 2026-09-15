using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using WorldTimeWidget.Services;
using WorldTimeWidget.ViewModels;

namespace WorldTimeWidget.Views;

public partial class MainWindow : Window
{
    /// <summary>Открыт для <c>App.xaml.cs</c> — трей-иконка живёт на уровне приложения
    /// (см. TrayIconService) и работает с той же view-model, что и главное окно.</summary>
    public MainViewModel ViewModel => (MainViewModel)DataContext;

    private bool _isHoveringCard;
    private IntPtr _hwnd = IntPtr.Zero;

    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainViewModel(new SettingsService(), new AutostartService());
        DataContext = viewModel;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Радиус скругления карточки меняется с темой (доработка v1.3) — контент нужно
        // переклипировать даже без изменения размеров окна (см. UpdateContentClip).
        viewModel.Theme.PropertyChanged += ThemeViewModel_PropertyChanged;

        Loaded += MainWindow_Loaded;
        LocationChanged += MainWindow_LocationChanged;
        Closing += MainWindow_Closing;
        Deactivated += MainWindow_Deactivated;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // hwnd появляется только здесь — применяем восстановленную из settings.json настройку
        // click-through сразу, до показа окна пользователю (окно всегда стартует видимым,
        // но может стартовать уже "прозрачным для кликов", если было включено в прошлый раз).
        _hwnd = new WindowInteropHelper(this).Handle;
        ApplyClickThrough(ViewModel.IsClickThrough);
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // "+" скрыта в режиме редактирования — если пользователь переключил режим,
        // не убирая курсор с карточки, актуализируем видимость hover-кнопок.
        if (e.PropertyName == nameof(MainViewModel.IsEditMode) && _isHoveringCard)
        {
            UpdateHoverButtonsVisibility();
        }
        else if (e.PropertyName == nameof(MainViewModel.IsClickThrough))
        {
            ApplyClickThrough(ViewModel.IsClickThrough);
        }
    }

    private void ThemeViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.ThemeViewModel.CardRadius))
        {
            UpdateContentClip();
        }
    }

    /// <summary>
    /// Применяет/снимает WS_EX_TRANSPARENT на hwnd главного окна (см. ClickThroughService).
    /// Единственный штатный способ выключить обратно, если уже включено — трей (см.
    /// TrayIconService), так как при включённом click-through окно не получает кликов мыши
    /// вообще, включая по собственным кнопкам/меню.
    /// </summary>
    private void ApplyClickThrough(bool enabled)
    {
        ClickThroughService.SetClickThrough(_hwnd, enabled);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var vm = ViewModel;
        if (vm.SavedWindowLeft is double left && vm.SavedWindowTop is double top)
        {
            Left = left;
            Top = top;
        }
        else
        {
            // По умолчанию — центр-справа основного экрана (см. spec.md, «Хранение настроек»).
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - ActualWidth - 24;
            Top = workArea.Top + (workArea.Height - ActualHeight) / 2;
        }
    }

    private void MainWindow_LocationChanged(object? sender, EventArgs e)
    {
        ViewModel.UpdateWindowPosition(Left, Top);
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        ViewModel.UpdateWindowPosition(Left, Top);
    }

    private void MainWindow_Deactivated(object? sender, EventArgs e)
    {
        // Клик вне карточки — один из двух способов выйти из режима редактирования
        // (см. spec.md, «Режим редактирования списка»). Не трогаем режим, если у пользователя
        // сейчас открыта одна из наших же всплывающих панелей (add-city/меню) —
        // переключение фокуса на них тоже деактивирует главное окно.
        if (!ViewModel.IsAddCityOpen && !ViewModel.IsMenuOpen)
        {
            ViewModel.IsEditMode = false;
        }
    }

    private void RootBorder_MouseEnter(object sender, MouseEventArgs e)
    {
        _isHoveringCard = true;
        UpdateHoverButtonsVisibility();
    }

    private void RootBorder_MouseLeave(object sender, MouseEventArgs e)
    {
        _isHoveringCard = false;
        SetHoverButtonShown(AddButton, false);
        SetHoverButtonShown(MenuButton, false);
    }

    /// <summary>
    /// Показывает hover-кнопки "+"/"⋯" (борд 02) поверх карточки. "+" скрыта в режиме
    /// редактирования (там уже есть своя ghost-кнопка добавления, борд 04).
    /// Показ/скрытие делается через Opacity/IsHitTestVisible, а не Visibility: у шаблонизированной
    /// Button переключение Visibility после первого показа окна в этом окружении не перерисовывалось
    /// (см. заметку в отчёте разработчика) — Opacity/IsHitTestVisible работает надёжно.
    /// </summary>
    private void UpdateHoverButtonsVisibility()
    {
        SetHoverButtonShown(AddButton, !ViewModel.IsEditMode);
        SetHoverButtonShown(MenuButton, true);
    }

    private static void SetHoverButtonShown(UIElement button, bool shown)
    {
        button.Opacity = shown ? 1 : 0;
        button.IsHitTestVisible = shown;
    }

    private void RootBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Перетаскивание окна за пустую область карточки. Клики по кнопкам сюда не доходят —
        // ButtonBase помечает MouseLeftButtonDown как Handled.
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                // Кнопка мыши была отпущена до вызова DragMove — игнорируем.
            }
        }
    }

    private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TimeZoneRowViewModel row } element)
        {
            e.Handled = true;
            DragDrop.DoDragDrop(element, row, DragDropEffects.Move);
        }
    }

    private void RowBorder_Drop(object sender, DragEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TimeZoneRowViewModel targetRow } &&
            e.Data.GetData(typeof(TimeZoneRowViewModel)) is TimeZoneRowViewModel sourceRow &&
            !ReferenceEquals(sourceRow, targetRow))
        {
            var vm = ViewModel;
            var oldIndex = vm.Rows.IndexOf(sourceRow);
            var newIndex = vm.Rows.IndexOf(targetRow);
            if (oldIndex >= 0 && newIndex >= 0)
            {
                vm.MoveRow(oldIndex, newIndex);
            }
        }
    }

    private void ContentClipPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateContentClip();
    }

    /// <summary>
    /// Border не клипит дочерний контент по скруглённым углам сам по себе — подсветка домашней
    /// строки иначе выступала бы за скруглённые углы карточки. Радиус зависит от активной темы
    /// (доработка v1.3), поэтому переклипируем не только при изменении размера, но и при
    /// переключении темы (см. ThemeViewModel_PropertyChanged).
    /// </summary>
    private void UpdateContentClip()
    {
        if (ContentClipPanel.ActualWidth > 0 && ContentClipPanel.ActualHeight > 0)
        {
            var radius = ViewModel.Theme.CardRadius;
            ContentClipPanel.Clip = new RectangleGeometry(
                new Rect(0, 0, ContentClipPanel.ActualWidth, ContentClipPanel.ActualHeight), radius, radius);
        }
    }
}
