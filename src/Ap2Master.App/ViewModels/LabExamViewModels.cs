using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ap2Master.App.Localization;
using Ap2Master.App.Services;
using Ap2Master.Application.Abstractions;
using Ap2Master.Domain;
using Ap2Master.Domain.Coverage;
using Ap2Master.Domain.Entities;
using Ap2Master.Domain.ProjectManagement;
using Ap2Master.Domain.Ui;
using Ap2Master.Infrastructure.Persistence;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;

namespace Ap2Master.App.ViewModels;

public sealed partial class PseudocodeViewModel : ObservableObject, INavigationAware
{
    private readonly IPseudocodeEngine _engine;
    [ObservableProperty] private string code = "SET max = liste[0]\nFOR i FROM 1 TO n-1\nIF liste[i] > max THEN\nSET max = liste[i]\nENDIF\nENDFOR\nRETURN max";
    [ObservableProperty] private string inputListe = "3,9,2,-1";
    [ObservableProperty] private string output = "";
    public void OnNavigated(object? parameter) { }
    public PseudocodeViewModel(IPseudocodeEngine engine) => _engine = engine;

    [RelayCommand]
    private void Run()
    {
        var list = InputListe.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
        var r = _engine.Run(Code, new Dictionary<string, object?> { ["liste"] = list, ["n"] = list.Count });
        Output = r.Success ? Loc.T("Pc.Return", r.ReturnValue ?? "", string.Join(", ", r.Variables.Select(kv => kv.Key+"="+kv.Value))) : r.Error ?? "";
        var trace = _engine.Trace(Code, new Dictionary<string, object?> { ["liste"] = list, ["n"] = list.Count });
        Output += "\n\n" + Loc.T("Pc.Steps") + "\n" + string.Join("\n", trace.Take(30).Select(s => $"{s.Line}: {s.Description}"));
    }
}

public sealed partial class UmlViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty] private string classA = "Kunde";
    [ObservableProperty] private string classB = "Ticket";
    [ObservableProperty] private string rel = "Komposition";
    [ObservableProperty] private string multiA = "1";
    [ObservableProperty] private string multiB = "0..*";
    [ObservableProperty] private string preview = "";
    [ObservableProperty] private string feedback = "";
    public void OnNavigated(object? parameter) => Update();
    [RelayCommand] private void Update()
    {
        Preview = $"+----------------+\n|  {ClassA}  |\n+----------------+\n        {MultiA}\n        {(Rel == "Komposition" ? "◆" : Rel == "Aggregation" ? "◇" : Rel == "Vererbung" ? "△" : "—")}\n        {MultiB}\n+----------------+\n|  {ClassB}  |\n+----------------+";
        Feedback = Rel == "Komposition" && MultiA is "0..*" or "*"
            ? Loc.T("Uml.CompErr")
            : Rel == "Vererbung" ? Loc.T("Uml.Inh") : Loc.T("Uml.Ok");
    }
}

public sealed partial class ProjektViewModel : ObservableObject, INavigationAware
{
    private readonly INetworkPlanTrainer _np;
    [ObservableProperty] private string input = "A;Analyse;3;\nB;Entwurf;4;A\nC;Testkonzept;2;A\nD;Umsetzung;5;B,C";
    [ObservableProperty] private string result = "";
    public ProjektViewModel(INetworkPlanTrainer np) => _np = np;
    public void OnNavigated(object? parameter) { }
    [RelayCommand]
    private void Calculate()
    {
        var acts = Input.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line =>
        {
            var p = line.Split(';');
            var preds = p.Length > 3 ? p[3].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : [];
            return new NetworkActivity(p[0].Trim(), p[1].Trim(), int.Parse(p[2]), preds);
        }).ToList();
        var r = _np.Calculate(acts);
        Result = Loc.T("Proj.Result", r.ProjectDuration, string.Join("–", r.CriticalPath)) + "\n" +
                 string.Join("\n", r.Activities.Select(a =>
                     $"{a.Id} {a.Name}: FAZ {a.Faz} FEZ {a.Fez} SAZ {a.Saz} SEZ {a.Sez} GP {a.GesamtPuffer} FP {a.FreierPuffer} {(a.IsCritical ? Loc.T("Proj.Critical") : "")}"));
    }
}

