using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ap2Master.App.Localization;
using Ap2Master.App.Services;
using Ap2Master.Application.Abstractions;
using Ap2Master.Content;
using Ap2Master.Domain.Evaluation;

namespace Ap2Master.App.ViewModels;

public sealed class SqlTaskPick
{
    public SqlTaskPick(SqlLabTask task) => Task = task;
    public SqlLabTask Task { get; }
    public int Number => Task.Number;
    public string Title => Task.Title;
    public override string ToString() => Number + " · " + Title;
}

public sealed partial class SqlQueryTab : ObservableObject
{
    [ObservableProperty] private string title = "Abfrage1.sql";
    [ObservableProperty] private string sql = "";
    [ObservableProperty] private bool isActive;
}

public sealed partial class SchemaNode : ObservableObject
{
    public SchemaNode(string name, string kind, string meta, IReadOnlyList<SchemaNode>? children = null)
    {
        Name = name;
        Kind = kind;
        Meta = meta;
        Children = children ?? [];
    }

    public string Name { get; }
    public string Kind { get; }
    public string Meta { get; }
    public IReadOnlyList<SchemaNode> Children { get; }
}

public sealed partial class SqlCriterionRow : ObservableObject
{
    public SqlCriterionRow(SqlCriterionResult result)
    {
        Id = result.Id;
        Label = result.Label;
        Points = result.Points;
        Awarded = result.Awarded;
        Passed = result.Passed;
        StatusKey = result.Passed ? "Sql.Met" : "Sql.OpenCrit";
    }

    public string Id { get; }
    public string Label { get; }
    public int Points { get; }
    public int Awarded { get; }
    public bool Passed { get; }
    public string StatusKey { get; }
}

public sealed partial class SqlLabViewModel : ObservableObject, INavigationAware
{
    private readonly ISqlLabService _sql;
    private bool _suppressQuery;

    public IReadOnlyList<SqlTaskPick> Tasks { get; } = SqlLabTasks.All.Select(t => new SqlTaskPick(t)).ToList();
    public ObservableCollection<SqlQueryTab> Tabs { get; } = [];
    public ObservableCollection<SchemaNode> SchemaRoot { get; } = [];
    public ObservableCollection<SqlCriterionRow> Criteria { get; } = [];
    public ObservableCollection<string> VisibleHints { get; } = [];

    [ObservableProperty] private SqlTaskPick? selectedPick;
    [ObservableProperty] private SqlLabTask? selectedTask;
    [ObservableProperty] private SqlQueryTab? selectedTab;
    [ObservableProperty] private string query = "";
    [ObservableProperty] private string status = "";
    [ObservableProperty] private string compare = "";
    [ObservableProperty] private string planText = "";
    [ObservableProperty] private bool previewMutations = true;
    [ObservableProperty] private DataView? resultView;
    [ObservableProperty] private string resultError = "";
    [ObservableProperty] private long elapsedMs;
    [ObservableProperty] private string scoreText = "";
    [ObservableProperty] private int hintIndex;
    [ObservableProperty] private bool isRunning;

    public SqlLabViewModel(ISqlLabService sql)
    {
        _sql = sql;
        SelectedPick = Tasks.First(t => t.Task.Id == SqlLabTasks.Default.Id);
    }

    public async void OnNavigated(object? parameter)
    {
        await LoadSchemaAsync();
        if (Tabs.Count == 0) NewQuery();
        if (parameter is string id)
        {
            var match = Tasks.FirstOrDefault(t => t.Task.Id == id || t.Task.TopicId == id);
            if (match is not null) SelectedPick = match;
        }
    }

    partial void OnSelectedPickChanged(SqlTaskPick? value)
    {
        SelectedTask = value?.Task;
    }

    partial void OnSelectedTaskChanged(SqlLabTask? value)
    {
        HintIndex = 0;
        VisibleHints.Clear();
        Criteria.Clear();
        ScoreText = "";
        Compare = "";
    }

    partial void OnQueryChanged(string value)
    {
        if (_suppressQuery || SelectedTab is null) return;
        SelectedTab.Sql = value;
    }

    [RelayCommand]
    private void NewQuery()
    {
        var tab = new SqlQueryTab
        {
            Title = $"Abfrage{Tabs.Count + 1}.sql",
            Sql = SelectedTask?.StarterSql ?? "SELECT\n"
        };
        Tabs.Add(tab);
        SelectTab(tab);
    }

    [RelayCommand]
    private void SelectTab(SqlQueryTab? tab)
    {
        if (tab is null) return;
        SelectedTab = tab;
        foreach (var t in Tabs) t.IsActive = t == tab;
        _suppressQuery = true;
        Query = tab.Sql;
        _suppressQuery = false;
    }

