using Ap2Master.Domain;
using Ap2Master.Domain.Coverage;
using Ap2Master.Domain.Entities;
using Ap2Master.Domain.Learning;
using Ap2Master.Domain.ProjectManagement;

namespace Ap2Master.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}

public interface IAppDatabase
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IBackupService
{
    Task<string> BackupAsync(string? targetPath = null, CancellationToken cancellationToken = default);
    Task RestoreAsync(string sourcePath, CancellationToken cancellationToken = default);
    Task ResetAllAsync(CancellationToken cancellationToken = default);
}

public interface IContentCatalog
{
    IReadOnlyList<Topic> Topics { get; }
    IReadOnlyList<LearningContent> Contents { get; }
    IReadOnlyList<Flashcard> Flashcards { get; }
    IReadOnlyList<ExerciseTemplate> Templates { get; }
    IReadOnlyList<LegalFact> LegalFacts { get; }
    IReadOnlyList<ContentConflict> Conflicts { get; }
    IReadOnlyList<ExamDefinition> Exams { get; }
    IReadOnlyList<SourceReference> Sources { get; }
}

public interface IDashboardService
{
    Task<DashboardSnapshot> GetAsync(CancellationToken cancellationToken = default);
}

public sealed record DashboardSnapshot(
    ReadinessBreakdown Readiness,
    int DueFlashcards,
    int TasksToday,
    int WeeklyMinutes,
    double AverageScore,
    int StreakDays,
    IReadOnlyList<string> WeakTopics,
    IReadOnlyList<string> StrongTopics,
    IReadOnlyList<MistakeEntry> LatestMistakes,
    IReadOnlyList<PlannedActivity> Upcoming,
    DateTime? ExamDate,
    int? DaysUntilExam,
    string RecommendedActivity,
    string LearnerName,
    IReadOnlyList<ExamPartProgress> Parts,
    IReadOnlyList<WeekDayMinutes> WeekBars,
    IReadOnlyList<TopicHeatCell> Heatmap,
    IReadOnlyList<(DateTime When, int Score)> ReadinessTrend);

public sealed record ExamPartProgress(ExamPart Part, int Percent, int Awarded, int Possible);
public sealed record WeekDayMinutes(string Label, int Minutes, double Ratio);
public sealed record TopicHeatCell(string TopicId, string Title, ExamPart Part, double Mastery, string BandKey, int Column);