public sealed partial class CoverageViewModel : ObservableObject, INavigationAware
{
    private readonly ICoverageTrainer _cov;
    public ObservableCollection<PathChoice> Paths { get; } = [];
    [ObservableProperty] private string report = "";
    public CoverageViewModel(ICoverageTrainer cov)
    {
        _cov = cov;
        Paths.Add(new PathChoice("p-then", "then: s1→s2→s4"));
        Paths.Add(new PathChoice("p-else", "else: s1→s3→s4"));
    }
    public void OnNavigated(object? parameter) { }
    [RelayCommand]
    private void Evaluate()
    {
        var graph = ControlFlowGraph.IfThenElse();
        var selected = Paths.Where(p => p.Selected).Select(p => p.Id);
        var r = _cov.Evaluate(graph, selected);
        var min = _cov.MinimalSet(graph, "branch");
        Report = Loc.T("Cov.Report", r.StatementCoverage, r.BranchCoverage, r.ConditionCoverage, r.PathCoverage, string.Join(", ", min));
    }
}

public sealed partial class PathChoice : ObservableObject
{
    public PathChoice(string id, string title) { Id = id; Title = title; }
    public string Id { get; }
    public string Title { get; }
    [ObservableProperty] private bool selected;
}

public sealed partial class NetzSecurityViewModel : ObservableObject, INavigationAware
{
    public ObservableCollection<MatchItem> Items { get; } = [];
    [ObservableProperty] private string feedback = "";
    public void OnNavigated(object? parameter)
    {
        Items.Clear();
        Items.Add(new MatchItem("HTTP", "80/TCP", Loc.T("Netz.Port")));
        Items.Add(new MatchItem("HTTPS", "443/TCP", Loc.T("Netz.Port")));
        Items.Add(new MatchItem("SSH", "22/TCP", Loc.T("Netz.Port")));
        Items.Add(new MatchItem("DNS", "53/UDP+TCP", Loc.T("Netz.Port")));
        Items.Add(new MatchItem("Router", "OSI 3", Loc.T("Netz.Layer")));
        Items.Add(new MatchItem("Switch", "OSI 2", Loc.T("Netz.Layer")));
        Items.Add(new MatchItem("AES-GCM", "symmetrisch (AEAD)", Loc.T("Netz.Crypto")));
        Items.Add(new MatchItem("TLS 1.3", "ECDHE + symmetrische Session", Loc.T("Netz.Crypto")));
    }
    [RelayCommand]
    private void Check()
    {
        var ok = Items.Count(i => string.Equals(i.Guess.Trim(), i.Answer, StringComparison.OrdinalIgnoreCase));
        Feedback = Loc.T("Netz.Fb", ok, Items.Count);
    }
}

public sealed partial class MatchItem : ObservableObject
{
    public MatchItem(string prompt, string answer, string group) { Prompt = prompt; Answer = answer; Group = group; }
    public string Prompt { get; }
    public string Answer { get; }
    public string Group { get; }
    [ObservableProperty] private string guess = "";
}

public sealed partial class ExamOptionVm : ObservableObject
{
    public ExamOptionVm(string letter, string germanText)
    {
        Letter = letter;
        GermanText = germanText;
    }

    public string Letter { get; }
    public string GermanText { get; }
    [ObservableProperty] private bool isSelected;
}

public sealed partial class ExamNavCellVm : ObservableObject
{
    public ExamNavCellVm(int index, int number, ExamNavKind kind)
    {
        Index = index;
        Number = number;
        Apply(kind);
    }

    public int Index { get; }
    [ObservableProperty] private int number;
    [ObservableProperty] private ExamNavKind kind;
    [ObservableProperty] private string kindKey = "";
    [ObservableProperty] private string glyph = "";

    public void Apply(ExamNavKind next)
    {
        Kind = next;
        KindKey = next.ToString();
        Glyph = ExamNavigator.Glyph(next);
    }
}

public partial class ExamHostViewModel : ObservableObject, INavigationAware
{
    private readonly IExamEngine _engine;
    private readonly AppDbContext _db;
    private readonly IDialogService _dialog;
    private readonly DispatcherTimer _timer;
    [ObservableProperty] private string examId = "exam-wiso";
    [ObservableProperty] private ExamSession? session;
    [ObservableProperty] private LearningContent? currentQuestion;
    [ObservableProperty] private string remaining = "";
    [ObservableProperty] private string answer = "";
    [ObservableProperty] private string markedInfo = "";
    [ObservableProperty] private string result = "";
    [ObservableProperty] private bool examMode = true;
    [ObservableProperty] private string autosave = "";
    [ObservableProperty] private int durationMinutes;
    [ObservableProperty] private int questionCount;
    [ObservableProperty] private int answeredCount;
    [ObservableProperty] private int progressPercent;
    [ObservableProperty] private string examTitle = "";
    [ObservableProperty] private bool hasActiveExam;
    [ObservableProperty] private bool currentMarked;
    private List<string> _ids = [];
    private Dictionary<string, string> _answers = new();
    private HashSet<string> _marked = [];
    private bool _loadingQuestion;

