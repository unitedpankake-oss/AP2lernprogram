using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ap2Master.App.ViewModels;

namespace Ap2Master.App.Views;

public partial class LernplanView : UserControl
{
    private Point _dragStart;

    public LernplanView() => InitializeComponent();

    private void BlockDown(object sender, MouseButtonEventArgs e)
    {
        _dragStart = e.GetPosition(null);
        if (sender is FrameworkElement { DataContext: PlanBlockVm block } && DataContext is LernplanViewModel vm)
            vm.SelectBlock(block);
    }

    private void BlockMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        var pos = e.GetPosition(null);
        if (Math.Abs(pos.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(pos.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;
        if (sender is FrameworkElement { DataContext: PlanBlockVm block })
            DragDrop.DoDragDrop((DependencyObject)sender, block.Id.ToString(), DragDropEffects.Move);
    }

    private void DayDragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.StringFormat) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private async void DayDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.StringFormat) is not string raw || !Guid.TryParse(raw, out var id)) return;
        if (sender is not FrameworkElement { DataContext: WeekDayVm day }) return;
        if (DataContext is not LernplanViewModel vm) return;
        await vm.MoveToAsync(id, day.Date);
    }
}
