using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Ap2Master.App.ViewModels;
using Ap2Master.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Ap2Master.App.Views;

public partial class LernkartenView : UserControl
{
    private bool _flipping;

    public LernkartenView() => InitializeComponent();

    private void OnCardFlip(object sender, MouseButtonEventArgs e) => _ = FlipAnimatedAsync();

    private void OnFlipClick(object sender, RoutedEventArgs e) => _ = FlipAnimatedAsync();

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Space && Keyboard.FocusedElement is not TextBox)
        {
            _ = FlipAnimatedAsync();
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    private async Task FlipAnimatedAsync()
    {
        if (_flipping || DataContext is not LernkartenViewModel vm) return;
        _flipping = true;
        try
        {
            var reduced = App.Services.GetRequiredService<IThemeService>().ReducedMotion;
            if (!reduced && CardScale is not null)
            {
                await AnimateScale(1, 0);
                vm.FlipCommand.Execute(null);
                await AnimateScale(0, 1);
                CardScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                CardScale.ScaleX = 1;
            }
            else
                vm.FlipCommand.Execute(null);
        }
        finally
        {
            _flipping = false;
        }
    }

    private Task AnimateScale(double from, double to)
    {
        var tcs = new TaskCompletionSource();
        var anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(90))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        anim.Completed += (_, _) => tcs.TrySetResult();
        CardScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        return tcs.Task;
    }
}
