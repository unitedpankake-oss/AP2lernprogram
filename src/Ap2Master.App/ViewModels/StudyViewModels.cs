using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ap2Master.App.Localization;
using Ap2Master.App.Services;
using Ap2Master.Application.Abstractions;
using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.Domain.Learning;
using Ap2Master.Domain.Ui;
using Ap2Master.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ap2Master.App.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject, INavigationAware
{
    private readonly IDashboardService _dash;
    private readonly INavigationService _nav;
    [ObservableProperty] private DashboardSnapshot? snapshot;
    [ObservableProperty] private string status = Loc.T("Dash.Loading");
    [ObservableProperty] private string greeting = "";
    [ObservableProperty] private string greetingSub = "";
    [ObservableProperty] private ObservableCollection<PlannedActivity> todayItems = [];
    public DashboardViewModel(IDashboardService dash, INavigationService nav) { _dash = dash; _nav = nav; }
    public async void OnNavigated(object? parameter) => await LoadAsync();
    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            Status = Loc.T("Dash.Loading");
            Snapshot = await _dash.GetAsync();
            var name = Snapshot.LearnerName;
            Greeting = Loc.T(GreetingClock.LocKey(DateTime.Now.Hour), name);
            GreetingSub = Snapshot.DaysUntilExam is int d
                ? Loc.T("Dash.DaysLeft", d)
                : Loc.T("Shell.NoExam");
            TodayItems = new ObservableCollection<PlannedActivity>(
                Snapshot.Upcoming.Where(a => a.Date == DateOnly.FromDateTime(DateTime.Today)).Take(4));
            if (TodayItems.Count == 0)
                TodayItems = new ObservableCollection<PlannedActivity>(Snapshot.Upcoming.Take(3));
            Status = Loc.T("Dash.Disclaimer");
        }
        catch (Exception ex) { Status = Loc.T("Dash.Error", ex.Message); }
    }

    [RelayCommand]
    private void StartTraining()
    {
        var first = TodayItems.FirstOrDefault();
        if (first is not null && ShellNavigation.MapActivity(first.Kind) is { } section)
            _nav.Navigate(section, string.IsNullOrWhiteSpace(first.TopicId) ? null : first.TopicId);
        else
            _nav.Navigate(AppSection.Lernkarten);
    }

    [RelayCommand] private void OpenCards() => _nav.Navigate(AppSection.Lernkarten);
    [RelayCommand] private void OpenPlan() => _nav.Navigate(AppSection.Lernplan);
    [RelayCommand] private void OpenStats() => _nav.Navigate(AppSection.Statistiken);
    [RelayCommand] private void OpenCatalog() => _nav.Navigate(AppSection.Themenkatalog);
    [RelayCommand] private void OpenTopic(TopicHeatCell? cell)
    {
        if (cell is null) return;
        _nav.Navigate(AppSection.Themenkatalog, cell.TopicId);
    }
}

public sealed partial class PlanBlockVm : ObservableObject
{
    public PlanBlockVm(PlannedActivity source)
    {
        Id = source.Id;
        Source = source;
        Title = source.Title;
        Date = source.Date;
        Minutes = source.EstimatedMinutes;
        IsCompleted = source.IsCompleted;
        Kind = source.Kind;
        var (accent, soft) = TopicPalette.ForActivity(source.Kind, source.TopicId);
        AccentHex = accent;
        SoftHex = soft;
    }

    public Guid Id { get; }
    public PlannedActivity Source { get; }
    public string Title { get; }
    public DateOnly Date { get; }
    public int Minutes { get; }
    public ActivityKind Kind { get; }
    public string AccentHex { get; }
    public string SoftHex { get; }
    [ObservableProperty] private bool isCompleted;
    [ObservableProperty] private bool isSelected;
}

public sealed partial class WeekDayVm : ObservableObject
{
    public WeekDayVm(DateOnly date, string dayKey, bool isToday, IEnumerable<PlanBlockVm> blocks)
    {
        Date = date;
        DayKey = dayKey;
        IsToday = isToday;
        DateLabel = date.ToString("dd.MM");
        Blocks = new ObservableCollection<PlanBlockVm>(blocks);
    }

    public DateOnly Date { get; }
    public string DayKey { get; }
    public string DateLabel { get; }
    public bool IsToday { get; }
    public ObservableCollection<PlanBlockVm> Blocks { get; }
}