    public ObservableCollection<ExamOptionVm> Options { get; } = [];
    public ObservableCollection<ExamNavCellVm> NavCells { get; } = [];
    public ObservableCollection<ExamDefinition> ExamChoices { get; } = [];
    public virtual bool ShowExamPicker => false;

    public ExamHostViewModel(IExamEngine engine, AppDbContext db, IDialogService dialog)
    {
        _engine = engine;
        _db = db;
        _dialog = dialog;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += async (_, _) => await Tick();
    }

    public async void OnNavigated(object? parameter)
    {
        if (parameter is string id) ExamId = id;
        if (ShowExamPicker)
        {
            ExamChoices.Clear();
            foreach (var def in await _db.ExamDefinitions.AsNoTracking().ToListAsync())
                ExamChoices.Add(def);
        }
    }

    [RelayCommand]
    public async Task StartAsync()
    {
        Session = await _engine.StartAsync(ExamId);
        await BindSessionAsync(resetMarked: true);
        Result = "";
        _timer.Start();
    }

    [RelayCommand]
    public async Task RestoreAsync()
    {
        var kind = ExamId switch
        {
            "exam-wiso" => ExamKind.WiSo,
            "exam-teil1" => ExamKind.Teil1,
            "exam-teil2" => ExamKind.Teil2,
            _ => ExamKind.Custom
        };
        Session = await _engine.RestoreOpenAsync(kind);
        if (Session is null)
        {
            Result = Loc.T("Exam.None");
            HasActiveExam = false;
            return;
        }
        await BindSessionAsync(resetMarked: false);
        _timer.Start();
    }

    [RelayCommand] private async Task Next() { PersistCurrent(); await ShowIndex(Math.Min(_ids.Count - 1, (Session?.CurrentIndex ?? 0) + 1)); }
    [RelayCommand] private async Task Prev() { PersistCurrent(); await ShowIndex(Math.Max(0, (Session?.CurrentIndex ?? 0) - 1)); }

    [RelayCommand]
    private void Mark()
    {
        if (CurrentQuestion is null) return;
        if (!_marked.Add(CurrentQuestion.Id)) _marked.Remove(CurrentQuestion.Id);
        CurrentMarked = _marked.Contains(CurrentQuestion.Id);
        PersistCurrent();
        RefreshNav();
    }

    [RelayCommand]
    private async Task Jump(ExamNavCellVm? cell)
    {
        if (cell is null) return;
        PersistCurrent();
        await ShowIndex(cell.Index);
    }

    [RelayCommand]
    private void SelectOption(ExamOptionVm? option)
    {
        if (option is null) return;
        foreach (var o in Options) o.IsSelected = o == option;
        Answer = option.GermanText;
        PersistCurrent();
        RefreshNav();
    }

    [RelayCommand]
    public async Task SubmitAsync()
    {
        if (Session is null) return;
        PersistCurrent();
        var unanswered = _ids.Count(id => !_answers.TryGetValue(id, out var a) || string.IsNullOrWhiteSpace(a));
        if (unanswered > 0 && !await _dialog.ConfirmAsync(Loc.T("Exam.WarnTitle"), Loc.T("Exam.Warn", unanswered)))
            return;
        _timer.Stop();
        var r = await _engine.SubmitAsync(Session.Id, false);
        Result = r.ReportText + "\n" + Loc.T("Exam.Time", r.ExpectedMinutes, r.ActualMinutes);
        Autosave = Loc.T("Exam.Submitted");
        HasActiveExam = false;
    }

    private async Task Tick()
    {
        if (Session is null || Session.IsSubmitted) return;
        Session.RemainingSeconds--;
        Remaining = TimeSpan.FromSeconds(Math.Max(0, Session.RemainingSeconds)).ToString(@"mm\:ss");
        if (Session.RemainingSeconds % 15 == 0)
        {
            PersistCurrent();
            await _engine.SaveAsync(Session);
            Autosave = Loc.T("Exam.Saved", DateTime.Now.ToString("HH:mm:ss"));
        }
        if (Session.RemainingSeconds <= 0)
        {
            _timer.Stop();
            var r = await _engine.SubmitAsync(Session.Id, true);
            Result = Loc.T("Exam.Timeout") + r.ReportText;
            Autosave = Loc.T("Exam.Submitted");
            HasActiveExam = false;
            await _dialog.AlertAsync(Loc.T("Exam.WarnTitle"), Loc.T("Exam.Timeout"));
        }
    }