public interface IFlashcardService
{
    Task<IReadOnlyList<Flashcard>> GetDueAsync(string? topicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Flashcard>> GetRandomAsync(int count, string? topicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Flashcard>> GetWeakAsync(CancellationToken cancellationToken = default);
    Task ReviewAsync(string flashcardId, FlashcardRating rating, int seconds, CancellationToken cancellationToken = default);
    Task ToggleFavoriteAsync(string flashcardId, CancellationToken cancellationToken = default);
    Task SaveNotesAsync(string flashcardId, string notes, CancellationToken cancellationToken = default);
    Task<int> ImportJsonAsync(string json, CancellationToken cancellationToken = default);
    Task<string> ExportJsonAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamPartBoard>> GetBoardsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Flashcard>> BuildQueueAsync(FlashcardQueueKind kind, IReadOnlyList<string>? topicIds, int take = 40, CancellationToken cancellationToken = default);
}

public interface IExerciseGenerator
{
    GeneratedExercise Generate(ExerciseTemplate template, int seed);
}

public interface ILearningPlanService
{
    Task<LearningPlan> CreateOrRebuildAsync(UserSettings settings, CancellationToken cancellationToken = default);
    Task RescheduleMissedAsync(CancellationToken cancellationToken = default);
    Task CompleteAsync(Guid activityId, CancellationToken cancellationToken = default);
    Task MoveAsync(Guid activityId, DateOnly date, CancellationToken cancellationToken = default);
}

public interface IMistakeBookService
{
    Task AddAsync(MistakeEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MistakeEntry>> GetOpenAsync(CancellationToken cancellationToken = default);
    Task MarkCorrectedAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IStatisticsService
{
    Task<StatisticsSnapshot> GetAsync(CancellationToken cancellationToken = default);
}

public sealed record StatisticsSnapshot(
    IReadOnlyDictionary<string, double> MasteryByTopic,
    IReadOnlyDictionary<string, double> AccuracyBySubject,
    IReadOnlyDictionary<Difficulty, double> AccuracyByDifficulty,
    IReadOnlyList<int> ResponseTimesSeconds,
    IReadOnlyList<(DateTime When, int Score)> ExamHistory,
    double RetentionRate,
    IReadOnlyDictionary<MistakeType, int> MistakeTypes,
    IReadOnlyList<(DateOnly WeekStart, int Minutes)> WeeklyTime,
    IReadOnlyList<(DateTime When, int Score)> ReadinessTrend,
    int DueWorkload,
    IReadOnlyList<TopicCoverageRow> Coverage);

public sealed record TopicCoverageRow(
    string TopicId,
    string Title,
    ExamPart Part,
    bool HasTheory,
    bool HasFlashcards,
    bool HasExercises,
    double Mastery);

public interface ISqlLabService
{
    Task ResetAsync(CancellationToken cancellationToken = default);
    Task<SqlExecutionResult> ExecuteAsync(string sql, bool previewMutations, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetSchemaAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SqlTableInfo>> GetSchemaTreeAsync(CancellationToken cancellationToken = default);
    Task<string?> ExplainAsync(string sql, CancellationToken cancellationToken = default);
    Task<bool> MatchesExpectedAsync(string learnerSql, string expectedSql, bool requireOrder, CancellationToken cancellationToken = default);
}

public sealed record SqlColumnInfo(
    string Name,
    string Type,
    bool NotNull,
    bool IsPrimaryKey,
    bool IsForeignKey,
    string? ForeignTable);

public sealed record SqlTableInfo(string Name, IReadOnlyList<SqlColumnInfo> Columns);

public sealed record SqlExecutionResult(
    bool Success,
    string? Error,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string>> Rows,
    int Affected,
    bool IsMutation,
    string? PreviewMessage);

public interface IPseudocodeEngine
{
    PseudocodeResult Run(string source, IReadOnlyDictionary<string, object?> inputs);
    IReadOnlyList<PseudocodeStep> Trace(string source, IReadOnlyDictionary<string, object?> inputs);
}

public sealed record PseudocodeResult(bool Success, object? ReturnValue, IReadOnlyDictionary<string, object?> Variables, string? Error, IReadOnlyList<string> Output);
public sealed record PseudocodeStep(int Line, string Description, IReadOnlyDictionary<string, object?> Variables);

public interface ICoverageTrainer
{
    CoverageResult Evaluate(ControlFlowGraph graph, IEnumerable<string> selectedPathIds);
    IReadOnlyList<string> MinimalSet(ControlFlowGraph graph, string mode);
}

public interface INetworkPlanTrainer
{
    NetworkPlanResult Calculate(IReadOnlyList<NetworkActivity> activities);
}

public interface IExamEngine
{
    Task<ExamSession> StartAsync(string examDefinitionId, CancellationToken cancellationToken = default);
    Task SaveAsync(ExamSession session, CancellationToken cancellationToken = default);
    Task<ExamSession?> RestoreOpenAsync(ExamKind kind, CancellationToken cancellationToken = default);
    Task<ExamResult> SubmitAsync(Guid sessionId, bool timedOut, CancellationToken cancellationToken = default);
}

public interface IThemeService
{
    ThemeKind Current { get; }
    ThemeKind CurrentTheme { get; }
    ThemeKind ResolvedTheme { get; }
    double FontScale { get; }
    bool ReducedMotion { get; }
    void Apply(UserSettings settings);
    void ApplyTheme(ThemeKind theme);
    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
}

public sealed class ThemeChangedEventArgs : EventArgs
{
    public ThemeKind Requested { get; init; }
    public ThemeKind Resolved { get; init; }
}

public sealed record InboxItem(string Id, string Title, string Detail, AppSection Section, string Kind);

public interface IInboxService
{
    Task<IReadOnlyList<InboxItem>> GetAsync(CancellationToken cancellationToken = default);
}

public interface INavigationService
{
    AppSection Current { get; }
    object CurrentViewModel { get; }
    void Navigate(AppSection section, object? parameter = null);
    event EventHandler? CurrentChanged;
}

public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message);
    Task AlertAsync(string title, string message);
}

public interface ILearningContentAiProvider
{
    Task<GeneratedExplanation> ExplainAsync(string topicId, string question, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuestionDraft>> GenerateQuestionDraftsAsync(string topicId, int count, CancellationToken cancellationToken = default);
}

public sealed record GeneratedExplanation(string Text, bool RequiresReview);
public sealed record QuestionDraft(string Question, IReadOnlyList<string> Options, int CorrectIndex, string Explanation, string SourceHint);

public interface ISecureSecretStore
{
    void Save(string name, string secret);
    string? Load(string name);
    void Delete(string name);
}

public interface IProgressTracker
{
    Task RecordAnswerAsync(UserAnswer answer, CancellationToken cancellationToken = default);
    Task RecordSessionAsync(StudySession session, CancellationToken cancellationToken = default);
}

public sealed class NullLearningContentAiProvider : ILearningContentAiProvider
{
    public Task<GeneratedExplanation> ExplainAsync(string topicId, string question, CancellationToken cancellationToken = default)
        => Task.FromResult(new GeneratedExplanation(
            "Die optionale KI-Schnittstelle ist nicht konfiguriert. Die Kernfunktionen stehen offline zur Verfügung.",
            true));

    public Task<IReadOnlyList<QuestionDraft>> GenerateQuestionDraftsAsync(string topicId, int count, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<QuestionDraft>>(Array.Empty<QuestionDraft>());
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
