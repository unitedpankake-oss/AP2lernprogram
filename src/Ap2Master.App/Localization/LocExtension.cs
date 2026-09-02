using System.Windows.Markup;

namespace Ap2Master.App.Localization;

public sealed class LocExtension : MarkupExtension
{
    public LocExtension() { }
    public LocExtension(string key) => Key = key;
    public string Key { get; set; } = "";

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new System.Windows.Data.Binding($"[{Key}]")
        {
            Source = Loc.Current,
            Mode = System.Windows.Data.BindingMode.OneWay
        };
        return binding.ProvideValue(serviceProvider);
    }
}
