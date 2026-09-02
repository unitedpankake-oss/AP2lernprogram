using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Ap2Master.Application.Abstractions;
using Ap2Master.Application.Ai;
using Ap2Master.Application.Planning;
using Ap2Master.Content;
using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.Domain.Learning;
using Ap2Master.ExamEngine;
using Ap2Master.Infrastructure.Ai;
using Ap2Master.Infrastructure.Persistence;
using Ap2Master.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ap2Master.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAp2Master(this IServiceCollection services, string databasePath)
    {
        services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={databasePath};Pooling=False"), ServiceLifetime.Singleton);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IContentCatalog, ContentCatalogFactory>();
        services.AddSingleton<IExerciseGenerator, Generators.SeededTemplateEngine>();
        services.AddSingleton<IPseudocodeEngine, Generators.PseudocodeEngine>();
        services.AddSingleton<ICoverageTrainer, CoverageTrainerService>();
        services.AddSingleton<INetworkPlanTrainer, NetworkPlanTrainerService>();
        services.AddSingleton<ILearningContentAiProvider, NullLearningContentAiProvider>();
        services.AddSingleton<LearningPlanService>();
        services.AddSingleton<ExamScoringService>();
        services.AddSingleton<Application.Validation.CatalogValidator>();
        services.AddSingleton<ISecureSecretStore, DpapiSecretStore>();
        services.AddSingleton<IAppDatabase, DatabaseInitializer>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<IDashboardService, DashboardService>();
        services.AddSingleton<IInboxService, InboxService>();
        services.AddSingleton<IFlashcardService, FlashcardService>();
        services.AddSingleton<ILearningPlanService, LearningPlanAppService>();
        services.AddSingleton<IMistakeBookService, MistakeBookService>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddSingleton<ISqlLabService, SqlLabService>();
        services.AddSingleton<IExamEngine, ExamEngineService>();
        services.AddSingleton<IProgressTracker, ProgressTracker>();
        services.AddSingleton(_ => new HttpClient { Timeout = TimeSpan.FromMinutes(6) });
        services.AddSingleton<IModelCatalogService, ModelCatalogService>();
        services.AddSingleton<IAssistantContextBroker, AssistantContextBroker>();
        services.AddSingleton<IKnowledgeSearchService, KnowledgeSearchService>();
        services.AddSingleton<IAiConversationStore, AiConversationStore>();
        services.AddSingleton<ILocalLanguageModelProvider, CompositeLanguageModelProvider>();
        services.AddSingleton<IAiAssistantService, AiAssistantService>();
        return services;
    }
}

public sealed class DatabaseInitializer : IAppDatabase
{
    private readonly AppDbContext _db;
    private readonly IContentCatalog _catalog;
    private readonly Application.Validation.CatalogValidator _validator;
    private readonly IKnowledgeSearchService _knowledge;

