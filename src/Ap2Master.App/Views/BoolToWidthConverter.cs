using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ap2Master.App.Views;

public sealed class BoolToWidthConverter : IValueConverter
{
    public static readonly BoolToWidthConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c) => value is true ? 72d : 248d;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class BoolToVisConverter : IValueConverter
{
    public static readonly BoolToVisConverter Instance = new();
    public object Convert(object value, Type t, object p, CultureInfo c) => value is true ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object value, Type t, object p, CultureInfo c) => Binding.DoNothing;
}