public sealed partial class LernplanViewModel : ObservableObject, INavigationAware
{
    private static readonly string[] DayKeys = ["Plan.Mo", "Plan.Tu", "Plan.We", "Plan.Th", "Plan.Fr", "Plan.Sa", "Plan.Su"];
    private readonly AppDbContext _db;
    private readonly ILearningPlanService _plans;
    [ObservableProperty] private DateTime examDate = DateTime.Today.AddDays(90);
    [ObservableProperty] private int minutes = 60;
    [ObservableProperty] private string weekdays = "1,2,3,4,5";
    [ObservableProperty] private int target = 75;
    [ObservableProperty] private bool isFiae = true;
    [ObservableProperty] private string level = "mittel";
    [ObservableProperty] private string difficult = "SQL,UML,WiSo";
    [ObservableProperty] private ObservableCollection<PlannedActivity> activities = [];
    [ObservableProperty] private ObservableCollection<WeekDayVm> days = [];
    [ObservableProperty] private DateOnly weekStart = MondayOf(DateOnly.FromDateTime(DateTime.Today));
    [ObservableProperty] private string weekLabel = "";
    [ObservableProperty] private string status = "";
    [ObservableProperty] private PlanBlockVm? selectedBlock;

    public LernplanViewModel(AppDbContext db, ILearningPlanService plans) { _db = db; _plans = plans; }
    public async void OnNavigated(object? parameter) => await RefreshAsync();

    [RelayCommand]
    public async Task CreateAsync()
    {
        var s = await _db.UserSettings.FirstAsync();
        s.ExamDate = ExamDate;
        s.MinutesPerDay = Minutes;
        s.AvailableWeekdaysCsv = Weekdays;
        s.TargetScore = Target;
        s.IsFiae = IsFiae;
        s.KnowledgeLevel = Level;
        s.DifficultSubjectsCsv = Difficult;
        s.OnboardingCompleted = true;
        await _db.SaveChangesAsync();
        await _plans.CreateOrRebuildAsync(s);
        await _plans.RescheduleMissedAsync();
        Status = Loc.T("Plan.Created");
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task CompleteAsync(PlanBlockVm? a)
    {
        if (a is null || a.IsCompleted) return;
        await _plans.CompleteAsync(a.Id);
        await RefreshAsync();
    }

    [RelayCommand] private async Task PrevWeek() { WeekStart = WeekStart.AddDays(-7); await RefreshWeekAsync(); }
    [RelayCommand] private async Task NextWeek() { WeekStart = WeekStart.AddDays(7); await RefreshWeekAsync(); }

    [RelayCommand]
    private async Task MoveSelectedLeft() => await MoveSelectedAsync(-1);

    [RelayCommand]
    private async Task MoveSelectedRight() => await MoveSelectedAsync(1);

    public void SelectBlock(PlanBlockVm? block)
    {
        SelectedBlock = block;
        foreach (var day in Days)
        foreach (var b in day.Blocks)
            b.IsSelected = block is not null && b.Id == block.Id;
    }

    public async Task MoveToAsync(Guid id, DateOnly date)
    {
        await _plans.MoveAsync(id, date);
        if (date < WeekStart || date > WeekStart.AddDays(6))
            WeekStart = MondayOf(date);
        await RefreshWeekAsync();
    }

    private async Task MoveSelectedAsync(int delta)
    {
        if (SelectedBlock is null) return;
        await MoveToAsync(SelectedBlock.Id, SelectedBlock.Date.AddDays(delta));
    }

    private async Task RefreshAsync()
    {
        var s = await _db.UserSettings.FirstAsync();
        ExamDate = s.ExamDate ?? ExamDate;
        Minutes = s.MinutesPerDay;
        var list = await _db.PlannedActivities.OrderBy(x => x.Date).ThenBy(x => x.SortOrder).Take(80).ToListAsync();
        Activities = new ObservableCollection<PlannedActivity>(list);
        if (list.Count == 0) Status = Loc.T("Plan.Empty");
        await RefreshWeekAsync();
    }

    private async Task RefreshWeekAsync()
    {
        WeekLabel = Loc.T("Plan.Week", WeekStart.ToString("dd.MM"), WeekStart.AddDays(6).ToString("dd.MM"));
        var from = WeekStart;
        var to = WeekStart.AddDays(6);
        var list = await _db.PlannedActivities
            .Where(x => x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date).ThenBy(x => x.SortOrder)
            .ToListAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var selectedId = SelectedBlock?.Id;
        Days = new ObservableCollection<WeekDayVm>(
            Enumerable.Range(0, 7).Select(i =>
            {
                var date = WeekStart.AddDays(i);
                var blocks = list.Where(a => a.Date == date).Select(a => new PlanBlockVm(a)).ToList();
                foreach (var b in blocks) b.IsSelected = selectedId is Guid g && b.Id == g;
                return new WeekDayVm(date, DayKeys[i], date == today, blocks);
            }));
        if (selectedId is Guid id)
            SelectedBlock = Days.SelectMany(d => d.Blocks).FirstOrDefault(b => b.Id == id);
    }

    private static DateOnly MondayOf(DateOnly date)
    {
        var offset = date.DayOfWeek == DayOfWeek.Sunday ? -6 : (int)DayOfWeek.Monday - (int)date.DayOfWeek;
        return date.AddDays(offset);
    }
}

public sealed partial class KatalogViewModel : ObservableObject, INavigationAware
{
    private readonly AppDbContext _db;
    private ObservableCollection<PartBoardVm> _allBoards = [];
    public ObservableCollection<PartChipVm> Parts { get; } = [];
    public ObservableCollection<PartBoardVm> VisibleBoards { get; } = [];
    [ObservableProperty] private TopicCardVm? selected;
    [ObservableProperty] private string theory = Loc.T("Cat.Pick");
    [ObservableProperty] private string theoryTitle = "";
    [ObservableProperty] private string accentHex = "#3B82F6";