    public DatabaseInitializer(AppDbContext db, IContentCatalog catalog, Application.Validation.CatalogValidator validator, IKnowledgeSearchService knowledge)
    {
        _db = db;
        _catalog = catalog;
        _validator = validator;
        _knowledge = knowledge;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.EnsureCreatedAsync(cancellationToken);
        await AiSchema.EnsureAsync(_db, cancellationToken);
        if (await _db.Topics.AnyAsync(cancellationToken))
        {
            if (!await _db.UserSettings.AnyAsync(cancellationToken))
                _db.UserSettings.Add(new UserSettings { Id = 1 });
            await _db.SaveChangesAsync(cancellationToken);
            await _knowledge.RebuildAsync(cancellationToken);
            return;
        }

        var issues = _validator.Validate(_catalog.Topics, _catalog.Contents, _catalog.Templates);
        foreach (var issue in issues.Where(i => i.Code is "duplicate-id" or "missing-answer" or "mc-single"))
        {
            var c = _catalog.Contents.FirstOrDefault(x => x.Id == issue.ContentId);
            if (c is not null) { c.IsQuarantined = true; c.QuarantineReason = issue.Message; }
        }

        _db.Topics.AddRange(_catalog.Topics);
        await _db.SaveChangesAsync(cancellationToken);
        foreach (var c in _catalog.Contents)
        {
            c.Topic = null;
            foreach (var o in c.Options) o.LearningContent = null;
        }
        _db.LearningContents.AddRange(_catalog.Contents);
        _db.Flashcards.AddRange(_catalog.Flashcards);
        _db.ExerciseTemplates.AddRange(_catalog.Templates);
        _db.LegalFacts.AddRange(_catalog.LegalFacts);
        _db.ContentConflicts.AddRange(_catalog.Conflicts);
        _db.ExamDefinitions.AddRange(_catalog.Exams);
        _db.SourceReferences.AddRange(_catalog.Sources);
        _db.UserSettings.Add(new UserSettings { Id = 1, DisplayName = "Lernende" });
        _db.Achievements.Add(new Achievement { Id = "first-card", Title = "Erste Lernkarte", Description = "Die erste Karte wurde bewertet." });
        await _db.SaveChangesAsync(cancellationToken);
        await _knowledge.RebuildAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}

public sealed class BackupService : IBackupService
{
    private readonly AppDbContext _db;
    private readonly string _path;

    public BackupService(AppDbContext db)
    {
        _db = db;
        _path = db.Database.GetDbConnection().DataSource;
    }

    public async Task<string> BackupAsync(string? targetPath = null, CancellationToken cancellationToken = default)
    {
        await _db.SaveChangesAsync(cancellationToken);
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ap2Master", "backups");
        Directory.CreateDirectory(dir);
        var dest = targetPath ?? Path.Combine(dir, $"ap2master-{DateTime.Now:yyyyMMdd-HHmmss}.db");
        File.Copy(_path, dest, overwrite: true);
        var zip = dest + ".zip";
        if (File.Exists(zip)) File.Delete(zip);
        using var archive = ZipFile.Open(zip, ZipArchiveMode.Create);
        archive.CreateEntryFromFile(dest, Path.GetFileName(dest));
        return zip;
    }

    public async Task RestoreAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        await _db.Database.CloseConnectionAsync();
        var file = sourcePath;
        if (sourcePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            var tmp = Path.Combine(Path.GetTempPath(), "ap2-restore.db");
            using var zip = ZipFile.OpenRead(sourcePath);
            zip.Entries[0].ExtractToFile(tmp, true);
            file = tmp;
        }
        File.Copy(file, _path, overwrite: true);
    }

    public async Task ResetAllAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.EnsureDeletedAsync(cancellationToken);
        await _db.Database.EnsureCreatedAsync(cancellationToken);
        await AiSchema.EnsureAsync(_db, cancellationToken);
    }
}

public sealed class DpapiSecretStore : ISecureSecretStore
{
    private static string FilePath(string name)
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ap2Master", "secrets");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(name))) + ".dpapi");
    }

    public void Save(string name, string secret)
    {
        var data = ProtectedData.Protect(Encoding.UTF8.GetBytes(secret), null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(FilePath(name), data);
    }

    public string? Load(string name)
    {
        var path = FilePath(name);
        if (!File.Exists(path)) return null;
        var bytes = ProtectedData.Unprotect(File.ReadAllBytes(path), null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(bytes);
    }

    public void Delete(string name)
    {
        var path = FilePath(name);
        if (File.Exists(path)) File.Delete(path);
    }
}

public sealed class ProgressTracker : IProgressTracker
{
    private readonly AppDbContext _db;
    public ProgressTracker(AppDbContext db) => _db = db;
    public async Task RecordAnswerAsync(UserAnswer answer, CancellationToken cancellationToken = default)
    {
        _db.UserAnswers.Add(answer);
        await _db.SaveChangesAsync(cancellationToken);
    }
    public async Task RecordSessionAsync(StudySession session, CancellationToken cancellationToken = default)
    {
        _db.StudySessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