    [RelayCommand]
    private void CloseTab(SqlQueryTab? tab)
    {
        if (tab is null || Tabs.Count <= 1) return;
        var i = Tabs.IndexOf(tab);
        Tabs.Remove(tab);
        SelectTab(Tabs[Math.Clamp(i - 1, 0, Tabs.Count - 1)]);
    }

    [RelayCommand]
    private void LoadStarter()
    {
        if (SelectedTask is null || SelectedTab is null) return;
        SelectedTab.Sql = SelectedTask.StarterSql;
        _suppressQuery = true;
        Query = SelectedTask.StarterSql;
        _suppressQuery = false;
    }

    [RelayCommand]
    private async Task Run(CancellationToken cancellationToken)
    {
        IsRunning = true;
        var sw = Stopwatch.StartNew();
        try
        {
            var r = await _sql.ExecuteAsync(Query, PreviewMutations, cancellationToken);
            ElapsedMs = sw.ElapsedMilliseconds;
            Status = r.Success
                ? (r.IsMutation ? Loc.T("Sql.Mut") : Loc.T("Sql.Rows", r.Affected))
                : r.Error ?? Loc.T("Err");
            if (!string.IsNullOrWhiteSpace(r.PreviewMessage))
                Status = r.PreviewMessage;
            ApplyResult(r);
            PlanText = await _sql.ExplainAsync(Query, cancellationToken) ?? "";
        }
        catch (OperationCanceledException)
        {
            Status = Loc.T("Sql.Cancelled");
        }
        finally
        {
            IsRunning = false;
        }
    }

    [RelayCommand]
    private void CancelRun() => RunCommand.Cancel();

    [RelayCommand]
    private async Task Reset()
    {
        await _sql.ResetAsync();
        Status = Loc.T("Sql.ResetOk");
        ResultView = null;
        ResultError = "";
        await LoadSchemaAsync();
    }

    [RelayCommand]
    private async Task CompareSets()
    {
        if (SelectedTask is null) { Compare = Loc.T("Sql.NoTask"); return; }
        Compare = await _sql.MatchesExpectedAsync(Query, SelectedTask.ExpectedSql, false)
            ? Loc.T("Sql.Match")
            : Loc.T("Sql.Diff");
    }

    [RelayCommand]
    private async Task CheckSolution()
    {
        if (SelectedTask is null) return;
        var resultOk = await _sql.MatchesExpectedAsync(Query, SelectedTask.ExpectedSql, false);
        var orderOk = SelectedTask.RequireOrder && await _sql.MatchesExpectedAsync(Query, SelectedTask.ExpectedSql, true);
        var grade = SqlTaskEvaluator.Grade(Query, SelectedTask.Criteria, resultOk, orderOk);
        Criteria.Clear();
        foreach (var row in grade.Criteria)
            Criteria.Add(new SqlCriterionRow(row));
        ScoreText = Loc.T("Sql.Score", grade.Awarded, grade.Max);
        Compare = resultOk ? Loc.T("Sql.Match") : Loc.T("Sql.Diff");
    }

    [RelayCommand]
    private void RevealHint()
    {
        if (SelectedTask is null || HintIndex >= SelectedTask.Hints.Count) return;
        VisibleHints.Add(SelectedTask.Hints[HintIndex]);
        HintIndex++;
    }

    private async Task LoadSchemaAsync()
    {
        var tables = await _sql.GetSchemaTreeAsync();
        SchemaRoot.Clear();
        foreach (var table in tables)
        {
            var cols = table.Columns.Select(c =>
            {
                var kind = c.IsPrimaryKey ? "PK" : c.IsForeignKey ? "FK" : "·";
                var meta = c.Type + (c.NotNull ? " NOT NULL" : "") + (c.ForeignTable is { Length: > 0 } t ? " → " + t : "");
                return new SchemaNode(c.Name, kind, meta);
            }).ToList();
            SchemaRoot.Add(new SchemaNode(table.Name, "TBL", Loc.T("Sql.Cols", table.Columns.Count), cols));
        }
    }

    private void ApplyResult(SqlExecutionResult r)
    {
        if (!r.Success)
        {
            ResultView = null;
            ResultError = r.Error ?? Loc.T("Err");
            return;
        }

        ResultError = "";
        var table = new DataTable();
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in r.Columns)
        {
            var name = string.IsNullOrWhiteSpace(c) ? "col" : c;
            var unique = name;
            var n = 2;
            while (!used.Add(unique)) unique = name + "_" + n++;
            table.Columns.Add(unique);
        }

        foreach (var row in r.Rows)
        {
            var dr = table.NewRow();
            for (var i = 0; i < table.Columns.Count; i++)
                dr[i] = i < row.Count ? row[i] : "";
            table.Rows.Add(dr);
        }

        ResultView = table.DefaultView;
    }
}
