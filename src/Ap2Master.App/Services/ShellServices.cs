using System.Windows;
using System.Windows.Media;
using Ap2Master.Application.Abstractions;
using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.Domain.Ui;
using Microsoft.Win32;

namespace Ap2Master.App.Services;

public sealed class ThemeService : IThemeService
{
    public ThemeKind Current { get; private set; } = ThemeKind.Dark;
    public ThemeKind CurrentTheme => Current;
    public ThemeKind ResolvedTheme { get; private set; } = ThemeKind.Dark;
    public double FontScale { get; private set; } = 1;
    public bool ReducedMotion { get; private set; }
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public ThemeService()
    {
        SystemEvents.UserPreferenceChanged += OnPreference;
    }

    public void Apply(UserSettings settings)
    {
        FontScale = settings.FontScale;
        ReducedMotion = settings.ReducedMotion;
        ApplyTheme(settings.Theme);
        System.Windows.Application.Current?.MainWindow?.SetValue(System.Windows.Controls.Control.FontSizeProperty, 14 * FontScale);
    }

    public void Apply(UserSettingsProxy settings)
    {
        FontScale = settings.FontScale;
        ReducedMotion = settings.ReducedMotion;
        ApplyTheme(settings.Theme);
        System.Windows.Application.Current?.MainWindow?.SetValue(System.Windows.Controls.Control.FontSizeProperty, 14 * FontScale);
    }

    public void ApplyTheme(ThemeKind theme)
    {
        Current = theme;
        ResolvedTheme = FocusTheme.Resolve(theme, WindowsAppsUseLight(), SystemParameters.HighContrast);
        var d = System.Windows.Application.Current?.Resources;
        if (d is not null)
        {
            foreach (var kv in FocusTheme.For(ResolvedTheme))
                d[kv.Key] = BrushOf(kv.Value);
        }
        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs { Requested = Current, Resolved = ResolvedTheme });
    }

    void IThemeService.Apply(UserSettings settings) => Apply(settings);

    private void OnPreference(object sender, UserPreferenceChangedEventArgs e)
    {
        if (Current != ThemeKind.System) return;
        if (e.Category is not (UserPreferenceCategory.General or UserPreferenceCategory.Color)) return;
        var app = System.Windows.Application.Current;
        if (app is null) return;
        app.Dispatcher.Invoke(() => ApplyTheme(ThemeKind.System));
    }

    public static bool WindowsAppsUseLight()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var v = key?.GetValue("AppsUseLightTheme");
            return v is int i && i == 1;
        }
        catch
        {
            return false;
        }
    }

    private static SolidColorBrush BrushOf(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!);
        brush.Freeze();
        return brush;
    }
}

public sealed record UserSettingsProxy(ThemeKind Theme, double FontScale, bool ReducedMotion);

public sealed class NavigationService : INavigationService
{
    private readonly Dictionary<AppSection, Func<object>> _factories = new();
    public AppSection Current { get; private set; } = AppSection.Dashboard;
    public object CurrentViewModel { get; private set; } = new();
    public event EventHandler? CurrentChanged;
    public void Register(AppSection section, Func<object> factory) => _factories[section] = factory;
    public void Navigate(AppSection section, object? parameter = null)
    {
        Current = section;
        CurrentViewModel = _factories[section]();
        if (CurrentViewModel is INavigationAware aware) aware.OnNavigated(parameter);
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }
}

public interface INavigationAware { void OnNavigated(object? parameter); }

public sealed class DialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string title, string message)
        => Task.FromResult(MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes);
    public Task AlertAsync(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }
}

public static class UiHelpers
{
    public static async Task<UserSettings> Settings(Ap2Master.Infrastructure.Persistence.AppDbContext db)
        => await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstAsync(db.UserSettings);
}
