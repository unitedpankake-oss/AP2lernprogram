namespace Ap2Master.Domain.Entities;

public sealed class Flashcard
{
    public string Id { get; set; } = "";
    public string LearningContentId { get; set; } = "";
    public LearningContent? LearningContent { get; set; }
    public string TopicId { get; set; } = "";
    public string Front { get; set; } = "";
    public string Back { get; set; } = "";
    public string? Code { get; set; }
    public string? TableJson { get; set; }
    public string? Formula { get; set; }
    public string? ImagePath { get; set; }
    public bool IsFavorite { get; set; }
    public string? PersonalNotes { get; set; }
    public double Easiness { get; set; } = 2.5;
    public int Repetitions { get; set; }
    public int IntervalDays { get; set; }
    public DateTime DueAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReviewedAt { get; set; }
    public ICollection<FlashcardReview> Reviews { get; set; } = new List<FlashcardReview>();
}

public sealed class FlashcardReview
{
    public Guid Id { get; set; }
    public string FlashcardId { get; set; } = "";
    public Flashcard? Flashcard { get; set; }
    public DateTime ReviewedAt { get; set; }
    public FlashcardRating Rating { get; set; }
    public int SecondsSpent { get; set; }
}

public sealed class UserAnswer
{
    public Guid Id { get; set; }
    public string? LearningContentId { get; set; }
    public string? GeneratedExerciseId { get; set; }
    public string TopicId { get; set; } = "";
    public Difficulty Difficulty { get; set; }
    public DateTime AnsweredAt { get; set; }
    public string LearnerAnswer { get; set; } = "";
    public string CorrectAnswer { get; set; } = "";
    public bool IsCorrect { get; set; }
    public int PointsAwarded { get; set; }
    public int PointsPossible { get; set; }
    public int SecondsSpent { get; set; }
    public Guid? StudySessionId { get; set; }
    public Guid? ExamSessionId { get; set; }
}

public sealed class StudySession
{
    public Guid Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public AppSection Section { get; set; }
    public string TopicId { get; set; } = "";
    public int ItemsCompleted { get; set; }
    public int SecondsSpent { get; set; }
}

public sealed class MistakeEntry
{
    public Guid Id { get; set; }
    public string TopicId { get; set; } = "";
    public string TaskText { get; set; } = "";
    public int? RandomSeed { get; set; }
    public string LearnerAnswer { get; set; } = "";
    public string CorrectAnswer { get; set; } = "";
    public string Explanation { get; set; } = "";
    public Difficulty Difficulty { get; set; }
    public DateTime OccurredAt { get; set; }
    public int SecondsSpent { get; set; }
    public int Repetitions { get; set; }
    public bool IsCorrected { get; set; }
    public MistakeType MistakeType { get; set; }
    public string? TemplateId { get; set; }
    public string? ExamSessionId { get; set; }
}

public sealed class Achievement
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime? UnlockedAt { get; set; }
}

public sealed class UserSettings
{
    public int Id { get; set; } = 1;
    public string DisplayName { get; set; } = "Lernende";
    public bool IsFiae { get; set; } = true;
    public DateTime? ExamDate { get; set; }
    public string AvailableWeekdaysCsv { get; set; } = "1,2,3,4,5";
    public int MinutesPerDay { get; set; } = 60;
    public string KnowledgeLevel { get; set; } = "mittel";
    public string PreferredModesCsv { get; set; } = "Lernkarten,SQL,WiSo";
    public int TargetScore { get; set; } = 75;
    public string DifficultSubjectsCsv { get; set; } = "";
    public bool OnboardingCompleted { get; set; }
    public ThemeKind Theme { get; set; } = ThemeKind.Dark;
    public double FontScale { get; set; } = 1.0;
    public bool ReducedMotion { get; set; }
    public bool ColorBlindFriendly { get; set; } = true;
    public bool SidebarCollapsed { get; set; }
    public string? AiProviderName { get; set; }
    public string UiLanguage { get; set; } = "de";
    public DateTime? LastBackupAt { get; set; }
    public int LastReadinessScore { get; set; }
    public string LastReadinessReasonsJson { get; set; } = "[]";
}

public sealed class ReadinessSnapshot
{
    public Guid Id { get; set; }
    public DateTime TakenAt { get; set; }
    public int Score { get; set; }
    public string BreakdownJson { get; set; } = "{}";
    public string ChangeReasonsJson { get; set; } = "[]";
}
