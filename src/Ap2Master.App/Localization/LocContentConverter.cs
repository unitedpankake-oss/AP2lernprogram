using System.Globalization;
using System.Windows.Data;

namespace Ap2Master.App.Localization;

public sealed class LocContentConverter : IMultiValueConverter
{
    public static readonly LocContentConverter Instance = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var text = values.Length > 0 ? values[0]?.ToString() ?? "" : "";
        var prefix = parameter as string;
        var body = Loc.Current.Content(text);
        return string.IsNullOrEmpty(prefix) ? body : Loc.T(prefix) + body;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Erster Wert ist der Katalogschlüssel, zweiter Wert nur der Sprachauslöser.</summary>
public sealed class LocKeyConverter : IMultiValueConverter
{
    public static readonly LocKeyConverter Instance = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        => Loc.T(values.Length > 0 ? values[0]?.ToString() ?? "" : "");

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class LocFormatConverter : IMultiValueConverter
{
    public static readonly LocFormatConverter Instance = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var key = parameter as string ?? "";
        var args = values.Length <= 1
            ? Array.Empty<object>()
            : values.Take(values.Length - 1).Select(v => (object)(v?.ToString() ?? "")).ToArray();
        return args.Length == 0 ? Loc.T(key) : Loc.T(key, args);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
