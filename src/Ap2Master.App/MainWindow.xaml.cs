using System.Windows;
using System.Windows.Input;
using Ap2Master.App.ViewModels;

namespace Ap2Master.App;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private void TitleDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMax();
            return;
        }
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void MinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaxClick(object sender, RoutedEventArgs e) => ToggleMax();
    private void CloseClick(object sender, RoutedEventArgs e) => Close();

    private void ToggleMax()
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void NavClick(object sender, MouseButtonEventArgs e)
    {
        // kept for compatibility; navigation is command-based
    }
}