    public KatalogViewModel(AppDbContext db)
    {
        _db = db;
        Loc.Current.LanguageChanged += (_, _) =>
        {
            TopicBoardUi.RefreshTitles(Parts, _allBoards);
            if (Selected is null) Theory = Loc.T("Cat.Pick");
        };
    }

    public async void OnNavigated(object? parameter)
    {
        var topics = await _db.Topics.AsNoTracking().OrderBy(t => t.SortOrder).ToListAsync();
        var built = TopicBoardBuilder.Build(topics);
        _allBoards = TopicBoardUi.Map(built);
        Parts.Clear();
        foreach (var chip in TopicBoardUi.Chips(built)) Parts.Add(chip);
        ApplyPartFilter(null);
        if (parameter is string id)
        {
            var card = TopicBoardUi.AllCards(_allBoards).FirstOrDefault(c => c.Id == id);
            if (card is not null) SelectTopic(card);
        }
    }

    [RelayCommand]
    private void SelectPart(PartChipVm? chip)
    {
        if (chip is null) return;
        ApplyPartFilter(chip.Part);
    }

    [RelayCommand]
    private void SelectTopic(TopicCardVm? card)
    {
        if (card is null) return;
        foreach (var existing in TopicBoardUi.AllCards(_allBoards)) existing.IsSelected = existing.Id == card.Id;
        Selected = card;
        AccentHex = card.AccentHex;
        TheoryTitle = card.Title;
        var c = _db.LearningContents.AsNoTracking()
            .FirstOrDefault(x => x.TopicId == card.Id && x.ContentType == ContentType.Theory);
        Theory = c?.Answer is { Length: > 0 } a && a != (c.Question ?? "")
            ? a
            : c?.Question ?? card.Title;
    }

    private void ApplyPartFilter(ExamPart? part)
    {
        foreach (var chip in Parts) chip.IsSelected = chip.Part == part;
        VisibleBoards.Clear();
        foreach (var board in _allBoards.Where(b => part is null || b.Part == part))
            VisibleBoards.Add(board);
    }
}

public sealed partial class LernkartenViewModel : ObservableObject, INavigationAware
{
    private readonly IFlashcardService _cards;
    private ObservableCollection<PartBoardVm> _allBoards = [];
    private List<Flashcard> _queue = [];
    private IReadOnlyList<string>? _scopeIds;
    private int _index;
    private DateTime _shown;
    private FlashcardQueueKind _kind = FlashcardQueueKind.Due;

    public ObservableCollection<PartChipVm> Parts { get; } = [];
    public ObservableCollection<PartBoardVm> VisibleBoards { get; } = [];

    [ObservableProperty] private Flashcard? current;
    [ObservableProperty] private bool showBack;
    [ObservableProperty] private string notes = "";
    [ObservableProperty] private string empty = "";
    [ObservableProperty] private string progress = "";
    [ObservableProperty] private string sessionTitle = "";
    [ObservableProperty] private string accentHex = "#3B82F6";
    [ObservableProperty] private string softHex = "#333B82F6";
    [ObservableProperty] private bool studying;
    [ObservableProperty] private TopicCardVm? selectedDeck;

