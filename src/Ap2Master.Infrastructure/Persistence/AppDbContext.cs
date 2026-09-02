using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ap2Master.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<LearningModule> LearningModules => Set<LearningModule>();
    public DbSet<LearningContent> LearningContents => Set<LearningContent>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Topic>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentTopicId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<LearningContent>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Topic).WithMany(x => x.Contents).HasForeignKey(x => x.TopicId);
            e.HasMany(x => x.Options).WithOne(x => x.LearningContent).HasForeignKey(x => x.LearningContentId);
        });
        modelBuilder.Entity<Flashcard>().HasKey(x => x.Id);
        modelBuilder.Entity<LegalFact>().HasKey(x => x.Id);
        modelBuilder.Entity<ExerciseTemplate>().HasKey(x => x.Id);
        modelBuilder.Entity<ExamDefinition>().HasKey(x => x.Id);
        modelBuilder.Entity<UserSettings>().HasKey(x => x.Id);
        modelBuilder.Entity<Achievement>().HasKey(x => x.Id);
        modelBuilder.Entity<LearningModule>().HasKey(x => x.Id);
        modelBuilder.Entity<ContentConflict>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<AssistantConversation>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Messages).WithOne(x => x.Conversation).HasForeignKey(x => x.ConversationId);
        });
        modelBuilder.Entity<AssistantMessage>().HasKey(x => x.Id);
        modelBuilder.Entity<AiProviderConfiguration>().HasKey(x => x.Id);
        modelBuilder.Entity<KnowledgeChunk>().HasKey(x => x.Id);
        modelBuilder.Entity<AssistantPromptVersion>().HasKey(x => x.Id);
    }
}
