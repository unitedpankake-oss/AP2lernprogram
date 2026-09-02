using Ap2Master.Domain;

namespace Ap2Master.Domain.Ui;

/// <summary>AP2 Focus palettes. Hex only — WPF applies them as frozen brushes.</summary>
public static class FocusTheme
{
    public static IReadOnlyDictionary<string, string> Dark { get; } = Palette(
        bg: "#FF0B0F19", surface: "#FF161B2C", alt: "#FF1D2437", sidebar: "#FF0E1220",
        text: "#FFE8ECF5", muted: "#FF94A0B8", line: "#FF242C42",
        primary: "#FF3B82F6", primaryHover: "#FF2563EB", primaryPressed: "#FF1D4ED8",
        cyan: "#FF22D3EE", ok: "#FF22C55E", warn: "#FFF59E0B", err: "#FFEF4444", danger: "#FFDC2626",
        focus: "#FFFFB900", hover: "#22FFFFFF",
        teil1: "#FF3B82F6", teil2: "#FF8B5CF6", wiso: "#FFF59E0B",
        accentSoft: "#403B82F6", okSoft: "#3322C55E");

    public static IReadOnlyDictionary<string, string> Light { get; } = Palette(
        bg: "#FFF4F6FB", surface: "#FFFFFFFF", alt: "#FFF0F3F9", sidebar: "#FFFFFFFF",
        text: "#FF15192B", muted: "#FF5C6784", line: "#FFDDE3EE",
        primary: "#FF2563EB", primaryHover: "#FF1D4ED8", primaryPressed: "#FF1E3A8A",
        cyan: "#FF0891B2", ok: "#FF0F7B3B", warn: "#FF9A5B00", err: "#FFC42B1C", danger: "#FFC42B1C",
        focus: "#FFFFB900", hover: "#14000000",
        teil1: "#FF2563EB", teil2: "#FF7C3AED", wiso: "#FFB45309",
        accentSoft: "#252563EB", okSoft: "#220F7B3B");

    public static IReadOnlyDictionary<string, string> HighContrast { get; } = Palette(
        bg: "#FF000000", surface: "#FF000000", alt: "#FF000000", sidebar: "#FF000000",
        text: "#FFFFFF00", muted: "#FFFFFFFF", line: "#FFFFFFFF",
        primary: "#FF00FFFF", primaryHover: "#FF00DDDD", primaryPressed: "#FF00AAAA",
        cyan: "#FF00FFFF", ok: "#FF00FF00", warn: "#FFFFFF00", err: "#FFFF6060", danger: "#FFFF6060",
        focus: "#FFFFFF00", hover: "#44FFFFFF",
        teil1: "#FF00FFFF", teil2: "#FFFF00FF", wiso: "#FFFFFF00",
        accentSoft: "#5500FFFF", okSoft: "#4400FF00");

    public static ThemeKind Resolve(ThemeKind requested, bool windowsLight, bool highContrast)
    {
        if (requested != ThemeKind.System) return requested;
        if (highContrast) return ThemeKind.HighContrast;
        return windowsLight ? ThemeKind.Light : ThemeKind.Dark;
    }

    public static IReadOnlyDictionary<string, string> For(ThemeKind resolved) => resolved switch
    {
        ThemeKind.Light => Light,
        ThemeKind.HighContrast => HighContrast,
        _ => Dark
    };

    public static TimeSpan Motion { get; } = TimeSpan.FromMilliseconds(160);
    public static TimeSpan SearchDebounce { get; } = TimeSpan.FromMilliseconds(250);

    public static IReadOnlyList<int> Spacing { get; } = [4, 8, 12, 16, 24, 32, 48];

    private static Dictionary<string, string> Palette(
        string bg, string surface, string alt, string sidebar,
        string text, string muted, string line,
        string primary, string primaryHover, string primaryPressed,
        string cyan, string ok, string warn, string err, string danger,
        string focus, string hover,
        string teil1, string teil2, string wiso,
        string accentSoft, string okSoft)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["AppBackgroundBrush"] = bg,
            ["SurfacePrimaryBrush"] = surface,
            ["SurfaceSecondaryBrush"] = alt,
            ["SurfaceElevatedBrush"] = surface,
            ["SidebarBrush"] = sidebar,
            ["TextPrimaryBrush"] = text,
            ["TextSecondaryBrush"] = muted,
            ["AppBorderBrush"] = line,
            ["PrimaryBrush"] = primary,
            ["PrimaryHoverBrush"] = primaryHover,
            ["PrimaryPressedBrush"] = primaryPressed,
            ["InfoBrush"] = cyan,
            ["SuccessBrush"] = ok,
            ["WarningBrush"] = warn,
            ["ErrorBrush"] = err,
            ["DangerBrush"] = danger,
            ["FocusBrush"] = focus,
            ["HoverBrush"] = hover,
            ["AccentSoftBrush"] = accentSoft,
            ["OkSoftBrush"] = okSoft,
            ["Teil1Brush"] = teil1,
            ["Teil2Brush"] = teil2,
            ["WiSoBrush"] = wiso,
            ["BgBrush"] = bg,
            ["SurfaceBrush"] = surface,
            ["SurfaceAltBrush"] = alt,
            ["TextBrush"] = text,
            ["MutedBrush"] = muted,
            ["LineBrush"] = line,
            ["AccentBrush"] = primary,
            ["OkBrush"] = ok,
            ["WarnBrush"] = warn,
            ["ErrBrush"] = err
        };
        return map;
    }
}