    public LernkartenViewModel(IFlashcardService cards)
    {
        _cards = cards;
        Loc.Current.LanguageChanged += (_, _) =>
        {
            TopicBoardUi.RefreshTitles(Parts, _allBoards);
            if (_queue.Count == 0 && Current is null) Empty = Loc.T("Cards.Pick");
            RefreshProgress();
        };
    }

    public async void OnNavigated(object? parameter)
    {
        await RefreshBoardAsync();
        if (parameter is string id)
        {
            var card = TopicBoardUi.AllCards(_allBoards).FirstOrDefault(c => c.Id == id);
            if (card is not null) await OpenDeckAsync(card);
            else await StartAsync(FlashcardQueueKind.Due, [id], Loc.T("Cards.Due"));
        }
        else
        {
            Empty = Loc.T("Cards.Pick");
            Studying = false;
        }
    }

    [RelayCommand]
    private void SelectPart(PartChipVm? chip)
    {
        if (chip is null) return;
        ApplyPartFilter(chip.Part);
        _scopeIds = SelectedPartScope();
    }

    [RelayCommand]
    private async Task OpenDeckAsync(TopicCardVm? card)
    {
        if (card is null) return;
        MarkSelected(card);
        SelectedDeck = card;
        AccentHex = card.AccentHex;
        SoftHex = card.SoftHex;
        await StartAsync(FlashcardQueueKind.Due, card.ScopeIds, card.Title);
    }

    [RelayCommand]
    private async Task StudyClusterAsync(ClusterBandVm? band)
    {
        if (band is null) return;
        SelectedDeck = null;
        AccentHex = band.AccentHex;
        SoftHex = band.SoftHex;
        await StartAsync(FlashcardQueueKind.Due, band.ScopeIds, band.Title);
    }

    [RelayCommand]
    private async Task StudyPartAsync(PartBoardVm? board)
    {
        if (board is null) return;
        SelectedDeck = null;
        AccentHex = board.AccentHex;
        SoftHex = board.SoftHex;
        await StartAsync(FlashcardQueueKind.Due, board.ScopeIds, board.Title);
    }

    [RelayCommand] private Task LoadDueAsync() => StartAsync(FlashcardQueueKind.Due, CurrentScope(), Loc.T("Cards.Due"));
    [RelayCommand] private Task LoadRandomAsync() => StartAsync(FlashcardQueueKind.Random, CurrentScope(), Loc.T("Cards.Random"));
    [RelayCommand] private Task LoadWeakAsync() => StartAsync(FlashcardQueueKind.Weak, CurrentScope(), Loc.T("Cards.Weak"));

    [RelayCommand] private void Flip() => ShowBack = !ShowBack;

    [RelayCommand]
    private async Task Rate(string? rating)
    {
        if (Current is null || !Enum.TryParse<FlashcardRating>(rating, out var r)) return;
        var sec = Math.Max(1, (int)(DateTime.UtcNow - _shown).TotalSeconds);
        await _cards.ReviewAsync(Current.Id, r, sec);
        _index++;
        Next();
    }

    [RelayCommand] private async Task Favorite() { if (Current is not null) await _cards.ToggleFavoriteAsync(Current.Id); }
    [RelayCommand] private async Task SaveNote() { if (Current is not null) await _cards.SaveNotesAsync(Current.Id, Notes); }

    [RelayCommand]
    private void CloseSession()
    {
        Studying = false;
        Current = null;
        _scopeIds = SelectedPartScope();
        Empty = Loc.T("Cards.Pick");
        Progress = "";
        SessionTitle = "";
    }

    private IReadOnlyList<string>? CurrentScope()
        => _scopeIds is { Count: > 0 } ? _scopeIds : SelectedPartScope();

    private IReadOnlyList<string>? SelectedPartScope()
    {
        var chip = Parts.FirstOrDefault(p => p.IsSelected);
        if (chip?.Part is null) return null;
        return _allBoards.FirstOrDefault(b => b.Part == chip.Part)?.ScopeIds;
    }

    private async Task StartAsync(FlashcardQueueKind kind, IReadOnlyList<string>? scope, string title)
    {
        _kind = kind;
        _scopeIds = scope is { Count: > 0 } ? scope : null;
        SessionTitle = title;
        _queue = (await _cards.BuildQueueAsync(kind, _scopeIds)).ToList();
        if (_queue.Count == 0 && kind == FlashcardQueueKind.Due)
            _queue = (await _cards.BuildQueueAsync(FlashcardQueueKind.Random, _scopeIds, 15)).ToList();
        _index = 0;
        Studying = true;
        Next();
    }

