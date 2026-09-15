using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using WorldTimeWidget.ViewModels;

namespace WorldTimeWidget.Services;

/// <summary>
/// Иконка в системном трее (<see cref="NotifyIcon"/>, <c>System.Windows.Forms</c>) — доработка v1.2.
/// Живёт на уровне приложения (создаётся/освобождается в <c>App.xaml.cs</c>), не привязана
/// к жизненному циклу <c>MainWindow</c>. Служит "предохранителем": не зависит от
/// click-through/видимости главного окна и всегда остаётся кликабельной — единственный
/// гарантированный способ выключить "Прозрачный для кликов" обратно, если виджет сам
/// перестал реагировать на мышь.
/// </summary>
public sealed class TrayIconService : IDisposable
{
    private readonly Window _window;
    private readonly MainViewModel _viewModel;
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _showHideItem;
    private readonly ToolStripMenuItem _clickThroughItem;
    private readonly ToolStripMenuItem _alwaysOnTopItem;

    public TrayIconService(Window window, MainViewModel viewModel)
    {
        _window = window;
        _viewModel = viewModel;

        _showHideItem = new ToolStripMenuItem();
        _showHideItem.Click += (_, _) => ToggleWindowVisibility();

        _clickThroughItem = new ToolStripMenuItem("Прозрачный для кликов");
        _clickThroughItem.Click += (_, _) => _viewModel.IsClickThrough = !_viewModel.IsClickThrough;

        _alwaysOnTopItem = new ToolStripMenuItem("Всегда поверх окон");
        _alwaysOnTopItem.Click += (_, _) => _viewModel.IsAlwaysOnTop = !_viewModel.IsAlwaysOnTop;

        var exitItem = new ToolStripMenuItem("Выход");
        exitItem.Click += (_, _) => _viewModel.ExitCommand.Execute(null);

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(_showHideItem);
        contextMenu.Items.Add(_clickThroughItem);
        contextMenu.Items.Add(_alwaysOnTopItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitItem);
        // ContextMenuStrip не биндится декларативно как WPF — актуализируем подписи/чекбоксы
        // вручную перед каждым показом (см. spec.md, доработка v1.2, п.1).
        contextMenu.Opening += (_, _) => RefreshMenuState();

        _notifyIcon = new NotifyIcon
        {
            Icon = LoadAppIcon(),
            Text = "WorldTimeWidget",
            ContextMenuStrip = contextMenu,
            Visible = true,
        };
        _notifyIcon.DoubleClick += (_, _) => ToggleWindowVisibility();
    }

    private void ToggleWindowVisibility()
    {
        _window.Visibility = _window.Visibility == Visibility.Visible
            ? Visibility.Hidden
            : Visibility.Visible;
    }

    private void RefreshMenuState()
    {
        var isVisible = _window.Visibility == Visibility.Visible;
        _showHideItem.Text = isVisible ? "Скрыть виджет" : "Показать виджет";
        _clickThroughItem.Checked = _viewModel.IsClickThrough;
        _alwaysOnTopItem.Checked = _viewModel.IsAlwaysOnTop;
    }

    /// <summary>
    /// Загружает ту же иконку, что зашита в exe через <c>ApplicationIcon</c> (Assets/app.ico) —
    /// извлекает её из самого исполняемого файла, без отдельного встраивания как WPF-ресурса.
    /// </summary>
    private static Icon LoadAppIcon()
    {
        var exePath = Environment.ProcessPath;
        if (exePath is not null)
        {
            var icon = Icon.ExtractAssociatedIcon(exePath);
            if (icon is not null)
            {
                return icon;
            }
        }

        return SystemIcons.Application;
    }

    public void Dispose()
    {
        // Обязательно скрыть перед Dispose — иначе иконка может "зависнуть" в трее
        // до наведения курсора на неё после завершения процесса (известная особенность NotifyIcon).
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
