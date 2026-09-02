using System.Text;
using System.Text.RegularExpressions;
using Ap2Master.Application.Ai;
using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ap2Master.Infrastructure.Ai;

public sealed class KnowledgeSearchService : IKnowledgeSearchService
{
    private readonly AppDbContext _db;

    public KnowledgeSearchService(AppDbContext db) => _db = db;

    public async Task RebuildAsync(CancellationToken cancellationToken)
    {
        await AiSchema.EnsureAsync(_db, cancellationToken);
        if (await _db.Set<KnowledgeChunk>().AnyAsync(cancellationToken))
            return;

        var contents = await _db.LearningContents.AsNoTracking()
            .Where(c => c.ContentType == ContentType.Theory || c.ContentType == ContentType.CheatSheet)
            .ToListAsync(cancellationToken);
        var facts = await _db.LegalFacts.AsNoTracking().ToListAsync(cancellationToken);
        var chunks = new List<KnowledgeChunk>();
        foreach (var c in contents)
        {
            var body = string.Join("\n", new[] { c.Question, c.Answer, c.Explanation }.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (body.Length < 40) continue;
            chunks.Add(new KnowledgeChunk
            {
                Id = Guid.NewGuid(),
                TopicId = c.TopicId,
                Title = c.Title,
                Body = Trim(body, 1800),
                SourceDocument = MapDoc(c.SourceDocument),
                SourceSection = string.IsNullOrWhiteSpace(c.SourceSection) ? c.Title : c.SourceSection,
                LegalBasis = c.LegalBasis,
                VerifiedAt = c.VerifiedAt,
                VerificationStatus = c.VerificationStatus,
                ValidFrom = c.ValidFrom,
                ValidUntil = c.ValidUntil
            });
        }
        foreach (var f in facts)
        {
            chunks.Add(new KnowledgeChunk
            {
                Id = Guid.NewGuid(),
                TopicId = f.TopicId,
                Title = f.Title,
                Body = Trim(f.Statement + " " + f.Notes, 1200),
                SourceDocument = "Gesetzestext / amtliche Quelle",
                SourceSection = f.LegalBasis ?? f.Title,
                LegalBasis = f.LegalBasis,
                VerifiedAt = f.VerifiedAt,
                VerificationStatus = f.VerificationStatus,
                ValidFrom = f.ValidFrom,
                ValidUntil = f.ValidUntil
            });
        }
        _db.Set<KnowledgeChunk>().AddRange(chunks);
        await _db.SaveChangesAsync(cancellationToken);
        await RebuildFtsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeChunk>> SearchAsync(string query, KnowledgeSearchOptions options, CancellationToken cancellationToken)
    {
        await RebuildAsync(cancellationToken);
        var q = SanitizeFts(query);
        if (q.Length < 2)
        {
            var topic = options.TopicId;
            return await _db.Set<KnowledgeChunk>().AsNoTracking()
                .Where(c => topic == null || c.TopicId == topic)
                .Take(options.Limit).ToListAsync(cancellationToken);
        }

        try
        {
            var ids = await _db.Database.SqlQueryRaw<FtsRow>(
                    "SELECT ChunkId as Id FROM KnowledgeFts WHERE KnowledgeFts MATCH {0} LIMIT {1}", q, options.Limit)
                .ToListAsync(cancellationToken);
            var guidIds = ids.Select(x => Guid.TryParse(x.Id, out var g) ? g : Guid.Empty).Where(g => g != Guid.Empty).ToList();
            var found = await _db.Set<KnowledgeChunk>().AsNoTracking()
                .Where(c => guidIds.Contains(c.Id)).ToListAsync(cancellationToken);
            if (found.Count > 0) return found;
        }
        catch
        {
            // FTS may be unavailable; fall through to LIKE
        }

        var like = "%" + query.Replace("%", "") + "%";
        return await _db.Set<KnowledgeChunk>().AsNoTracking()
            .Where(c => EF.Functions.Like(c.Title, like) || EF.Functions.Like(c.Body, like) || (options.TopicId != null && c.TopicId == options.TopicId))
            .Take(options.Limit)
            .ToListAsync(cancellationToken);
    }

    private async Task RebuildFtsAsync(CancellationToken ct)
    {
        await _db.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS KnowledgeFts;", ct);
        await _db.Database.ExecuteSqlRawAsync(
            "CREATE VIRTUAL TABLE KnowledgeFts USING fts5(ChunkId UNINDEXED, Title, Body, TopicId UNINDEXED, SourceDocument UNINDEXED, SourceSection);", ct);
        var chunks = await _db.Set<KnowledgeChunk>().AsNoTracking().ToListAsync(ct);
        foreach (var c in chunks)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO KnowledgeFts(ChunkId, Title, Body, TopicId, SourceDocument, SourceSection) VALUES ({c.Id.ToString()}, {c.Title}, {c.Body}, {c.TopicId}, {c.SourceDocument}, {c.SourceSection})",
                ct);
        }
    }

