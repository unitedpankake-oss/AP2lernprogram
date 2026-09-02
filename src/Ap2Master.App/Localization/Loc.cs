using System.ComponentModel;
using Ap2Master.Infrastructure.Ai;

namespace Ap2Master.App.Localization;

public sealed class Loc : INotifyPropertyChanged
{
    public static Loc Current { get; } = new();

    private Loc() => LocCatalog.EnsureComplete();

    public string Language { get; private set; } = "de";

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? LanguageChanged;

    public string this[string key] => Lookup(key);

    public IReadOnlyList<LanguageOption> Languages { get; } =
    [
        new("de", "Deutsch (Hauptsprache)"),
        new("ru", "Русский"),
        new("uk", "Українська"),
        new("en", "English"),
        new("es", "Español")
    ];

    public void SetLanguage(string? code)
    {
        var next = CodePreservingTranslator.Normalize(code);
        if (next == Language) return;
        Language = next;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public string Lookup(string key) => LocCatalog.Get(Language, key);

    public static string T(string key) => Current.Lookup(key);

    public static string T(string key, params object[] args) => string.Format(Current.Lookup(key), args);

    public string Content(string? german) => CodePreservingTranslator.TranslateInline(german ?? "", Language);

    public string RoleUser() => Lookup("Ai.You");
    public string RoleAssistant() => Lookup("Ai.Bot");
}

public sealed record LanguageOption(string Code, string NativeName);