    private void PersistCurrent()
    {
        if (Session is null || CurrentQuestion is null) return;
        if (string.IsNullOrWhiteSpace(Answer)) _answers.Remove(CurrentQuestion.Id);
        else _answers[CurrentQuestion.Id] = Answer;
        Session.AnswersJson = JsonSerializer.Serialize(_answers);
        Session.MarkedJson = JsonSerializer.Serialize(_marked);
    }

    private async Task BindSessionAsync(bool resetMarked)
    {
        if (Session is null) return;
        _ids = JsonSerializer.Deserialize<List<string>>(Session.QuestionIdsJson) ?? [];
        _answers = JsonSerializer.Deserialize<Dictionary<string, string>>(Session.AnswersJson) ?? new();
        _marked = resetMarked
            ? []
            : JsonSerializer.Deserialize<HashSet<string>>(Session.MarkedJson) ?? [];
        var def = await _db.ExamDefinitions.AsNoTracking().FirstOrDefaultAsync(e => e.Id == Session.ExamDefinitionId);
        DurationMinutes = def?.DurationMinutes ?? Math.Max(1, Session.RemainingSeconds / 60);
        QuestionCount = _ids.Count;
        ExamTitle = def?.Title ?? ExamId;
        ExamMode = def?.HideHints ?? true;
        HasActiveExam = true;
        await ShowIndex(Math.Clamp(Session.CurrentIndex, 0, Math.Max(0, _ids.Count - 1)));
    }

    private async Task ShowIndex(int i)
    {
        if (Session is null || _ids.Count == 0) return;
        _loadingQuestion = true;
        try
        {
            Session.CurrentIndex = i;
            CurrentQuestion = await _db.LearningContents.FirstAsync(c => c.Id == _ids[i]);
            Answer = _answers.GetValueOrDefault(CurrentQuestion.Id, "");
            CurrentMarked = _marked.Contains(CurrentQuestion.Id);
            Remaining = TimeSpan.FromSeconds(Math.Max(0, Session.RemainingSeconds)).ToString(@"mm\:ss");
            RebuildOptions();
            RefreshNav();
        }
        finally
        {
            _loadingQuestion = false;
        }
    }

    partial void OnAnswerChanged(string value)
    {
        if (_loadingQuestion || CurrentQuestion is null) return;
        PersistCurrent();
        RefreshNav();
    }

    private void RebuildOptions()
    {
        Options.Clear();
        if (CurrentQuestion is null) return;
        var ordered = CurrentQuestion.Options.OrderBy(o => o.SortOrder).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            var letter = ((char)('A' + i)).ToString();
            var vm = new ExamOptionVm(letter, ordered[i].Text)
            {
                IsSelected = string.Equals(ordered[i].Text, Answer, StringComparison.Ordinal)
            };
            Options.Add(vm);
        }
    }

    private void RefreshNav()
    {
        while (NavCells.Count > _ids.Count) NavCells.RemoveAt(NavCells.Count - 1);
        for (var i = 0; i < _ids.Count; i++)
        {
            var id = _ids[i];
            var answered = _answers.TryGetValue(id, out var a) && !string.IsNullOrWhiteSpace(a);
            var kind = ExamNavigator.Classify(Session?.CurrentIndex == i, answered, _marked.Contains(id));
            if (i < NavCells.Count)
            {
                NavCells[i].Number = i + 1;
                NavCells[i].Apply(kind);
            }
            else
                NavCells.Add(new ExamNavCellVm(i, i + 1, kind));
        }

        AnsweredCount = _ids.Count(id => _answers.TryGetValue(id, out var a) && !string.IsNullOrWhiteSpace(a));
        ProgressPercent = _ids.Count == 0 ? 0 : (int)Math.Round(AnsweredCount * 100.0 / _ids.Count);
        MarkedInfo = Loc.T("Exam.Task", (Session?.CurrentIndex ?? 0) + 1, _ids.Count);
    }
}

public sealed class WiSoViewModel : ExamHostViewModel
{
    public WiSoViewModel(IExamEngine e, AppDbContext db, IDialogService d) : base(e, db, d) { ExamId = "exam-wiso"; }
}

