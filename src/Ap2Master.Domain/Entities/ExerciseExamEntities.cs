namespace Ap2Master.Domain.Entities;

public sealed class ExerciseTemplate
{
    public string Id { get; set; } = "";
    public string TopicId { get; set; } = "";
    public Difficulty Difficulty { get; set; }
    public ContentType ContentType { get; set; }
    public string Operator { get; set; } = "Nennen";
    public int EstimatedMinutes { get; set; }
    public int Points { get; set; }
    public string TemplateText { get; set; } = "";
    public string SolutionTemplate { get; set; } = "";
    public string ExplanationTemplate { get; set; } = "";
    public string EvaluationStrategy { get; set; } = "exact";
    public string HintsJson { get; set; } = "[]";
    public string ParameterSchemaJson { get; set; } = "{}";
    public bool IsApprovedForExam { get; set; } = true;
}

public sealed class GeneratedExercise
{
    public Guid Id { get; set; }
    public string TemplateId { get; set; } = "";
    public int Seed { get; set; }
    public string TopicId { get; set; } = "";
    public Difficulty Difficulty { get; set; }
    public string Operator { get; set; } = "";
    public int EstimatedMinutes { get; set; }
    public int Points { get; set; }
    public string TaskText { get; set; } = "";
    public string Solution { get; set; } = "";
    public string Explanation { get; set; } = "";
    public string EvaluationStrategy { get; set; } = "";
    public string HintsJson { get; set; } = "[]";
    public string ParametersJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}

public sealed class ExamDefinition
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public ExamKind Kind { get; set; }
    public int DurationMinutes { get; set; }
    public int QuestionCount { get; set; }
    public int TotalPoints { get; set; }
    public string RulesJson { get; set; } = "{}";
    public bool HideHints { get; set; } = true;
}

public sealed class ExamSession
{
    public Guid Id { get; set; }
    public string ExamDefinitionId { get; set; } = "";
    public ExamKind Kind { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? LastSavedAt { get; set; }
    public int RemainingSeconds { get; set; }
    public int CurrentIndex { get; set; }
    public bool IsSubmitted { get; set; }
    public bool TimedOut { get; set; }
    public string QuestionIdsJson { get; set; } = "[]";
    public string AnswersJson { get; set; } = "{}";
    public string MarkedJson { get; set; } = "[]";
    public int? ScorePercent { get; set; }
    public int? PointsAwarded { get; set; }
    public int? PointsPossible { get; set; }
}

public sealed class ExamResult
{
    public Guid Id { get; set; }
    public Guid ExamSessionId { get; set; }
    public DateTime CompletedAt { get; set; }
    public int ScorePercent { get; set; }
    public int PointsAwarded { get; set; }
    public int PointsPossible { get; set; }
    public int ExpectedMinutes { get; set; }
    public int ActualMinutes { get; set; }
    public string TopicStatsJson { get; set; } = "{}";
    public string ReportText { get; set; } = "";
}

public sealed class LearningPlan
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExamDate { get; set; }
    public int MinutesPerDay { get; set; }
    public string AvailableWeekdaysCsv { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public ICollection<PlannedActivity> Activities { get; set; } = new List<PlannedActivity>();
}

public sealed class PlannedActivity
{
    public Guid Id { get; set; }
    public Guid LearningPlanId { get; set; }
    public LearningPlan? LearningPlan { get; set; }
    public DateOnly Date { get; set; }
    public ActivityKind Kind { get; set; }
    public string TopicId { get; set; } = "";
    public string Title { get; set; } = "";
    public int EstimatedMinutes { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsMissed { get; set; }
    public int SortOrder { get; set; }
}

public sealed class QueryHistoryEntry
{
    public Guid Id { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string Sql { get; set; } = "";
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int RowCount { get; set; }
}

public sealed class ContentApproval
{
    public Guid Id { get; set; }
    public string DraftJson { get; set; } = "";
    public string Source { get; set; } = "ai";
    public bool StructurallyValid { get; set; }
    public bool AnswerValid { get; set; }
    public bool DuplicateFree { get; set; }
    public bool SourceLinked { get; set; }
    public bool ManuallyApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
