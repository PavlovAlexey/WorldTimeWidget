using System.Windows;
using WorldTimeWidget.Services;
using WorldTimeWidget.Views;

namespace WorldTimeWidget;

public partial class App : Application
{
    private TrayIconService? _trayIconService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Окно создаётся и показывается явно здесь (без StartupUri) — в .NET 8 WPF окно из
        // StartupUri создаётся отложенно через Dispatcher и ещё не существует к моменту вызова
        // OnStartup, поэтому Application.MainWindow был бы null. Трей-иконка живёт на уровне
        // приложения (не привязана к жизненному циклу MainWindow), но работает с той же
        // view-model, см. spec.md, доработка v1.2.
        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();

        _trayIconService = new TrayIconService(mainWindow, mainWindow.ViewModel);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIconService?.Dispose();
        base.OnExit(e);
    }
}