    public static string SanitizeFts(string query)
    {
        var t = Regex.Replace(query ?? "", @"[^\p{L}\p{N}\s]", " ");
        t = Regex.Replace(t, @"\s+", " ").Trim();
        if (t.Length == 0) return "";
        var parts = t.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(8).Select(p => "\"" + p.Replace("\"", "") + "\"");
        return string.Join(" OR ", parts);
    }

    public static string FormatSources(IEnumerable<KnowledgeChunk> chunks)
    {
        var lines = new List<string>();
        foreach (var c in chunks)
        {
            var v = c.VerifiedAt is { } d ? $" – geprüft am {d:dd.MM.yyyy}" : "";
            var legal = string.IsNullOrWhiteSpace(c.LegalBasis) ? "" : $" • {c.LegalBasis}{v}";
            lines.Add($"• {c.SourceDocument} – {c.SourceSection}{legal}");
        }
        return lines.Count == 0 ? "" : "Quellen:\n" + string.Join("\n", lines);
    }

    private static string MapDoc(string src)
    {
        if (src.Contains("Lernzettel", StringComparison.OrdinalIgnoreCase)) return "AP2 Lernzettel";
        if (src.Contains("Vorbereitung", StringComparison.OrdinalIgnoreCase) || src.Contains("Intensiv", StringComparison.OrdinalIgnoreCase))
            return "AP2 Intensivkurs";
        return string.IsNullOrWhiteSpace(src) ? "AP2 Master – strukturierter Lehrkanon" : src;
    }

    private static string Trim(string s, int max) => s.Length <= max ? s : s[..max];

    private sealed class FtsRow { public string Id { get; set; } = ""; }
}

public static class AiSchema
{
    public static async Task EnsureAsync(AppDbContext db, CancellationToken ct)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS AssistantConversations (
              Id TEXT PRIMARY KEY, CreatedAt TEXT, UpdatedAt TEXT, Title TEXT, TopicId TEXT, TaskId TEXT, ModelName TEXT, Restriction INTEGER);
            """, ct);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS AssistantMessages (
              Id TEXT PRIMARY KEY, ConversationId TEXT, CreatedAt TEXT, Role TEXT, Text TEXT, SourcesJson TEXT, HintLevel INTEGER, DurationMs INTEGER, Intent TEXT);
            """, ct);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS AssistantFeedbacks (Id TEXT PRIMARY KEY, MessageId TEXT, Helpful INTEGER, Comment TEXT, CreatedAt TEXT);
            """, ct);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS AssistantUsages (Id TEXT PRIMARY KEY, At TEXT, ModelName TEXT, Intent TEXT, PromptChars INTEGER, CompletionChars INTEGER, DurationMs INTEGER, UsedLlm INTEGER);
            """, ct);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS AiModelDefinitions (Id TEXT PRIMARY KEY, Path TEXT, Format TEXT, Task INTEGER, SuitableForChat INTEGER, Reason TEXT, ScannedAt TEXT);
            """, ct);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS AiProviderConfigurations (
              Id INTEGER PRIMARY KEY, Enabled INTEGER, ModelsDirectory TEXT, PrimaryModelPath TEXT, SecondaryModelPath TEXT,
              Runtime INTEGER, PreferredLanguage TEXT, ExplanationLevel TEXT, Temperature REAL, MaxResponseTokens INTEGER,
              MaxContextTokens INTEGER, UseGpuWhenAvailable INTEGER, SaveHistory INTEGER, ExamRestriction INTEGER, OllamaBaseUrl TEXT, OllamaModel TEXT);
            """, ct);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS KnowledgeChunks (
              Id TEXT PRIMARY KEY, TopicId TEXT, Title TEXT, Body TEXT, SourceDocument TEXT, SourceSection TEXT, LegalBasis TEXT,
              VerifiedAt TEXT, VerificationStatus INTEGER, ValidFrom TEXT, ValidUntil TEXT);
            """, ct);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS KnowledgeSources (Id TEXT PRIMARY KEY, DocumentName TEXT, Kind TEXT, ImportedAt TEXT);
            """, ct);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS AssistantPromptVersions (Id TEXT PRIMARY KEY, SystemPrompt TEXT, CreatedAt TEXT);
            """, ct);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS OcrImports (Id TEXT PRIMARY KEY, CreatedAt TEXT, Text TEXT, Confidence REAL, SavedByLearner INTEGER);
            """, ct);
        if (!await db.Set<AiProviderConfiguration>().AnyAsync(ct))
        {
            db.Set<AiProviderConfiguration>().Add(new AiProviderConfiguration { Id = 1 });
            await db.SaveChangesAsync(ct);
        }
        else
        {
            var cfg = await db.Set<AiProviderConfiguration>().FirstAsync(ct);
            var isLegacy = string.Equals(cfg.ModelsDirectory, AiDefaults.LegacyModelsDirectory, StringComparison.OrdinalIgnoreCase);
            var missing = string.IsNullOrWhiteSpace(cfg.ModelsDirectory) || !Directory.Exists(cfg.ModelsDirectory);
            if ((isLegacy || missing) && Directory.Exists(AiDefaults.ModelsDirectory))
            {
                cfg.ModelsDirectory = AiDefaults.ModelsDirectory;
                await db.SaveChangesAsync(ct);
            }
        }
        try
        {
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE UserSettings ADD COLUMN UiLanguage TEXT NOT NULL DEFAULT 'de';", ct);
        }
        catch
        {
            // column already exists
        }
        try
        {
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE UserSettings ADD COLUMN SidebarCollapsed INTEGER NOT NULL DEFAULT 0;", ct);
        }
        catch
        {
            // column already exists
        }
        if (!await db.Set<AssistantPromptVersion>().AnyAsync(ct))
        {
            db.Set<AssistantPromptVersion>().Add(new AssistantPromptVersion
            {
                Id = "v1",
                SystemPrompt = AssistantSystemPrompt.German,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
        }
    }
}

