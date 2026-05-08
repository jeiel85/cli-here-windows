using System.Windows;
using CliHere.App.ViewModels;

namespace CliHere.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            await vm.OnAppStartedAsync();
        }
    }
}
