using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WorldTimeWidget.Services;
using WorldTimeWidget.ViewModels;

namespace WorldTimeWidget.Views;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;
    private bool _isHoveringCard;

    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainViewModel(new SettingsService(), new AutostartService());
        DataContext = viewModel;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;

        Loaded += MainWindow_Loaded;
        LocationChanged += MainWindow_LocationChanged;
        Closing += MainWindow_Closing;
        Deactivated += MainWindow_Deactivated;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // "+" скрыта в режиме редактирования — если пользователь переключил режим,
        // не убирая курсор с карточки, актуализируем видимость hover-кнопок.
        if (e.PropertyName == nameof(MainViewModel.IsEditMode) && _isHoveringCard)
        {
            UpdateHoverButtonsVisibility();
        }
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
        // Border не клипит дочерний контент по скруглённым углам сам по себе —
        // подсветка домашней строки иначе выступала бы за скруглённые углы карточки.
        if (sender is FrameworkElement element && element.ActualWidth > 0 && element.ActualHeight > 0)
        {
            element.Clip = new RectangleGeometry(new Rect(0, 0, element.ActualWidth, element.ActualHeight), 8, 8);
        }
    }
}
