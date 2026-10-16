using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ap2Master.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<LearningModule> LearningModules => Set<LearningModule>();
    public DbSet<LearningContent> LearningContents => Set<LearningContent>();
    public DbSet<SourceReference> SourceReferences => Set<SourceReference>();
    public DbSet<LegalFact> LegalFacts => Set<LegalFact>();
    public DbSet<ContentConflict> ContentConflicts => Set<ContentConflict>();
    public DbSet<ContentVersionRecord> ContentVersions => Set<ContentVersionRecord>();
    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<FlashcardReview> FlashcardReviews => Set<FlashcardReview>();
    public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();
    public DbSet<StudySession> StudySessions => Set<StudySession>();
    public DbSet<MistakeEntry> MistakeEntries => Set<MistakeEntry>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<ReadinessSnapshot> ReadinessSnapshots => Set<ReadinessSnapshot>();
    public DbSet<ExerciseTemplate> ExerciseTemplates => Set<ExerciseTemplate>();
    public DbSet<GeneratedExercise> GeneratedExercises => Set<GeneratedExercise>();
    public DbSet<ExamDefinition> ExamDefinitions => Set<ExamDefinition>();
    public DbSet<ExamSession> ExamSessions => Set<ExamSession>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();
    public DbSet<LearningPlan> LearningPlans => Set<LearningPlan>();
    public DbSet<PlannedActivity> PlannedActivities => Set<PlannedActivity>();
    public DbSet<QueryHistoryEntry> QueryHistory => Set<QueryHistoryEntry>();
    public DbSet<ContentApproval> ContentApprovals => Set<ContentApproval>();
    public DbSet<AssistantConversation> AssistantConversations => Set<AssistantConversation>();
    public DbSet<AssistantMessage> AssistantMessages => Set<AssistantMessage>();
    public DbSet<AssistantFeedback> AssistantFeedbacks => Set<AssistantFeedback>();
    public DbSet<AssistantUsage> AssistantUsages => Set<AssistantUsage>();
    public DbSet<AiModelDefinition> AiModelDefinitions => Set<AiModelDefinition>();
    public DbSet<AiProviderConfiguration> AiProviderConfigurations => Set<AiProviderConfiguration>();
    public DbSet<KnowledgeChunk> KnowledgeChunks => Set<KnowledgeChunk>();
    public DbSet<KnowledgeSource> KnowledgeSources => Set<KnowledgeSource>();
    public DbSet<AssistantPromptVersion> AssistantPromptVersions => Set<AssistantPromptVersion>();
    public DbSet<OcrImport> OcrImports => Set<OcrImport>();
    public DbSet<MaterialText> MaterialTexts => Set<MaterialText>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ClusterLimiter.Enter();
        try
        {
            StagePackedTexts();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        finally { ClusterLimiter.Exit(); }
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ClusterLimiter.Enter();
        try
        {
            StagePackedTexts();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        finally { ClusterLimiter.Exit(); }
    }

    private void StagePackedTexts()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;
            foreach (var prop in entry.Properties)
            {
                if (prop.Metadata.FindAnnotation("Packed") is null) continue;
                if (prop.CurrentValue is string text)
                    MaterialLibrary.Shared.Encode(text);
            }
        }

        foreach (var item in MaterialLibrary.Shared.TakePending())
        {
            if (MaterialTexts.Local.All(x => x.Id != item.Id))
                MaterialTexts.Add(item);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Topic>(e =>
        {
            e.HasKey(x => x.Id);
            e.Ignore(x => x.Parent);
            e.Ignore(x => x.Children);
            e.Ignore(x => x.Contents);
        });
        modelBuilder.Entity<LearningContent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Ignore(x => x.Topic);
            e.OwnsMany(x => x.Options, o =>
            {
                o.HasKey(x => x.Id);
                o.Ignore(x => x.LearningContent);
                Pack(o.Property(x => x.Text));
                Pack(o.Property(x => x.Explanation));
            });
            Pack(e.Property(x => x.Title));
            Pack(e.Property(x => x.Question));
            Pack(e.Property(x => x.Answer));
            Pack(e.Property(x => x.Explanation));
            Pack(e.Property(x => x.HintsJson));
            Pack(e.Property(x => x.PayloadJson));
        });
        modelBuilder.Entity<Flashcard>(e =>
        {
            e.HasKey(x => x.Id);
            e.Ignore(x => x.LearningContent);
            e.Ignore(x => x.Reviews);
            Pack(e.Property(x => x.Front));
            Pack(e.Property(x => x.Back));
            Pack(e.Property(x => x.Code));
            Pack(e.Property(x => x.TableJson));
            Pack(e.Property(x => x.Formula));
            Pack(e.Property(x => x.PersonalNotes));
        });
        modelBuilder.Entity<FlashcardReview>(e =>
        {
            e.HasKey(x => x.Id);
            e.Ignore(x => x.Flashcard);
        });
        modelBuilder.Entity<LegalFact>(e =>
        {
            e.HasKey(x => x.Id);
            Pack(e.Property(x => x.Statement));
            Pack(e.Property(x => x.HistoricalStatement));
            Pack(e.Property(x => x.Notes));
        });
        modelBuilder.Entity<ExerciseTemplate>(e =>
        {
            e.HasKey(x => x.Id);
            Pack(e.Property(x => x.TemplateText));
            Pack(e.Property(x => x.SolutionTemplate));
            Pack(e.Property(x => x.ExplanationTemplate));
            Pack(e.Property(x => x.HintsJson));
            Pack(e.Property(x => x.ParameterSchemaJson));
        });
        modelBuilder.Entity<ExamDefinition>().HasKey(x => x.Id);
        modelBuilder.Entity<UserSettings>().HasKey(x => x.Id);
        modelBuilder.Entity<Achievement>().HasKey(x => x.Id);
        modelBuilder.Entity<LearningModule>().HasKey(x => x.Id);
        modelBuilder.Entity<ContentConflict>(e =>
        {
            e.Property(x => x.Id).ValueGeneratedNever();
            Pack(e.Property(x => x.ClaimA));
            Pack(e.Property(x => x.ClaimB));
            Pack(e.Property(x => x.ResolvedClaim));
            Pack(e.Property(x => x.Rationale));
        });
        modelBuilder.Entity<LearningPlan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Ignore(x => x.Activities);
        });
        modelBuilder.Entity<PlannedActivity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Ignore(x => x.LearningPlan);
        });
        modelBuilder.Entity<AssistantConversation>(e =>
        {
            e.HasKey(x => x.Id);
            e.Ignore(x => x.Messages);
        });
        modelBuilder.Entity<AiProviderConfiguration>().HasKey(x => x.Id);
        modelBuilder.Entity<KnowledgeChunk>(e =>
        {
            e.HasKey(x => x.Id);
            Pack(e.Property(x => x.Title));
            Pack(e.Property(x => x.Body));
        });
        modelBuilder.Entity<MaterialText>().HasKey(x => x.Id);
        modelBuilder.Entity<GeneratedExercise>(e =>
        {
            e.HasKey(x => x.Id);
            Pack(e.Property(x => x.TaskText));
            Pack(e.Property(x => x.Solution));
            Pack(e.Property(x => x.Explanation));
            Pack(e.Property(x => x.HintsJson));
            Pack(e.Property(x => x.ParametersJson));
        });
        modelBuilder.Entity<UserAnswer>(e =>
        {
            Pack(e.Property(x => x.LearnerAnswer));
            Pack(e.Property(x => x.CorrectAnswer));
        });
        modelBuilder.Entity<MistakeEntry>(e =>
        {
            Pack(e.Property(x => x.TaskText));
            Pack(e.Property(x => x.LearnerAnswer));
            Pack(e.Property(x => x.CorrectAnswer));
            Pack(e.Property(x => x.Explanation));
        });
        modelBuilder.Entity<AssistantMessage>(e =>
        {
            e.HasKey(x => x.Id);
            e.Ignore(x => x.Conversation);
            Pack(e.Property(x => x.Text));
        });
        modelBuilder.Entity<OcrImport>(e => Pack(e.Property(x => x.Text)));
        modelBuilder.Entity<AssistantPromptVersion>(e =>
        {
            e.HasKey(x => x.Id);
            Pack(e.Property(x => x.SystemPrompt));
        });
    }

    private static void Pack(object property)
    {
        var builder = (PropertyBuilder<string>)property;
        builder.HasConversion(
            v => MaterialLibrary.Shared.Encode(v),
            v => MaterialLibrary.Shared.Decode(v)!);
        builder.HasAnnotation("Packed", true);
    }
}