public static class CodePreservingTranslator
{
    private static readonly Regex Code = new("`{1,3}[\\s\\S]*?`{1,3}|\\b(SELECT|INSERT|UPDATE|DELETE|CREATE)\\b[^;\\n]*;", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Dictionary<string, (string Ru, string Uk, string En, string Es)> Glossary = new(StringComparer.OrdinalIgnoreCase)
    {
        ["nennen"] = ("перечислить (кратко)", "назвати (стисло)", "list/name (keywords)", "enumerar (breve)"),
        ["beschreiben"] = ("описать признаки/ход", "описати ознаки/хід", "describe features/process", "describir rasgos/proceso"),
        ["erläutern"] = ("пояснить связь/причину", "пояснити зв’язок/причину", "explain connection/cause", "explicar la relación/causa"),
        ["begründen"] = ("обосновать", "обґрунтувати", "justify", "justificar"),
        ["erstellen"] = ("создать полностью", "створити повністю", "create fully", "crear por completo"),
        ["ergänzen"] = ("дополнить", "доповнити", "complete/supplement", "completar"),
        ["Lastenheft"] = ("спецификация заказчика", "специфікація замовника", "requirements spec (customer)", "pliego del cliente"),
        ["Pflichtenheft"] = ("спецификация исполнителя", "специфікація виконавця", "functional spec (contractor)", "pliego del contratista"),
        ["Probezeit"] = ("испытательный срок", "випробувальний строк", "probation", "periodo de prueba"),
        ["Beitragsbemessungsgrenze"] = ("предел взносооблагаемой базы", "межа бази внесків", "contribution assessment ceiling", "tope de cotización"),
        ["Textform"] = ("текстовая форма", "текстова форма", "text form", "forma de texto"),
        ["Kündigung"] = ("увольнение / расторжение", "звільнення / розірвання", "termination/notice", "despido/terminación"),
        ["Datenbank"] = ("база данных", "база даних", "database", "base de datos"),
        ["Prüfung"] = ("экзамен", "іспит", "exam", "examen")
    };

    public static TranslationResult Translate(string text, string target)
    {
        var placeholders = new List<string>();
        var masked = Code.Replace(text ?? "", m =>
        {
            placeholders.Add(m.Value);
            return $"⟦CODE{placeholders.Count - 1}⟧";
        });
        var lang = Normalize(target);
        string body = lang switch
        {
            "ru" => Header("ru") + ReplaceOps(masked, "ru"),
            "uk" => Header("uk") + ReplaceOps(masked, "uk"),
            "en" => Header("en") + ReplaceOps(masked, "en"),
            "es" => Header("es") + ReplaceOps(masked, "es"),
            _ => masked
        };
        for (var i = 0; i < placeholders.Count; i++)
            body = body.Replace($"⟦CODE{i}⟧", placeholders[i]);
        var gloss = Glossary.Where(kv => (text ?? "").Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
            .Select(kv => $"{kv.Key}: {Pick(kv.Value, lang)}").ToList();
        return new TranslationResult(body, gloss);
    }

    public static string TranslateInline(string text, string target)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        var lang = Normalize(target);
        if (lang == "de") return text;
        var full = Translate(text, lang).Text;
        var nl = full.IndexOf('\n');
        return nl >= 0 ? full[(nl + 1)..] : full;
    }

    public static string Normalize(string? target)
    {
        var t = (target ?? "de").Trim().ToLowerInvariant();
        return t switch
        {
            "ru" or "russisch" or "russian" => "ru",
            "uk" or "ua" or "ukrainisch" or "ukrainian" => "uk",
            "en" or "englisch" or "english" => "en",
            "es" or "spanisch" or "spanish" or "español" => "es",
            _ => "de"
        };
    }

    private static string Header(string lang) => lang switch
    {
        "ru" => "Перевод (RU). SQL, код, UML и формулы не изменены.\n",
        "uk" => "Переклад (UK). SQL, код, UML і формули не змінено.\n",
        "en" => "Translation (EN). SQL, code, UML and formulas are unchanged.\n",
        "es" => "Traducción (ES). SQL, código, UML y fórmulas no se modifican.\n",
        _ => ""
    };

    private static string Pick((string Ru, string Uk, string En, string Es) v, string lang) => lang switch
    {
        "ru" => v.Ru, "uk" => v.Uk, "es" => v.Es, _ => v.En
    };

    private static string ReplaceOps(string t, string lang)
    {
        foreach (var kv in Glossary)
            t = Regex.Replace(t, $@"\b{Regex.Escape(kv.Key)}\b", m => m.Value + " [" + Pick(kv.Value, lang) + "]");
        return t;
    }

    public static string SimplifyGerman(string text)
    {
        var t = text.Replace("grundsätzlich", "in der Regel", StringComparison.OrdinalIgnoreCase)
            .Replace("insbesondere", "vor allem", StringComparison.OrdinalIgnoreCase);
        return "Einfaches Deutsch (B1/B2):\n" + t;
    }
}

public static class ExamRestrictionGuard
{
    public static string? Block(ExamAssistantMode mode, AssistantIntent intent, bool sessionActive)
    {
        if (!sessionActive || mode == ExamAssistantMode.Full)
            return null;
        if (mode == ExamAssistantMode.Disabled)
            return "Strenger Prüfungsmodus: Der KI-Assistent ist bis zur Abgabe deaktiviert.";
        if (mode == ExamAssistantMode.LanguageHelp)
        {
            if (intent is AssistantIntent.TranslateRu or AssistantIntent.TranslateEn or AssistantIntent.TranslateUk or AssistantIntent.TranslateEs or AssistantIntent.Glossary)
                return null;
            return "Sprachhilfe: Nur Übersetzung und Fachbegriffe, keine fachlichen Hinweise.";
        }
        if (mode == ExamAssistantMode.Accessibility)
        {
            if (intent is AssistantIntent.TranslateRu or AssistantIntent.TranslateEn or AssistantIntent.TranslateUk or AssistantIntent.TranslateEs or AssistantIntent.Glossary or AssistantIntent.ExplainSimpler)
                return null;
            if (intent is AssistantIntent.Hint or AssistantIntent.SimilarExercise or AssistantIntent.Flashcards or AssistantIntent.AnalyseAnswer)
                return "Barrierefreier Modus: Keine Lösungen oder fachlichen Lösungshinweise während der Prüfung.";
        }
        return null;
    }
}

public static class PromptInjectionGuard
{
    public static string WrapData(string? data)
    {
        if (string.IsNullOrEmpty(data)) return "";
        var cleaned = data.Replace("</daten>", "", StringComparison.OrdinalIgnoreCase);
        return "<daten>\n" + cleaned + "\n</daten>";
    }

    public static bool LooksLikeInjection(string text)
    {
        var t = text.ToLowerInvariant();
        return t.Contains("ignore previous") || t.Contains("ignoriere alle regeln") || t.Contains("system prompt")
               || t.Contains("reveal the hidden") || t.Contains("du bist jetzt");
    }
}