public sealed record ShellNavEntry(AppSection Section, string TitleKey, string Glyph);
public sealed record ShellNavGroup(string TitleKey, IReadOnlyList<ShellNavEntry> Items);

public static class ShellNavigation
{
    public static IReadOnlyList<ShellNavGroup> Groups { get; } =
    [
        new("NavGroup.Overview",
        [
            new(AppSection.Dashboard, "Nav.Dashboard", "\uE80F")
        ]),
        new("NavGroup.Learn",
        [
            new(AppSection.Lernplan, "Nav.Plan", "\uE787"),
            new(AppSection.Themenkatalog, "Nav.Catalog", "\uE8A5"),
            new(AppSection.Lernkarten, "Nav.Cards", "\uE8A1"),
            new(AppSection.Aufgabentrainer, "Nav.Exercises", "\uE70F")
        ]),
        new("NavGroup.Labs",
        [
            new(AppSection.SqlLabor, "Nav.Sql", "\uE8F1"),
            new(AppSection.PseudocodeTrainer, "Nav.Pseudo", "\uE943"),
            new(AppSection.UmlErTrainer, "Nav.Uml", "\uE8FD"),
            new(AppSection.ProjektmanagementLabor, "Nav.Project", "\uE916"),
            new(AppSection.CoverageTrainer, "Nav.Coverage", "\uE73E"),
            new(AppSection.NetzwerkSecurityTrainer, "Nav.Netz", "\uE72E")
        ]),
        new("NavGroup.Exam",
        [
            new(AppSection.WiSoTrainer, "Nav.Wiso", "\uE8D6"),
            new(AppSection.Probeklausuren, "Nav.Exam", "\uE7C3"),
            new(AppSection.Fehlerbuch, "Nav.Mistakes", "\uE7BA"),
            new(AppSection.Statistiken, "Nav.Stats", "\uE9D2")
        ]),
        new("NavGroup.System",
        [
            new(AppSection.InhalteVerwalten, "Nav.Content", "\uE8B7"),
            new(AppSection.Einstellungen, "Nav.Settings", "\uE713")
        ])
    ];

    public static IReadOnlyList<AppSection> AllSections
        => Groups.SelectMany(g => g.Items.Select(i => i.Section)).ToList();

    public static AppSection? MapActivity(ActivityKind kind) => kind switch
    {
        ActivityKind.Theory => AppSection.Themenkatalog,
        ActivityKind.Flashcards => AppSection.Lernkarten,
        ActivityKind.Exercise => AppSection.Aufgabentrainer,
        ActivityKind.SqlLab => AppSection.SqlLabor,
        ActivityKind.Algorithm => AppSection.PseudocodeTrainer,
        ActivityKind.WiSoSimulation => AppSection.WiSoTrainer,
        ActivityKind.MockTeil1 or ActivityKind.MockTeil2 => AppSection.Probeklausuren,
        ActivityKind.WeakTopicReview => AppSection.Fehlerbuch,
        ActivityKind.MixedRetrieval => AppSection.Aufgabentrainer,
        _ => AppSection.Dashboard
    };
}

public static class GreetingClock
{
    public static string LocKey(int hour24) => hour24 switch
    {
        >= 5 and < 12 => "Dash.Morning",
        >= 12 and < 18 => "Dash.Afternoon",
        _ => "Dash.Evening"
    };

    public static string Initials(string? displayName)
    {
        var parts = (displayName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return "AP";
        if (parts.Length == 1) return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();
        return string.Concat(parts[0][0], parts[^1][0]).ToUpperInvariant();
    }
}

public static class ShellSearch
{
    public static IReadOnlyList<T> Filter<T>(IEnumerable<T> items, string? query, Func<T, string> text)
    {
        var q = (query ?? "").Trim();
        if (q.Length == 0) return items.ToList();
        return items.Where(i => text(i).Contains(q, StringComparison.CurrentCultureIgnoreCase)).ToList();
    }
}

public static class TopicHeatBand
{
    public static string LocKey(double mastery) => mastery switch
    {
        < 0.34 => "Dash.HeatWeak",
        < 0.67 => "Dash.HeatMid",
        _ => "Dash.HeatStrong"
    };

    public static int Column(double mastery) => mastery switch
    {
        < 0.25 => 0,
        < 0.5 => 1,
        < 0.75 => 2,
        _ => 3
    };
}