    private async Task RefreshBoardAsync()
    {
        var boards = await _cards.GetBoardsAsync();
        _allBoards = TopicBoardUi.Map(boards);
        Parts.Clear();
        foreach (var chip in TopicBoardUi.Chips(boards)) Parts.Add(chip);
        ApplyPartFilter(null);
    }

    private void ApplyPartFilter(ExamPart? part)
    {
        foreach (var chip in Parts) chip.IsSelected = chip.Part == part;
        VisibleBoards.Clear();
        foreach (var board in _allBoards.Where(b => part is null || b.Part == part))
            VisibleBoards.Add(board);
    }

    private void MarkSelected(TopicCardVm card)
    {
        foreach (var existing in TopicBoardUi.AllCards(_allBoards)) existing.IsSelected = existing.Id == card.Id;
    }

    private void Next()
    {
        ShowBack = false;
        if (_index >= _queue.Count)
        {
            Current = null;
            Empty = Loc.T("Cards.Empty");
            RefreshProgress();
            return;
        }
        Current = _queue[_index];
        Notes = Current.PersonalNotes ?? "";
        Empty = "";
        _shown = DateTime.UtcNow;
        RefreshProgress();
    }

    private void RefreshProgress()
        => Progress = _queue.Count == 0 ? "" : Loc.T("Cards.Progress", Math.Min(_index + 1, _queue.Count), _queue.Count);
}

public sealed partial class AufgabenViewModel : ObservableObject, INavigationAware
{
    private readonly AppDbContext _db;
    private readonly IExerciseGenerator _gen;
    private readonly IProgressTracker _progress;
    private readonly IMistakeBookService _mistakes;
    [ObservableProperty] private ObservableCollection<ExerciseTemplate> templates = [];
    [ObservableProperty] private ExerciseTemplate? selected;
    [ObservableProperty] private int seed = 42;
    [ObservableProperty] private GeneratedExercise? exercise;
    [ObservableProperty] private string answer = "";
    [ObservableProperty] private string feedback = "";
    [ObservableProperty] private bool showHint;

    public AufgabenViewModel(AppDbContext db, IExerciseGenerator gen, IProgressTracker progress, IMistakeBookService mistakes)
    { _db = db; _gen = gen; _progress = progress; _mistakes = mistakes; }
    public async void OnNavigated(object? parameter)
        => Templates = new(await _db.ExerciseTemplates.AsNoTracking().ToListAsync());

    [RelayCommand]
    private void Generate()
    {
        if (Selected is null) return;
        Exercise = _gen.Generate(Selected, Seed);
        Answer = ""; Feedback = ""; ShowHint = false;
    }

    [RelayCommand]
    private async Task Check()
    {
        if (Exercise is null) return;
        var ok = string.Equals(Answer.Trim(), Exercise.Solution.Trim(), StringComparison.OrdinalIgnoreCase)
                 || (double.TryParse(Answer.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var n)
                     && double.TryParse(Exercise.Solution.Replace(",", ".").Replace("%","").Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var e)
                     && Math.Abs(n - e) < 0.05);
        Feedback = ok ? Loc.T("Ex.Ok") : Loc.T("Ex.Bad", Exercise.Solution, Exercise.Explanation);
        await _progress.RecordAnswerAsync(new UserAnswer
        {
            Id = Guid.NewGuid(), TopicId = Exercise.TopicId, Difficulty = Exercise.Difficulty,
            AnsweredAt = DateTime.UtcNow, LearnerAnswer = Answer, CorrectAnswer = Exercise.Solution,
            IsCorrect = ok, PointsAwarded = ok ? Exercise.Points : 0, PointsPossible = Exercise.Points, SecondsSpent = 60
        });
        if (!ok)
            await _mistakes.AddAsync(new MistakeEntry
            {
                Id = Guid.NewGuid(), TopicId = Exercise.TopicId, TaskText = Exercise.TaskText, RandomSeed = Exercise.Seed,
                LearnerAnswer = Answer, CorrectAnswer = Exercise.Solution, Explanation = Exercise.Explanation,
                Difficulty = Exercise.Difficulty, OccurredAt = DateTime.UtcNow, MistakeType = MistakeType.Rechenfehler,
                TemplateId = Exercise.TemplateId
            });
    }
}
