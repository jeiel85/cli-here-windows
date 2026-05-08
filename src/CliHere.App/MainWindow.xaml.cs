using System.Windows;
using System.Windows.Controls;
using CliHere.App.ViewModels;

namespace CliHere.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        StateChanged += MainWindow_StateChanged;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateMaxRestoreGlyph();
        if (DataContext is MainViewModel vm)
        {
            await vm.OnAppStartedAsync();
        }
    }

    private void MainWindow_StateChanged(object? sender, System.EventArgs e)
    {
        UpdateMaxRestoreGlyph();
    }

    private void UpdateMaxRestoreGlyph()
    {
        if (MaxRestoreGlyph == null) return;
        MaxRestoreGlyph.Text = WindowState == WindowState.Maximized ? "❐" : "▢";
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaxRestoreButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
