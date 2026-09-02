namespace Ap2Master.Domain.Entities;

public sealed class Topic
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string? ParentTopicId { get; set; }
    public Topic? Parent { get; set; }
    public ExamPart ExamPart { get; set; }
    public bool IsPriority { get; set; }
    public int EstimatedMinutes { get; set; }
    public string TagsCsv { get; set; } = "";
    public int SortOrder { get; set; }
    public ICollection<Topic> Children { get; set; } = new List<Topic>();
    public ICollection<LearningContent> Contents { get; set; } = new List<LearningContent>();
}

public sealed class LearningModule
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public ExamPart ExamPart { get; set; }
    public string TopicId { get; set; } = "";
    public string Description { get; set; } = "";
    public int SortOrder { get; set; }
}

public sealed class LearningContent
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string TopicId { get; set; } = "";
    public Topic? Topic { get; set; }
    public string? ParentTopicId { get; set; }
    public ExamPart ExamPart { get; set; }
    public Difficulty Difficulty { get; set; }
    public ContentType ContentType { get; set; }
    public string TagsCsv { get; set; } = "";
    public int EstimatedMinutes { get; set; }
    public int Points { get; set; }
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public string? Explanation { get; set; }
    public string HintsJson { get; set; } = "[]";
    public string PayloadJson { get; set; } = "{}";
    public string SourceDocument { get; set; } = "";
    public string SourceSection { get; set; } = "";
    public string? SourcePage { get; set; }
    public string SourceType { get; set; } = "";
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public string? LegalBasis { get; set; }
    public string? TechnicalStandardVersion { get; set; }
    public string ContentVersion { get; set; } = "1.0.0";
    public bool IsQuarantined { get; set; }
    public string? QuarantineReason { get; set; }
    public ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();
}

public sealed class AnswerOption
{
    public Guid Id { get; set; }
    public string LearningContentId { get; set; } = "";
    public LearningContent? LearningContent { get; set; }
    public string Text { get; set; } = "";
    public bool IsCorrect { get; set; }
    public string Explanation { get; set; } = "";
    public int SortOrder { get; set; }
}

public sealed class SourceReference
{
    public Guid Id { get; set; }
    public string DocumentName { get; set; } = "";
    public string Section { get; set; } = "";
    public string? Page { get; set; }
    public string TopicId { get; set; } = "";
    public string ContentType { get; set; } = "";
    public bool OverlapsOtherSource { get; set; }
    public bool TimeSensitive { get; set; }
    public bool Verified { get; set; }
    public bool HasExercises { get; set; }
    public bool HasFlashcards { get; set; }
    public string Notes { get; set; } = "";
}

public sealed class LegalFact
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string TopicId { get; set; } = "";
    public string Statement { get; set; } = "";
    public string? HistoricalStatement { get; set; }
    public string LegalBasis { get; set; } = "";
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public DateTime VerifiedAt { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public string SourceUrl { get; set; } = "";
    public string Notes { get; set; } = "";
}

public sealed class ContentConflict
{
    public Guid Id { get; set; }
    public string TopicId { get; set; } = "";
    public string ClaimA { get; set; } = "";
    public string SourceA { get; set; } = "";
    public string ClaimB { get; set; } = "";
    public string SourceB { get; set; } = "";
    public ConflictResolution Resolution { get; set; }
    public string ResolvedClaim { get; set; } = "";
    public string Rationale { get; set; } = "";
    public bool IsResolved { get; set; }
}

public sealed class ContentVersionRecord
{
    public Guid Id { get; set; }
    public string ContentId { get; set; } = "";
    public string Version { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string ChangeSummary { get; set; } = "";
    public string SnapshotJson { get; set; } = "";
}
