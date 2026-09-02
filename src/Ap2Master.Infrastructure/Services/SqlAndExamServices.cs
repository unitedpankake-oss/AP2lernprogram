using System.Text.Json;
using Ap2Master.Application.Abstractions;
using Ap2Master.Content;
using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.Domain.Evaluation;
using Ap2Master.ExamEngine;
using Ap2Master.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ap2Master.Infrastructure.Services;

public sealed class SqlLabService : ISqlLabService
{
    private readonly AppDbContext _db;
    private readonly string _labPath;
    private SqliteConnection? _openTx;

    public SqlLabService(AppDbContext db)
    {
        _db = db;
        var dataSource = db.Database.GetDbConnection().DataSource;
        var dir = Path.GetDirectoryName(dataSource);
        if (string.IsNullOrWhiteSpace(dir))
            dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ap2Master");
        Directory.CreateDirectory(dir);
        _labPath = Path.Combine(dir, "helpdesk.db");
    }

    private string LabConnectionString => $"Data Source={_labPath};Pooling=False";

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        _openTx?.Dispose();
        _openTx = null;
        SqliteConnection.ClearAllPools();
        if (File.Exists(_labPath)) File.Delete(_labPath);
        await using var con = new SqliteConnection(LabConnectionString);
        await con.OpenAsync(cancellationToken);
        await using var cmd = con.CreateCommand();
        cmd.CommandText = HelpDeskSchema.Sql;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetSchemaAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAsync(cancellationToken);
        await using var con = new SqliteConnection(LabConnectionString);
        await con.OpenAsync(cancellationToken);
        await using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT name, sql FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite%' ORDER BY name;";
        var list = new List<string>();
        await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await r.ReadAsync(cancellationToken))
            list.Add($"{r.GetString(0)}\n{r.GetString(1)}");
        return list;
    }

    public async Task<IReadOnlyList<SqlTableInfo>> GetSchemaTreeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAsync(cancellationToken);
        await using var con = new SqliteConnection(LabConnectionString);
        await con.OpenAsync(cancellationToken);
        await using var namesCmd = con.CreateCommand();
        namesCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite%' ORDER BY name;";
        var names = new List<string>();
        await using (var reader = await namesCmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
                names.Add(reader.GetString(0));
        }

        var tables = new List<SqlTableInfo>(names.Count);
        foreach (var name in names)
        {
            var quoted = QuoteIdent(name);
            var fks = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            await using (var fkCmd = con.CreateCommand())
            {
                fkCmd.CommandText = "PRAGMA foreign_key_list(" + quoted + ");";
                await using var fkReader = await fkCmd.ExecuteReaderAsync(cancellationToken);
                while (await fkReader.ReadAsync(cancellationToken))
                {
                    var from = fkReader["from"]?.ToString() ?? "";
                    var table = fkReader["table"]?.ToString();
                    if (from.Length > 0) fks[from] = table ?? "";
                }
            }

            var columns = new List<SqlColumnInfo>();
            await using (var colCmd = con.CreateCommand())
            {
                colCmd.CommandText = "PRAGMA table_info(" + quoted + ");";
                await using var colReader = await colCmd.ExecuteReaderAsync(cancellationToken);
                while (await colReader.ReadAsync(cancellationToken))
                {
                    var colName = colReader["name"]?.ToString() ?? "";
                    var type = colReader["type"]?.ToString() ?? "";
                    var notNull = Convert.ToInt32(colReader["notnull"]) != 0;
                    var pk = Convert.ToInt32(colReader["pk"]) != 0;
                    var isFk = fks.TryGetValue(colName, out var fkTable);
                    columns.Add(new SqlColumnInfo(colName, type, notNull, pk, isFk, isFk ? fkTable : null));
                }
            }

            tables.Add(new SqlTableInfo(name, columns));
        }

        return tables;
    }

    private static string QuoteIdent(string name) => "\"" + name.Replace("\"", "\"\"") + "\"";

    public async Task<string?> ExplainAsync(string sql, CancellationToken cancellationToken = default)
    {
        await EnsureAsync(cancellationToken);
        try
        {
            await using var con = new SqliteConnection(LabConnectionString);
            await con.OpenAsync(cancellationToken);
            await using var cmd = con.CreateCommand();
            cmd.CommandText = "EXPLAIN QUERY PLAN " + sql;
            await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
            var rows = new List<string>();
            while (await r.ReadAsync(cancellationToken))
                rows.Add(string.Join(" | ", Enumerable.Range(0, r.FieldCount).Select(i => r.IsDBNull(i) ? "" : r.GetValue(i)?.ToString())));
            return string.Join(Environment.NewLine, rows);
        }
        catch (Exception ex) { return ex.Message; }
    }

    public async Task<SqlExecutionResult> ExecuteAsync(string sql, bool previewMutations, CancellationToken cancellationToken = default)
    {
        await EnsureAsync(cancellationToken);
        var trimmed = sql.Trim();
        var isMut = StartsMutation(trimmed);
        if (isMut && previewMutations)
        {
            return new SqlExecutionResult(true, null, ["Hinweis"], [["Vorschau: Die Änderung wird nicht ausgeführt. Entfernen Sie den Vorschaumodus, um COMMIT zu erlauben."]], 0, true,
                "UPDATE/DELETE/INSERT im Vorschaumodus blockiert.");
        }
        try
        {
            await using var con = new SqliteConnection(LabConnectionString);
            await con.OpenAsync(cancellationToken);
            await using var cmd = con.CreateCommand();
            cmd.CommandText = trimmed;
            if (trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("WITH", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("PRAGMA", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("EXPLAIN", StringComparison.OrdinalIgnoreCase))
            {
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                var cols = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                var rows = new List<IReadOnlyList<string>>();
                while (await reader.ReadAsync(cancellationToken))
                    rows.Add(Enumerable.Range(0, reader.FieldCount).Select(i => reader.IsDBNull(i) ? "NULL" : Convert.ToString(reader.GetValue(i)) ?? "").ToList());
                _db.QueryHistory.Add(new QueryHistoryEntry { Id = Guid.NewGuid(), ExecutedAt = DateTime.UtcNow, Sql = trimmed, Succeeded = true, RowCount = rows.Count });
                await _db.SaveChangesAsync(cancellationToken);
                return new SqlExecutionResult(true, null, cols, rows, rows.Count, false, null);
            }

            var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
            _db.QueryHistory.Add(new QueryHistoryEntry { Id = Guid.NewGuid(), ExecutedAt = DateTime.UtcNow, Sql = trimmed, Succeeded = true, RowCount = affected });
            await _db.SaveChangesAsync(cancellationToken);
            return new SqlExecutionResult(true, null, ["BetroffeneZeilen"], [[affected.ToString()]], affected, isMut, null);
        }
        catch (Exception ex)
        {
            _db.QueryHistory.Add(new QueryHistoryEntry { Id = Guid.NewGuid(), ExecutedAt = DateTime.UtcNow, Sql = trimmed, Succeeded = false, Error = ex.Message });
            await _db.SaveChangesAsync(cancellationToken);
            return new SqlExecutionResult(false, ex.Message, Array.Empty<string>(), Array.Empty<IReadOnlyList<string>>(), 0, isMut, null);
        }
    }

    public async Task<bool> MatchesExpectedAsync(string learnerSql, string expectedSql, bool requireOrder, CancellationToken cancellationToken = default)
    {
        var a = await ExecuteAsync(learnerSql, false, cancellationToken);
        var b = await ExecuteAsync(expectedSql, false, cancellationToken);
        if (!a.Success || !b.Success) return false;
        var mapA = a.Rows.Select(r => Enumerable.Range(0, a.Columns.Count).ToDictionary(i => a.Columns[i], i => (object?)r[i])).ToList();
        var mapB = b.Rows.Select(r => Enumerable.Range(0, b.Columns.Count).ToDictionary(i => b.Columns[i], i => (object?)r[i])).ToList();
        return SqlResultComparer.AreEqual(mapB, mapA, requireOrder);
    }

    private async Task EnsureAsync(CancellationToken ct)
    {
        if (!File.Exists(_labPath)) await ResetAsync(ct);
    }

    private static bool StartsMutation(string sql)
    {
        var s = sql.TrimStart().ToUpperInvariant();
        return s.StartsWith("UPDATE") || s.StartsWith("DELETE") || s.StartsWith("INSERT") || s.StartsWith("DROP") || s.StartsWith("ALTER") || s.StartsWith("CREATE");
    }
}

public sealed class ExamEngineService : IExamEngine
{
    private readonly AppDbContext _db;
    private readonly ExamScoringService _scoring;
    private readonly IMistakeBookService _mistakes;
    private readonly IClock _clock;

    public ExamEngineService(AppDbContext db, ExamScoringService scoring, IMistakeBookService mistakes, IClock clock)
    {
        _db = db;
        _scoring = scoring;
        _mistakes = mistakes;
        _clock = clock;
    }

    public async Task<ExamSession> StartAsync(string examDefinitionId, CancellationToken cancellationToken = default)
    {
        var def = await _db.ExamDefinitions.FirstAsync(e => e.Id == examDefinitionId, cancellationToken);
        var pool = await _db.LearningContents
            .Include(c => c.Options)
            .Where(c => !c.IsQuarantined && c.VerificationStatus != VerificationStatus.Historical && c.VerificationStatus != VerificationStatus.Quarantined)
            .Where(c => c.ContentType == ContentType.SingleChoice || c.ContentType == ContentType.MultipleChoice || c.ContentType == ContentType.MockExamTask)
            .ToListAsync(cancellationToken);

        List<LearningContent> picked;
        if (def.Kind == ExamKind.WiSo)
        {
            picked = pool.Where(c => c.ExamPart == ExamPart.WiSo && c.ContentType is ContentType.SingleChoice or ContentType.MultipleChoice)
                .OrderBy(_ => Guid.NewGuid()).Take(30).ToList();
            ExamScoringService.EnsureWiSoIntegrity(picked);
        }
        else if (def.Kind == ExamKind.Teil1)
            picked = pool.Where(c => c.ExamPart == ExamPart.Teil1).OrderBy(_ => Guid.NewGuid()).Take(Math.Max(12, def.QuestionCount)).Take(def.QuestionCount).ToList();
        else if (def.Kind == ExamKind.Teil2)
            picked = pool.Where(c => c.ExamPart == ExamPart.Teil2).OrderBy(_ => Guid.NewGuid()).Take(def.QuestionCount).ToList();
        else if (def.Kind == ExamKind.TrainingDay)
        {
            var w = pool.Where(c => c.ExamPart == ExamPart.WiSo).OrderBy(_ => Guid.NewGuid()).Take(30);
            var t1 = pool.Where(c => c.ExamPart == ExamPart.Teil1).OrderBy(_ => Guid.NewGuid()).Take(12);
            var t2 = pool.Where(c => c.ExamPart == ExamPart.Teil2).OrderBy(_ => Guid.NewGuid()).Take(12);
            picked = w.Concat(t1).Concat(t2).ToList();
        }
        else
            picked = pool.OrderBy(_ => Guid.NewGuid()).Take(def.QuestionCount).ToList();

        if (picked.Count == 0) throw new InvalidOperationException("Keine Prüfungsaufgaben verfügbar.");

        var session = new ExamSession
        {
            Id = Guid.NewGuid(),
            ExamDefinitionId = def.Id,
            Kind = def.Kind,
            StartedAt = _clock.UtcNow,
            RemainingSeconds = def.DurationMinutes * 60,
            QuestionIdsJson = JsonSerializer.Serialize(picked.Select(p => p.Id).ToList()),
            AnswersJson = "{}",
            MarkedJson = "[]"
        };
        _db.ExamSessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task SaveAsync(ExamSession session, CancellationToken cancellationToken = default)
    {
        session.LastSavedAt = _clock.UtcNow;
        _db.ExamSessions.Update(session);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<ExamSession?> RestoreOpenAsync(ExamKind kind, CancellationToken cancellationToken = default)
        => _db.ExamSessions.FirstOrDefaultAsync(s => s.Kind == kind && !s.IsSubmitted, cancellationToken);

    public async Task<ExamResult> SubmitAsync(Guid sessionId, bool timedOut, CancellationToken cancellationToken = default)
    {
        var session = await _db.ExamSessions.FirstAsync(s => s.Id == sessionId, cancellationToken);
        session.IsSubmitted = true;
        session.TimedOut = timedOut;
        session.SubmittedAt = _clock.UtcNow;
        var ids = JsonSerializer.Deserialize<List<string>>(session.QuestionIdsJson) ?? [];
        var questions = await _db.LearningContents.Include(c => c.Options).Where(c => ids.Contains(c.Id)).ToListAsync(cancellationToken);
        var ordered = ids.Select(id => questions.First(q => q.Id == id)).ToList();
        var result = _scoring.Score(session, ordered, _clock.UtcNow);
        session.ScorePercent = result.ScorePercent;
        session.PointsAwarded = result.PointsAwarded;
        session.PointsPossible = result.PointsPossible;
        _db.ExamResults.Add(result);
        var answers = JsonSerializer.Deserialize<Dictionary<string, string>>(session.AnswersJson) ?? new();
        foreach (var q in ordered)
        {
            answers.TryGetValue(q.Id, out var given);
            if (!ExamScoringService.IsCorrect(q, given))
            {
                await _mistakes.AddAsync(new MistakeEntry
                {
                    Id = Guid.NewGuid(),
                    TopicId = q.TopicId,
                    TaskText = q.Question ?? q.Title,
                    LearnerAnswer = given ?? "",
                    CorrectAnswer = q.Answer ?? "",
                    Explanation = q.Explanation ?? "",
                    Difficulty = q.Difficulty,
                    OccurredAt = _clock.UtcNow,
                    MistakeType = MistakeType.Wissensluecke,
                    ExamSessionId = session.Id.ToString()
                }, cancellationToken);
            }
        }
        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }
}
