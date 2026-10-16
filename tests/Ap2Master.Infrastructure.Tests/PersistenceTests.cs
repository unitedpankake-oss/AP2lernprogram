using Ap2Master.Application.Abstractions;
using Ap2Master.Content;
using Ap2Master.Domain;
using Ap2Master.Infrastructure;
using Ap2Master.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Ap2Master.Infrastructure.Tests;

[Collection("sqlite")]
public class PersistenceTests
{
    private static async Task<(ServiceProvider Sp, string Path)> CreateAsync()
    {
        var path = "t" + Guid.NewGuid().ToString("N");
        var sc = new ServiceCollection();
        sc.AddAp2Master(path);
        var sp = sc.BuildServiceProvider();
        await sp.GetRequiredService<IAppDatabase>().InitializeAsync();
        return (sp, path);
    }

    private static void Release(string databaseName)
    {
        SqliteConnection.ClearAllPools();
        new MongoClient(MongoSettings.ConnectionString).DropDatabase(databaseName);
        var lab = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ap2Master", databaseName);
        try { if (Directory.Exists(lab)) Directory.Delete(lab, true); } catch (IOException) { }
    }

    [Fact]
    public async Task Seed_creates_topics_and_survives_reopen()
    {
        var (sp, path) = await CreateAsync();
        var db = sp.GetRequiredService<AppDbContext>();
        (await db.Topics.CountAsync()).Should().Be(TopicCatalog.All.Count);
        (await db.Flashcards.CountAsync()).Should().BeGreaterThan(100);
        (await db.UserSettings.CountAsync()).Should().Be(1);
        var cards = sp.GetRequiredService<IFlashcardService>();
        var due = await cards.GetDueAsync(null);
        due.Should().NotBeEmpty();
        await cards.ReviewAsync(due[0].Id, FlashcardRating.Gut, 8);
        await sp.DisposeAsync();
        SqliteConnection.ClearAllPools();

        var sc2 = new ServiceCollection();
        sc2.AddAp2Master(path);
        await using var sp2 = sc2.BuildServiceProvider();
        await sp2.GetRequiredService<IAppDatabase>().InitializeAsync();
        var db2 = sp2.GetRequiredService<AppDbContext>();
        (await db2.FlashcardReviews.CountAsync()).Should().Be(1);
        await sp2.DisposeAsync();
        Release(path);
    }

    [Fact]
    public async Task Sql_lab_compares_functionally_not_by_string()
    {
        var (sp, path) = await CreateAsync();
        var sql = sp.GetRequiredService<ISqlLabService>();
        await sql.ResetAsync();
        var a = "SELECT Name FROM Kunde ORDER BY Id";
        var b = "select name from kunde order by id";
        (await sql.MatchesExpectedAsync(a, b, true)).Should().BeTrue();
        await sp.DisposeAsync();
        Release(path);
    }

    [Fact]
    public async Task Backup_and_mistake_book_roundtrip()
    {
        var (sp, path) = await CreateAsync();
        var mistakes = sp.GetRequiredService<IMistakeBookService>();
        await mistakes.AddAsync(new Domain.Entities.MistakeEntry
        {
            Id = Guid.NewGuid(), TopicId = "t3-textform", TaskText = "Form?", LearnerAnswer = "nur Unterschrift",
            CorrectAnswer = "Textform", Explanation = "§ 11", OccurredAt = DateTime.UtcNow, MistakeType = MistakeType.VeralteteRegelVerwechselt
        });
        (await mistakes.GetOpenAsync()).Should().HaveCount(1);
        var backup = sp.GetRequiredService<IBackupService>();
        var zip = await backup.BackupAsync();
        File.Exists(zip).Should().BeTrue();
        await sp.DisposeAsync();
        Release(path);
    }

    [Fact]
    public async Task Exam_session_can_be_restored()
    {
        var (sp, path) = await CreateAsync();
        var engine = sp.GetRequiredService<IExamEngine>();
        var session = await engine.StartAsync("exam-wiso");
        var ids = System.Text.Json.JsonSerializer.Deserialize<List<string>>(session.QuestionIdsJson);
        ids.Should().HaveCount(30);
        await engine.SaveAsync(session);
        var restored = await engine.RestoreOpenAsync(ExamKind.WiSo);
        restored.Should().NotBeNull();
        restored!.Id.Should().Be(session.Id);
        await sp.DisposeAsync();
        Release(path);
    }

    [Fact]
    public async Task Sql_schema_tree_lists_tables_and_keys()
    {
        var (sp, path) = await CreateAsync();
        var sql = sp.GetRequiredService<ISqlLabService>();
        await sql.ResetAsync();
        var tree = await sql.GetSchemaTreeAsync();
        tree.Should().Contain(t => t.Name == "Ticket");
        var ticket = tree.Single(t => t.Name == "Ticket");
        ticket.Columns.Should().Contain(c => c.Name == "Id" && c.IsPrimaryKey);
        ticket.Columns.Should().Contain(c => c.Name == "KundenId" && c.IsForeignKey && c.ForeignTable == "Kunde");
        await sp.DisposeAsync();
        Release(path);
    }
}