public sealed class ProbeklausurViewModel : ExamHostViewModel
{
    public override bool ShowExamPicker => true;
    public ProbeklausurViewModel(IExamEngine e, AppDbContext db, IDialogService d) : base(e, db, d) { ExamId = "exam-teil1"; }
}

public sealed partial class FehlerbuchViewModel : ObservableObject, INavigationAware
{
    private readonly IMistakeBookService _book;
    private readonly INavigationService _nav;
    public ObservableCollection<MistakeEntry> Items { get; } = [];
    public FehlerbuchViewModel(IMistakeBookService book, INavigationService nav) { _book = book; _nav = nav; }
    public async void OnNavigated(object? parameter)
    {
        Items.Clear();
        foreach (var m in await _book.GetOpenAsync()) Items.Add(m);
    }
    [RelayCommand] private async Task Correct(MistakeEntry? m) { if (m is null) return; await _book.MarkCorrectedAsync(m.Id); OnNavigated(null); }
    [RelayCommand] private void Retry(MistakeEntry? m)
    {
        if (m is null) return;
        _nav.Navigate(string.IsNullOrEmpty(m.TemplateId) ? AppSection.Aufgabentrainer : AppSection.Aufgabentrainer, m.TopicId);
    }
}

public sealed partial class StatistikViewModel : ObservableObject, INavigationAware
{
    private readonly IStatisticsService _stats;
    private readonly INavigationService _nav;
    [ObservableProperty] private string text = "";
    [ObservableProperty] private ISeries[] examSeries = [];
    [ObservableProperty] private Axis[] examXAxes = [new Axis { Name = Loc.T("Stat.AxisTime"), Labels = [] }];
    [ObservableProperty] private Axis[] examYAxes = [new Axis { Name = Loc.T("Stat.AxisPts"), MinLimit = 0, MaxLimit = 100 }];
    [ObservableProperty] private ISeries[] masterySeries = [];
    [ObservableProperty] private Axis[] masteryXAxes = [new Axis { LabelsRotation = 45, Labels = [] }];
    [ObservableProperty] private Axis[] masteryYAxes = [new Axis { Name = "Mastery", MinLimit = 0, MaxLimit = 1 }];

    public StatistikViewModel(IStatisticsService stats, INavigationService nav) { _stats = stats; _nav = nav; }
    public async void OnNavigated(object? parameter)
    {
        var s = await _stats.GetAsync();
        Text = Loc.T("Stat.Retention", s.RetentionRate) + "\n" + Loc.T("Stat.Due", s.DueWorkload) + "\n" +
               Loc.T("Stat.Coverage") + "\n" + string.Join("\n", s.Coverage.Take(30).Select(c =>
                   Loc.T("Stat.TopicLine", Loc.Current.Content(c.Title),
                       c.HasTheory ? Loc.T("Stat.Yes") : Loc.T("Stat.No"),
                       c.HasFlashcards ? Loc.T("Stat.Yes") : Loc.T("Stat.No"),
                       c.HasExercises ? Loc.T("Stat.Yes") : Loc.T("Stat.No"),
                       c.Mastery))) +
               "\n\n" + Loc.T("Stat.HistoryBlock") + "\n" + string.Join("\n", s.ExamHistory.Select(e => $"{e.When:g}: {e.Score}%"));

        var history = s.ExamHistory.ToList();
        ExamXAxes = [new Axis { Name = Loc.T("Stat.AxisExam"), Labels = history.Select(h => h.When.ToString("dd.MM")).ToArray() }];
        ExamYAxes = [new Axis { Name = Loc.T("Stat.AxisPts"), MinLimit = 0, MaxLimit = 100 }];
        ExamSeries =
        [
            new LineSeries<int>
            {
                Name = Loc.T("Stat.ExamResult"),
                Values = history.Select(h => h.Score).ToArray()
            }
        ];

        var slice = s.Coverage.Where(c => c.Mastery > 0 || c.HasExercises).Take(16).ToList();
        if (slice.Count == 0) slice = s.Coverage.Take(16).ToList();
        MasteryXAxes = [new Axis { LabelsRotation = 30, Labels = slice.Select(c => c.Title.Length > 22 ? c.Title[..22] + "…" : c.Title).ToArray() }];
        MasterySeries =
        [
            new ColumnSeries<double>
            {
                Name = "Mastery",
                Values = slice.Select(c => c.Mastery).ToArray()
            }
        ];
    }
    [RelayCommand] private void Drill(string? topic) { if (!string.IsNullOrEmpty(topic)) _nav.Navigate(AppSection.Lernkarten, topic); }
}

