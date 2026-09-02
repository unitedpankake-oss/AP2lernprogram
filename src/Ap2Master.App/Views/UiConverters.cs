using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Ap2Master.Domain;

namespace Ap2Master.App.Views;

public sealed class BoolVisConverter : IValueConverter
{
    public static readonly BoolVisConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is true ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class InverseBoolToVisConverter : IValueConverter
{
    public static readonly InverseBoolToVisConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c) => value is true ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class NotEmptyToVisConverter : IValueConverter
{
    public static readonly NotEmptyToVisConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
        => string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class EmptyToVisConverter : IValueConverter
{
    public static readonly EmptyToVisConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
        => string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class PositiveToVisConverter : IValueConverter
{
    public static readonly PositiveToVisConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is int i && i > 0 ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class NullToVisConverter : IValueConverter
{
    public static readonly NullToVisConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is null ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class InverseNullToVisConverter : IValueConverter
{
    public static readonly InverseNullToVisConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is null ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class EmptyCountToVisConverter : IValueConverter
{
    public static readonly EmptyCountToVisConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is int i && i <= 0 ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

/// <summary>
/// Wandelt 0..100 in ein StrokeDashArray, damit ein Kreis als Fortschrittsring gezeichnet wird.
/// ConverterParameter: "durchmesser,strichstaerke".
/// </summary>
public sealed class RingDashConverter : IValueConverter
{
    public static readonly RingDashConverter Instance = new();

    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var parts = (p as string ?? "160,12").Split(',');
        var size = double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var s) ? s : 160;
        var thickness = parts.Length > 1 && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var th) ? th : 12;
        var percent = value switch
        {
            int i => i,
            double d => d,
            _ => 0d
        };
        percent = Math.Clamp(percent, 0, 100);
        var circumference = Math.PI * (size - thickness);
        var unit = circumference / thickness;
        return new DoubleCollection([unit * percent / 100.0, unit]);
    }

    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

/// <summary>Mastery 0..1 auf eine Farbstufe abbilden. Zahl steht immer zusätzlich im Tooltip.</summary>
public sealed class MasteryBrushConverter : IValueConverter
{
    public static readonly MasteryBrushConverter Instance = new();

    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var m = value switch { double d => d, int i => i, _ => 0d };
        var key = m switch
        {
            <= 0.001 => "LineBrush",
            < 0.34 => "ErrBrush",
            < 0.67 => "WarnBrush",
            _ => "OkBrush"
        };
        var brush = System.Windows.Application.Current?.TryFindResource(key) as Brush;
        if (brush is null) return Brushes.Gray;
        if (m <= 0.001) return brush;
        var clone = brush.Clone();
        clone.Opacity = 0.35 + 0.65 * Math.Clamp(m, 0, 1);
        clone.Freeze();
        return clone;
    }

    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class PercentTextConverter : IValueConverter
{
    public static readonly PercentTextConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var m = value switch { double d => d, int i => i, _ => 0d };
        return Math.Round(m * 100) + " %";
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class HexToBrushConverter : IValueConverter
{
    public static readonly HexToBrushConverter Instance = new();

    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return Brushes.Transparent;
        try
        {
            var converted = new BrushConverter().ConvertFromString(s);
            if (converted is not SolidColorBrush brush) return Brushes.Transparent;
            if (brush.CanFreeze) brush.Freeze();
            return brush;
        }
        catch
        {
            return Brushes.Transparent;
        }
    }

    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class SelectedBorderConverter : IValueConverter
{
    public static readonly SelectedBorderConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c) => value is true ? 2d : 1d;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class RatioToHeightConverter : IValueConverter
{
    public static readonly RatioToHeightConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var ratio = value switch { double d => d, int i => i, _ => 0d };
        var max = 88d;
        if (p is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var m)) max = m;
        return Math.Max(4, ratio * max);
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class ExamPartBrushConverter : IValueConverter
{
    public static readonly ExamPartBrushConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var key = value switch
        {
            ExamPart.Teil2 => "Teil2Brush",
            ExamPart.WiSo => "WiSoBrush",
            _ => "Teil1Brush"
        };
        return System.Windows.Application.Current?.TryFindResource(key) as Brush ?? Brushes.Gray;
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class ActiveWeightConverter : IValueConverter
{
    public static readonly ActiveWeightConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c) => value is true ? FontWeights.SemiBold : FontWeights.Normal;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class PassedGlyphConverter : IValueConverter
{
    public static readonly PassedGlyphConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c) => value is true ? "\uE73E" : "\uE711";
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class PassedBrushConverter : IValueConverter
{
    public static readonly PassedBrushConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var key = value is true ? "OkBrush" : "MutedBrush";
        return System.Windows.Application.Current?.TryFindResource(key) as Brush ?? Brushes.Gray;
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class TodayBrushConverter : IValueConverter
{
    public static readonly TodayBrushConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        var key = value is true ? "AccentSoftBrush" : "SurfaceBrush";
        return System.Windows.Application.Current?.TryFindResource(key) as Brush ?? Brushes.Transparent;
    }
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}
