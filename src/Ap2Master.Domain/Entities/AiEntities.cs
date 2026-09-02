namespace Ap2Master.Domain.Entities;

public sealed class AssistantConversation
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Title { get; set; } = "Neue Unterhaltung";
    public string? TopicId { get; set; }
    public string? TaskId { get; set; }
    public string ModelName { get; set; } = "";
    public ExamAssistantMode Restriction { get; set; }
    public ICollection<AssistantMessage> Messages { get; set; } = new List<AssistantMessage>();
}

public sealed class AssistantMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public AssistantConversation? Conversation { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Role { get; set; } = "user";
    public string Text { get; set; } = "";
    public string SourcesJson { get; set; } = "[]";
    public AssistantHintLevel HintLevel { get; set; }
    public int DurationMs { get; set; }
    public string Intent { get; set; } = "";
}

public sealed class AssistantFeedback
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public bool Helpful { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class AssistantUsage
{
    public Guid Id { get; set; }
    public DateTime At { get; set; }
    public string ModelName { get; set; } = "";
    public string Intent { get; set; } = "";
    public int PromptChars { get; set; }
    public int CompletionChars { get; set; }
    public int DurationMs { get; set; }
    public bool UsedLlm { get; set; }
}

public sealed class AiModelDefinition
{
    public Guid Id { get; set; }
    public string Path { get; set; } = "";
    public string Format { get; set; } = "";
    public LocalModelTask Task { get; set; }
    public bool SuitableForChat { get; set; }
    public string Reason { get; set; } = "";
    public DateTime ScannedAt { get; set; }
}

public sealed class AiProviderConfiguration
{
    public int Id { get; set; } = 1;
    public bool Enabled { get; set; } = true;
    public string ModelsDirectory { get; set; } = AiDefaults.ModelsDirectory;
    public string? PrimaryModelPath { get; set; }
    public string? SecondaryModelPath { get; set; }
    public AiRuntimeKind Runtime { get; set; } = AiRuntimeKind.Auto;
    public string PreferredLanguage { get; set; } = "de";
    public string ExplanationLevel { get; set; } = "mittel";
    public double Temperature { get; set; } = AiDefaults.DefaultTemperature;
    public int MaxResponseTokens { get; set; } = 1024;
    public int MaxContextTokens { get; set; } = AiDefaults.DefaultMaxContextTokens;
    public bool UseGpuWhenAvailable { get; set; } = true;
    public bool SaveHistory { get; set; } = true;
    public ExamAssistantMode ExamRestriction { get; set; } = ExamAssistantMode.Disabled;
    public string OllamaBaseUrl { get; set; } = AiDefaults.OllamaBaseUrl;
    public string OllamaModel { get; set; } = AiDefaults.RecommendedOllamaModel;
}

public sealed class KnowledgeChunk
{
    public Guid Id { get; set; }
    public string TopicId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string SourceDocument { get; set; } = "";
    public string SourceSection { get; set; } = "";
    public string? LegalBasis { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
}

public sealed class KnowledgeSource
{
    public Guid Id { get; set; }
    public string DocumentName { get; set; } = "";
    public string Kind { get; set; } = "";
    public DateTime ImportedAt { get; set; }
}

public sealed class AssistantPromptVersion
{
    public string Id { get; set; } = "v1";
    public string SystemPrompt { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public sealed class OcrImport
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Text { get; set; } = "";
    public double Confidence { get; set; }
    public bool SavedByLearner { get; set; }
}
