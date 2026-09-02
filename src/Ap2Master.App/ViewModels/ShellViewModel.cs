using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ap2Master.App.Localization;
using Ap2Master.App.Services;
using Ap2Master.Application.Abstractions;
using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.Domain.Ui;
using Ap2Master.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ap2Master.App.ViewModels;

public sealed partial class NavItem : ObservableObject
{
    public AppSection Section { get; init; }
    public string TitleKey { get; init; } = "";
    public string Glyph { get; init; } = "";
    [ObservableProperty] private string title = "";
    [ObservableProperty] private bool isSelected;
}

public sealed partial class NavGroupVm : ObservableObject
{
    public string TitleKey { get; init; } = "";
    [ObservableProperty] private string title = "";
    public ObservableCollection<NavItem> Items { get; } = [];
}

public sealed partial class SearchHitVm : ObservableObject
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public AppSection Section { get; init; }
    public object? Parameter { get; init; }
}

public sealed partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _nav;
    private readonly AppDbContext _db;
    private readonly IInboxService _inbox;
    private readonly ThemeService _theme;
    private CancellationTokenSource? _searchCts;

    public ObservableCollection<NavItem> Items { get; } = [];
    public ObservableCollection<NavGroupVm> Groups { get; } = [];
    public ObservableCollection<SearchHitVm> SearchHits { get; } = [];
    public ObservableCollection<InboxItem> Inbox { get; } = [];

    [ObservableProperty] private object? current;
    [ObservableProperty] private bool sidebarCollapsed;
    [ObservableProperty] private string autosave = Loc.T("App.Ready");
    [ObservableProperty] private string title = Loc.T("App.Title");
    [ObservableProperty] private string searchQuery = "";
    [ObservableProperty] private bool searchOpen;
    [ObservableProperty] private bool inboxOpen;
    [ObservableProperty] private string displayName = "Lernende";
    [ObservableProperty] private string initials = "AP";
    [ObservableProperty] private string countdown = "";
    [ObservableProperty] private int inboxCount;
    [ObservableProperty] private string dbStatus = "SQLite";
    [ObservableProperty] private bool isDark = true;
    [ObservableProperty] private string pageTitle = "";

    public MainViewModel(INavigationService nav, AppDbContext db, IInboxService inbox, ThemeService theme)
    {
        _nav = nav;
        _db = db;
        _inbox = inbox;
        _theme = theme;
        BuildNav();
        Loc.Current.LanguageChanged += (_, _) => RefreshTitles();
        _nav.CurrentChanged += (_, _) =>
        {
            Current = _nav.CurrentViewModel;
            PageTitle = Loc.T(Items.FirstOrDefault(i => i.Section == _nav.Current)?.TitleKey ?? "Nav.Dashboard");
            foreach (var i in Items) i.IsSelected = i.Section == _nav.Current;
            _ = RefreshInboxAsync();
        };
        Current = _nav.CurrentViewModel;
        _theme.ThemeChanged += (_, e) => IsDark = e.Resolved == ThemeKind.Dark;
        _ = BootstrapAsync();
    }

    private void BuildNav()
    {
        Items.Clear();
        Groups.Clear();
        foreach (var g in ShellNavigation.Groups)
        {
            var vm = new NavGroupVm { TitleKey = g.TitleKey, Title = Loc.T(g.TitleKey) };
            foreach (var e in g.Items)
            {
                var item = new NavItem { Section = e.Section, TitleKey = e.TitleKey, Glyph = e.Glyph, Title = Loc.T(e.TitleKey) };
                Items.Add(item);
                vm.Items.Add(item);
            }
            Groups.Add(vm);
        }
    }

    private void RefreshTitles()
    {
        Title = Loc.T("App.Title");
        foreach (var g in Groups) g.Title = Loc.T(g.TitleKey);
        foreach (var i in Items) i.Title = Loc.T(i.TitleKey);
        PageTitle = Loc.T(Items.FirstOrDefault(i => i.IsSelected)?.TitleKey ?? "Nav.Dashboard");
    }

    private async Task BootstrapAsync()
    {
        var s = await _db.UserSettings.AsNoTracking().FirstAsync();
        DisplayName = s.DisplayName;
        Initials = GreetingClock.Initials(s.DisplayName);
        SidebarCollapsed = s.SidebarCollapsed;
        IsDark = _theme.ResolvedTheme == ThemeKind.Dark;
        Countdown = s.ExamDate is { } d
            ? Loc.T("Shell.Countdown", Math.Max(0, (int)(d.Date - DateTime.Today).TotalDays))
            : Loc.T("Shell.NoExam");
        Autosave = Loc.T("App.Ready");
        await RefreshInboxAsync();
    }

    [RelayCommand]
    private void Navigate(NavItem? item)
    {
        if (item is null) return;
        SearchOpen = false;
        InboxOpen = false;
        _nav.Navigate(item.Section);
    }

    [RelayCommand]
    private void NavigateSection(object? parameter)
    {
        if (parameter is not AppSection section) return;
        SearchOpen = false;
        InboxOpen = false;
        _nav.Navigate(section);
    }

    [RelayCommand]
    private async Task ToggleSidebarAsync()
    {
        SidebarCollapsed = !SidebarCollapsed;
        var s = await _db.UserSettings.FirstAsync();
        s.SidebarCollapsed = SidebarCollapsed;
        await _db.SaveChangesAsync();
    }

    [RelayCommand]
    private async Task ToggleThemeAsync()
    {
        var next = _theme.ResolvedTheme == ThemeKind.Dark ? ThemeKind.Light : ThemeKind.Dark;
        var s = await _db.UserSettings.FirstAsync();
        s.Theme = next;
        await _db.SaveChangesAsync();
        _theme.Apply(s);
        IsDark = next == ThemeKind.Dark;
    }

    [RelayCommand]
    private void OpenSettings() => _nav.Navigate(AppSection.Einstellungen);

    [RelayCommand]
    private void ToggleInbox()
    {
        InboxOpen = !InboxOpen;
        SearchOpen = false;
        if (InboxOpen) _ = RefreshInboxAsync();
    }

    [RelayCommand]
    private void OpenInboxItem(InboxItem? item)
    {
        if (item is null) return;
        InboxOpen = false;
        _nav.Navigate(item.Section);
    }

    [RelayCommand]
    private void FocusSearch() => SearchOpen = true;

    [RelayCommand]
    private void OpenHit(SearchHitVm? hit)
    {
        if (hit is null) return;
        SearchOpen = false;
        SearchQuery = "";
        _nav.Navigate(hit.Section, hit.Parameter);
    }

    partial void OnSearchQueryChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;
        _ = DebounceSearchAsync(value, token);
    }

    private async Task DebounceSearchAsync(string value, CancellationToken token)
    {
        try
        {
            await Task.Delay(FocusTheme.SearchDebounce, token);
            await FilterSearchAsync(value, token);
        }
        catch (TaskCanceledException)
        {
            // superseded
        }
    }

    private async Task FilterSearchAsync(string value, CancellationToken token)
    {
        var q = value.Trim();
        SearchOpen = q.Length > 0;
        var hits = new List<SearchHitVm>();
        foreach (var item in ShellSearch.Filter(Items, q, i => i.Title + " " + Loc.T(i.TitleKey)))
            hits.Add(new SearchHitVm { Title = item.Title, Subtitle = Loc.T("Shell.SearchNav"), Section = item.Section });
        if (q.Length >= 2)
        {
            var topics = await _db.Topics.AsNoTracking()
                .Where(t => t.Title.Contains(q))
                .OrderBy(t => t.SortOrder)
                .Take(12)
                .ToListAsync(token);
            foreach (var t in topics)
                hits.Add(new SearchHitVm { Title = t.Title, Subtitle = Loc.T("Shell.SearchTopic"), Section = AppSection.Themenkatalog, Parameter = t.Id });
        }
        SearchHits.Clear();
        foreach (var h in hits.Take(16)) SearchHits.Add(h);
    }

    private async Task RefreshInboxAsync()
    {
        var list = await _inbox.GetAsync();
        Inbox.Clear();
        foreach (var i in list) Inbox.Add(i);
        InboxCount = list.Count;
    }
}