public sealed partial class InhalteViewModel : ObservableObject, INavigationAware
{
    private readonly AppDbContext _db;
    private readonly Application.Validation.CatalogValidator _val;
    [ObservableProperty] private string report = "";
    public InhalteViewModel(AppDbContext db, Application.Validation.CatalogValidator val) { _db = db; _val = val; }
    public async void OnNavigated(object? parameter)
    {
        var topics = await _db.Topics.ToListAsync();
        var contents = await _db.LearningContents.ToListAsync();
        var tpls = await _db.ExerciseTemplates.ToListAsync();
        var issues = _val.Validate(topics, contents, tpls);
        Report = Loc.T("Cont.Summary", topics.Count, contents.Count, tpls.Count, await _db.ContentConflicts.CountAsync()) + "\n" +
                 (issues.Count == 0 ? Loc.T("Cont.Ok") : string.Join("\n", issues.Select(i => $"{i.Code}: {i.Message} ({i.ContentId})")));
    }
}

public sealed partial class EinstellungenViewModel : ObservableObject, INavigationAware
{
    private readonly AppDbContext _db;
    private readonly ThemeService _theme;
    private readonly IBackupService _backup;
    private readonly IDialogService _dialog;
    [ObservableProperty] private string displayName = "Lernende";
    [ObservableProperty] private string themeCode = "Light";
    [ObservableProperty] private double fontScale = 1;
    [ObservableProperty] private bool reducedMotion;
    [ObservableProperty] private bool colorBlind = true;
    [ObservableProperty] private string status = "";
    [ObservableProperty] private string uiLanguage = "de";
    public IReadOnlyList<LanguageOption> Languages => Loc.Current.Languages;
    public ObservableCollection<LanguageOption> ThemeChoices { get; } = [];
    public EinstellungenViewModel(AppDbContext db, ThemeService theme, IBackupService backup, IDialogService dialog)
    { _db = db; _theme = theme; _backup = backup; _dialog = dialog; FillChoices(); }
    private void FillChoices()
    {
        ThemeChoices.Clear();
        ThemeChoices.Add(new("Light", Loc.T("Theme.Light")));
        ThemeChoices.Add(new("Dark", Loc.T("Theme.Dark")));
        ThemeChoices.Add(new("System", Loc.T("Theme.System")));
        ThemeChoices.Add(new("Contrast", Loc.T("Theme.Contrast")));
    }
    public async void OnNavigated(object? parameter)
    {
        FillChoices();
        var s = await _db.UserSettings.FirstAsync();
        DisplayName = s.DisplayName; FontScale = s.FontScale; ReducedMotion = s.ReducedMotion; ColorBlind = s.ColorBlindFriendly;
        ThemeCode = s.Theme switch { ThemeKind.Dark => "Dark", ThemeKind.HighContrast => "Contrast", ThemeKind.System => "System", _ => "Light" };
        UiLanguage = string.IsNullOrWhiteSpace(s.UiLanguage) ? "de" : s.UiLanguage;
    }
    [RelayCommand]
    public async Task SaveAsync()
    {
        var s = await _db.UserSettings.FirstAsync();
        s.DisplayName = DisplayName;
        s.FontScale = FontScale;
        s.ReducedMotion = ReducedMotion;
        s.ColorBlindFriendly = ColorBlind;
        s.UiLanguage = UiLanguage;
        s.Theme = ThemeCode switch { "Dark" => ThemeKind.Dark, "Contrast" => ThemeKind.HighContrast, "System" => ThemeKind.System, _ => ThemeKind.Light };
        await _db.SaveChangesAsync();
        Loc.Current.SetLanguage(UiLanguage);
        FillChoices();
        _theme.Apply(s);
        Status = Loc.T("Set.Saved");
    }
    partial void OnUiLanguageChanged(string value)
    {
        Loc.Current.SetLanguage(value);
        FillChoices();
    }
    [RelayCommand]
    public async Task BackupAsync() => Status = Loc.T("Set.BackupOk", await _backup.BackupAsync());
    [RelayCommand]
    public async Task ResetAsync()
    {
        if (!await _dialog.ConfirmAsync(Loc.T("Set.ResetTitle"), Loc.T("Set.ResetAsk"))) return;
        await _backup.ResetAllAsync();
        Status = Loc.T("Set.ResetOk");
    }
}
