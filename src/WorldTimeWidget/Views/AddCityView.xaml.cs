using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WorldTimeWidget.ViewModels;

namespace WorldTimeWidget.Views;

public partial class AddCityView : UserControl
{
    public AddCityView()
    {
        InitializeComponent();
        IsVisibleChanged += OnIsVisibleChanged;
    }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (IsVisible)
        {
            // Панель открылась (борд 03) — сразу ставим фокус в поле поиска.
            Dispatcher.BeginInvoke(new Action(() => SearchBox.Focus()), System.Windows.Threading.DispatcherPriority.Input);
        }
    }

    private void SearchResultRow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: AddCityItemViewModel item } &&
            DataContext is MainViewModel viewModel)
        {
            viewModel.AddCityCommand.Execute(item);
        }
    }
}
