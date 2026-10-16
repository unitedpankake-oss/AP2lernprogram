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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Ap2Master.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAp2Master(this IServiceCollection services, string? databaseName = null)
    {
        var name = string.IsNullOrWhiteSpace(databaseName) ? MongoSettings.DatabaseName : databaseName;
        MaterialLibrary.Shared.Bind(name);
        services.AddSingleton(new MongoDatabaseOptions(MongoSettings.ConnectionString, name));
        services.AddDbContext<AppDbContext>(o => o.UseMongoDB(MongoSettings.ConnectionString, name), ServiceLifetime.Singleton);
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
    private readonly MongoDatabaseOptions _mongo;

    public DatabaseInitializer(AppDbContext db, IContentCatalog catalog, Application.Validation.CatalogValidator validator, IKnowledgeSearchService knowledge, MongoDatabaseOptions mongo)
    {
        _db = db;
        _catalog = catalog;
        _validator = validator;
        _knowledge = knowledge;
        _mongo = mongo;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        MaterialLibrary.Shared.Bind(_mongo.DatabaseName);
        await _db.Database.EnsureCreatedAsync(cancellationToken);
        await MaterialLibrary.Shared.WarmAsync(_db, cancellationToken);
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
    private readonly MongoDatabaseOptions _mongo;

    public BackupService(AppDbContext db, MongoDatabaseOptions mongo)
    {
        _db = db;
        _mongo = mongo;
    }

    public async Task<string> BackupAsync(string? targetPath = null, CancellationToken cancellationToken = default)
    {
        await _db.SaveChangesAsync(cancellationToken);
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ap2Master", "backups");
        Directory.CreateDirectory(dir);
        var zip = targetPath ?? Path.Combine(dir, $"ap2master-{DateTime.Now:yyyyMMdd-HHmmss}.zip");
        if (File.Exists(zip)) File.Delete(zip);

        var database = new MongoClient(_mongo.ConnectionString).GetDatabase(_mongo.DatabaseName);
        using var archive = ZipFile.Open(zip, ZipArchiveMode.Create);
        var names = await database.ListCollectionNames().ToListAsync(cancellationToken);
        foreach (var name in names)
        {
            var docs = await database.GetCollection<BsonDocument>(name)
                .Find(FilterDefinition<BsonDocument>.Empty)
                .ToListAsync(cancellationToken);
            var entry = archive.CreateEntry(name + ".json");
            await using var stream = entry.Open();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(new BsonArray(docs).ToJson());
        }
        return zip;
    }

    public async Task RestoreAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        _db.ChangeTracker.Clear();
        var database = new MongoClient(_mongo.ConnectionString).GetDatabase(_mongo.DatabaseName);
        using var zip = ZipFile.OpenRead(sourcePath);
        foreach (var collectionName in await database.ListCollectionNames().ToListAsync(cancellationToken))
            await database.DropCollectionAsync(collectionName, cancellationToken);

        foreach (var entry in zip.Entries.Where(e => e.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
        {
            using var reader = new StreamReader(entry.Open());
            var json = await reader.ReadToEndAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(json)) continue;
            var docs = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonArray>(json)
                .Select(v => v.AsBsonDocument)
                .ToList();
            if (docs.Count == 0) continue;
            var name = Path.GetFileNameWithoutExtension(entry.Name);
            await database.GetCollection<BsonDocument>(name).InsertManyAsync(docs, cancellationToken: cancellationToken);
        }
    }

    public async Task ResetAllAsync(CancellationToken cancellationToken = default)
    {
        _db.ChangeTracker.Clear();
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
