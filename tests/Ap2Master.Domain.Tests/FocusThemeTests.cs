using Ap2Master.Domain;
using Ap2Master.Domain.Ui;
using FluentAssertions;

namespace Ap2Master.Domain.Tests;

public class FocusThemeTests
{
    [Fact]
    public void System_follows_windows_light_and_high_contrast()
    {
        FocusTheme.Resolve(ThemeKind.System, windowsLight: true, highContrast: false).Should().Be(ThemeKind.Light);
        FocusTheme.Resolve(ThemeKind.System, windowsLight: false, highContrast: false).Should().Be(ThemeKind.Dark);
        FocusTheme.Resolve(ThemeKind.System, windowsLight: true, highContrast: true).Should().Be(ThemeKind.HighContrast);
        FocusTheme.Resolve(ThemeKind.Dark, windowsLight: true, highContrast: true).Should().Be(ThemeKind.Dark);
    }

    [Fact]
    public void Palettes_share_semantic_and_legacy_keys()
    {
        foreach (var map in new[] { FocusTheme.Dark, FocusTheme.Light, FocusTheme.HighContrast })
        {
            map.Should().ContainKey("AppBackgroundBrush");
            map.Should().ContainKey("PrimaryBrush");
            map.Should().ContainKey("BgBrush");
            map.Should().ContainKey("AccentBrush");
            map["AppBackgroundBrush"].Should().Be(map["BgBrush"]);
            map["PrimaryBrush"].Should().Be(map["AccentBrush"]);
        }
        FocusTheme.Dark["PrimaryBrush"].Should().NotBe(FocusTheme.Light["PrimaryBrush"]);
        FocusTheme.SearchDebounce.TotalMilliseconds.Should().Be(250);
    }
}

public class ShellNavigationTests
{
    [Fact]
    public void All_seventeen_sections_remain_grouped()
    {
        ShellNavigation.AllSections.Should().HaveCount(17);
        ShellNavigation.AllSections.Should().OnlyHaveUniqueItems();
        ShellNavigation.AllSections.Should().Contain(AppSection.Fehlerbuch);
        ShellNavigation.AllSections.Should().Contain(AppSection.SqlLabor);
        ShellNavigation.Groups.Should().HaveCount(5);
    }

    [Fact]
    public void Search_filters_without_inventing_entries()
    {
        var items = new[] { "Dashboard", "SQL-Labor", "Einstellungen" };
        ShellSearch.Filter(items, "sql", x => x).Should().Equal("SQL-Labor");
        ShellSearch.Filter(items, "", x => x).Should().HaveCount(3);
    }

    [Fact]
    public void Initials_and_greeting_are_stable()
    {
        GreetingClock.Initials("Max Schulz").Should().Be("MS");
        GreetingClock.Initials("Ivan").Should().Be("IV");
        GreetingClock.LocKey(8).Should().Be("Dash.Morning");
        GreetingClock.LocKey(15).Should().Be("Dash.Afternoon");
        GreetingClock.LocKey(21).Should().Be("Dash.Evening");
    }
}
