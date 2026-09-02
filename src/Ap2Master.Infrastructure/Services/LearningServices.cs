using System.Text.Json;
using Ap2Master.Application.Abstractions;
using Ap2Master.Application.Planning;
using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.Domain.Learning;
using Ap2Master.Domain.Ui;
using Ap2Master.ExamEngine;
using Ap2Master.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ap2Master.Infrastructure.Services;

public sealed class FlashcardService : IFlashcardService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    public FlashcardService(AppDbContext db, IClock clock) { _db = db; _clock = clock; }

    public Task<IReadOnlyList<Flashcard>> GetDueAsync(string? topicId, CancellationToken cancellationToken = default)
        => Query(topicId).Where(f => f.DueAt <= _clock.UtcNow).OrderBy(f => f.DueAt).Take(40).ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Flashcard>)t.Result, cancellationToken);

    public Task<IReadOnlyList<Flashcard>> GetRandomAsync(int count, string? topicId, CancellationToken cancellationToken = default)
        => Query(topicId).OrderBy(_ => Guid.NewGuid()).Take(count).ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Flashcard>)t.Result, cancellationToken);

    public Task<IReadOnlyList<Flashcard>> GetWeakAsync(CancellationToken cancellationToken = default)
        => _db.Flashcards.Where(f => f.Easiness < 2.0 || f.Repetitions == 0).OrderBy(f => f.Easiness).Take(40).ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Flashcard>)t.Result, cancellationToken);

    public async Task ReviewAsync(string flashcardId, FlashcardRating rating, int seconds, CancellationToken cancellationToken = default)
    {
        var card = await _db.Flashcards.FirstAsync(f => f.Id == flashcardId, cancellationToken);
        var next = Sm2Scheduler.Review(card.Easiness, card.Repetitions, card.IntervalDays, rating, _clock.UtcNow);
        card.Easiness = next.Easiness;
        card.Repetitions = next.Repetitions;
        card.IntervalDays = next.IntervalDays;
        card.DueAt = next.DueAt;
        card.LastReviewedAt = _clock.UtcNow;
        _db.FlashcardReviews.Add(new FlashcardReview
        {
            Id = Guid.NewGuid(),
            FlashcardId = card.Id,
            ReviewedAt = _clock.UtcNow,
            Rating = rating,
            SecondsSpent = seconds
        });
        var ach = await _db.Achievements.FirstOrDefaultAsync(a => a.Id == "first-card", cancellationToken);
        if (ach is { UnlockedAt: null }) ach.UnlockedAt = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ToggleFavoriteAsync(string flashcardId, CancellationToken cancellationToken = default)
    {
        var card = await _db.Flashcards.FirstAsync(f => f.Id == flashcardId, cancellationToken);
        card.IsFavorite = !card.IsFavorite;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveNotesAsync(string flashcardId, string notes, CancellationToken cancellationToken = default)
    {
        var card = await _db.Flashcards.FirstAsync(f => f.Id == flashcardId, cancellationToken);
        card.PersonalNotes = notes;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ImportJsonAsync(string json, CancellationToken cancellationToken = default)
    {
        var incoming = JsonSerializer.Deserialize<List<Flashcard>>(json) ?? [];
        var existingFronts = await _db.Flashcards.Select(f => f.Front).ToListAsync(cancellationToken);
        var added = 0;
        foreach (var c in incoming)
        {
            if (existingFronts.Contains(c.Front, StringComparer.OrdinalIgnoreCase)) continue;
            c.Id = string.IsNullOrWhiteSpace(c.Id) ? Guid.NewGuid().ToString("N") : c.Id;
            _db.Flashcards.Add(c);
            added++;
        }
        await _db.SaveChangesAsync(cancellationToken);
        return added;
    }

    public async Task<string> ExportJsonAsync(CancellationToken cancellationToken = default)
    {
        var all = await _db.Flashcards.AsNoTracking().ToListAsync(cancellationToken);
        return JsonSerializer.Serialize(all, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<IReadOnlyList<ExamPartBoard>> GetBoardsAsync(CancellationToken cancellationToken = default)
    {
        var topics = await _db.Topics.AsNoTracking().OrderBy(t => t.SortOrder).ToListAsync(cancellationToken);
        var now = _clock.UtcNow;
        var rows = await _db.Flashcards.AsNoTracking()
            .GroupBy(f => f.TopicId)
            .Select(g => new { g.Key, Total = g.Count(), Due = g.Count(f => f.DueAt <= now) })
            .ToListAsync(cancellationToken);
        var stats = rows.ToDictionary(r => r.Key, r => new TopicCardStats(r.Total, r.Due), StringComparer.Ordinal);
        return TopicBoardBuilder.Build(topics, stats);
    }

    public async Task<IReadOnlyList<Flashcard>> BuildQueueAsync(
        FlashcardQueueKind kind,
        IReadOnlyList<string>? topicIds,
        int take = 40,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 80);
        IQueryable<Flashcard> q = _db.Flashcards;
        if (topicIds is { Count: > 0 })
            q = q.Where(f => topicIds.Contains(f.TopicId));

        q = kind switch
        {
            FlashcardQueueKind.Due => q.Where(f => f.DueAt <= _clock.UtcNow).OrderBy(f => f.DueAt),
            FlashcardQueueKind.Weak => q.Where(f => f.Easiness < 2.0 || f.Repetitions == 0).OrderBy(f => f.Easiness),
            _ => q.OrderBy(_ => Guid.NewGuid())
        };

        return await q.Take(take).ToListAsync(cancellationToken);
    }

    private IQueryable<Flashcard> Query(string? topicId)
        => string.IsNullOrWhiteSpace(topicId) ? _db.Flashcards : _db.Flashcards.Where(f => f.TopicId == topicId);
}

public sealed class MistakeBookService : IMistakeBookService
{
    private readonly AppDbContext _db;
    public MistakeBookService(AppDbContext db) => _db = db;
    public async Task AddAsync(MistakeEntry entry, CancellationToken cancellationToken = default)
    {
        _db.MistakeEntries.Add(entry);
        await _db.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<MistakeEntry>> GetOpenAsync(CancellationToken cancellationToken = default)
        => await _db.MistakeEntries.Where(m => !m.IsCorrected).OrderByDescending(m => m.OccurredAt).ToListAsync(cancellationToken);
    public async Task MarkCorrectedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var e = await _db.MistakeEntries.FirstAsync(x => x.Id == id, cancellationToken);
        e.IsCorrected = true;
        e.Repetitions++;
        await _db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class LearningPlanAppService : ILearningPlanService
{
    private readonly AppDbContext _db;
    private readonly LearningPlanService _core;
    private readonly IClock _clock;
    public LearningPlanAppService(AppDbContext db, LearningPlanService core, IClock clock)
    { _db = db; _core = core; _clock = clock; }

    public async Task<LearningPlan> CreateOrRebuildAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        var old = await _db.LearningPlans.Include(p => p.Activities).Where(p => p.IsActive).ToListAsync(cancellationToken);
        foreach (var p in old) p.IsActive = false;
        var topics = await _db.Topics.AsNoTracking().ToListAsync(cancellationToken);
        var plan = _core.Build(settings, topics, _clock.UtcNow);
        _db.LearningPlans.Add(plan);
        await _db.SaveChangesAsync(cancellationToken);
        return plan;
    }

    public async Task RescheduleMissedAsync(CancellationToken cancellationToken = default)
    {
        var plan = await _db.LearningPlans.Include(p => p.Activities).FirstOrDefaultAsync(p => p.IsActive, cancellationToken);
        if (plan is null) return;
        _core.RescheduleMissed(plan, DateOnly.FromDateTime(_clock.UtcNow));
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAsync(Guid activityId, CancellationToken cancellationToken = default)
    {
        var a = await _db.PlannedActivities.FirstAsync(x => x.Id == activityId, cancellationToken);
        a.IsCompleted = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MoveAsync(Guid activityId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var a = await _db.PlannedActivities.FirstAsync(x => x.Id == activityId, cancellationToken);
        a.Date = date;
        a.IsMissed = false;
        await _db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    public DashboardService(AppDbContext db, IClock clock) { _db = db; _clock = clock; }

    public async Task<DashboardSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _db.UserSettings.FirstAsync(cancellationToken);
        var topics = await _db.Topics.CountAsync(cancellationToken);
        var touched = await _db.UserAnswers.Select(a => a.TopicId).Distinct().CountAsync(cancellationToken)
                      + await _db.FlashcardReviews.Include(r => r.Flashcard).Select(r => r.Flashcard!.TopicId).Distinct().CountAsync(cancellationToken);
        var due = await _db.Flashcards.CountAsync(f => f.DueAt <= _clock.UtcNow, cancellationToken);
        var today = DateOnly.FromDateTime(_clock.UtcNow);
        var tasksToday = await _db.UserAnswers.CountAsync(a => DateOnly.FromDateTime(a.AnsweredAt) == today, cancellationToken);
        var weekStart = _clock.UtcNow.Date.AddDays(-(int)_clock.UtcNow.DayOfWeek + 1);
        var weekly = await _db.StudySessions.Where(s => s.StartedAt >= weekStart).SumAsync(s => (int?)s.SecondsSpent, cancellationToken) ?? 0;
        var answers = await _db.UserAnswers.ToListAsync(cancellationToken);
        var avg = answers.Count == 0 ? 0 : answers.Average(a => a.PointsPossible == 0 ? 0 : a.PointsAwarded * 100.0 / a.PointsPossible);
        var byTopic = answers.GroupBy(a => a.TopicId).ToDictionary(g => g.Key, g => g.Average(x => x.IsCorrect ? 1.0 : 0.0));
        var weak = byTopic.OrderBy(kv => kv.Value).Take(5).Select(kv => kv.Key).ToList();
        var strong = byTopic.OrderByDescending(kv => kv.Value).Take(5).Select(kv => kv.Key).ToList();
        var mistakes = await _db.MistakeEntries.OrderByDescending(m => m.OccurredAt).Take(5).ToListAsync(cancellationToken);
        var upcoming = await _db.PlannedActivities.Where(a => a.Date >= today && !a.IsCompleted).OrderBy(a => a.Date).Take(8).ToListAsync(cancellationToken);
        var exams = await _db.ExamResults.OrderByDescending(e => e.CompletedAt).Take(5).ToListAsync(cancellationToken);
        var mock = exams.Count == 0 ? 0 : exams.Average(e => e.ScorePercent) / 100.0;
        var recency = answers.Count == 0 ? 0 : answers.Count(a => a.AnsweredAt > _clock.UtcNow.AddDays(-14)) / (double)Math.Max(1, answers.Count);
        var cards = await _db.Flashcards.ToListAsync(cancellationToken);
        var recall = cards.Count == 0 ? 0 : cards.Average(c => Math.Min(1, c.Repetitions / 5.0) * (c.Easiness / 3.0));
        var required = await _db.Topics.CountAsync(t => t.IsPriority, cancellationToken);
        var requiredDone = byTopic.Keys.Count;
        var breakdown = ReadinessCalculator.Calculate(
            topicCoverage: topics == 0 ? 0 : Math.Min(1, (double)touched / Math.Max(1, topics)),
            recency: recency,
            flashcardRecall: Math.Clamp(recall, 0, 1),
            exerciseAccuracy: avg / 100.0,
            difficultyDistribution: answers.Select(a => a.Difficulty).Distinct().Count() / 3.0,
            mockExamScores: mock,
            timeManagement: exams.Count == 0 ? 0.4 : exams.Average(e => e.ActualMinutes <= e.ExpectedMinutes ? 1 : 0.5),
            weakSubjectImprovement: weak.Count == 0 ? 0.5 : 0.4,
            requiredTopicCompletion: required == 0 ? 1 : Math.Min(1, requiredDone / (double)required),
            previousScore: settings.LastReadinessScore);

        settings.LastReadinessScore = breakdown.Score;
        settings.LastReadinessReasonsJson = JsonSerializer.Serialize(breakdown.Reasons);
        _db.ReadinessSnapshots.Add(new ReadinessSnapshot
        {
            Id = Guid.NewGuid(),
            TakenAt = _clock.UtcNow,
            Score = breakdown.Score,
            BreakdownJson = JsonSerializer.Serialize(breakdown),
            ChangeReasonsJson = JsonSerializer.Serialize(breakdown.Reasons)
        });
        await _db.SaveChangesAsync(cancellationToken);

        var days = new HashSet<DateOnly>(answers.Select(a => DateOnly.FromDateTime(a.AnsweredAt)));
        var streak = 0;
        for (var d = today; days.Contains(d); d = d.AddDays(-1)) streak++;

        var recommended = due > 0 ? "Fällige Lernkarten wiederholen" :
            upcoming.FirstOrDefault()?.Title ?? "Themenkatalog durchgehen";

        int? until = settings.ExamDate is { } exam ? (int)(exam.Date - _clock.UtcNow.Date).TotalDays : null;

        var topicRows = await _db.Topics.AsNoTracking().OrderBy(t => t.SortOrder).ToListAsync(cancellationToken);
        ExamPartProgress PartOf(ExamPart part)
        {
            var ids = topicRows.Where(t => t.ExamPart == part).Select(t => t.Id).ToHashSet(StringComparer.Ordinal);
            var slice = answers.Where(a => ids.Contains(a.TopicId)).ToList();
            var awarded = slice.Sum(a => a.PointsAwarded);
            var possible = slice.Sum(a => a.PointsPossible);
            var pct = possible == 0 ? 0 : (int)Math.Round(awarded * 100.0 / possible);
            return new ExamPartProgress(part, pct, awarded, possible);
        }
        var parts = new[] { PartOf(ExamPart.Teil1), PartOf(ExamPart.Teil2), PartOf(ExamPart.WiSo) };

        var monday = today.AddDays(today.DayOfWeek == DayOfWeek.Sunday ? -6 : -(int)today.DayOfWeek + 1);
        var weekStartDt = monday.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var sessions = await _db.StudySessions.AsNoTracking().Where(s => s.StartedAt >= weekStartDt).ToListAsync(cancellationToken);
        var dayLabels = new[] { "Mo", "Di", "Mi", "Do", "Fr", "Sa", "So" };
        var rawDays = Enumerable.Range(0, 7).Select(i =>
        {
            var day = monday.AddDays(i);
            var mins = sessions.Where(s => DateOnly.FromDateTime(s.StartedAt) == day).Sum(s => s.SecondsSpent) / 60;
            return (Label: dayLabels[i], Minutes: mins);
        }).ToList();
        var maxMins = Math.Max(1, rawDays.Max(d => d.Minutes));
        var weekBars = rawDays.Select(d => new WeekDayMinutes(d.Label, d.Minutes, d.Minutes / (double)maxMins)).ToList();

        var clusters = topicRows.Where(t => t.ParentTopicId is "t1" or "t2" or "t3").Take(8).ToList();
        var heatmap = clusters.Select(t =>
        {
            var m = byTopic.TryGetValue(t.Id, out var v) ? v : 0;
            var kids = topicRows.Where(c => c.ParentTopicId == t.Id).Select(c => c.Id);
            var kidAnswers = answers.Where(a => a.TopicId == t.Id || kids.Contains(a.TopicId)).ToList();
            if (kidAnswers.Count > 0) m = kidAnswers.Average(a => a.IsCorrect ? 1.0 : 0.0);
            return new TopicHeatCell(t.Id, t.Title, t.ExamPart, m, TopicHeatBand.LocKey(m), TopicHeatBand.Column(m));
        }).ToList();

        var trend = await _db.ReadinessSnapshots.AsNoTracking().OrderByDescending(s => s.TakenAt).Take(8)
            .ToListAsync(cancellationToken);
        trend.Reverse();

        return new DashboardSnapshot(
            breakdown, due, tasksToday, weekly / 60, avg, streak, weak, strong, mistakes, upcoming,
            settings.ExamDate, until, recommended, settings.DisplayName, parts, weekBars, heatmap,
            trend.Select(s => (s.TakenAt, s.Score)).ToList());
    }
}

public sealed class StatisticsService : IStatisticsService
{
    private readonly AppDbContext _db;
    public StatisticsService(AppDbContext db) => _db = db;

    public async Task<StatisticsSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        var answers = await _db.UserAnswers.ToListAsync(cancellationToken);
        var topics = await _db.Topics.ToListAsync(cancellationToken);
        var contents = await _db.LearningContents.AsNoTracking().ToListAsync(cancellationToken);
        var cards = await _db.Flashcards.AsNoTracking().ToListAsync(cancellationToken);
        var mastery = topics.ToDictionary(t => t.Id, t =>
        {
            var a = answers.Where(x => x.TopicId == t.Id).ToList();
            return a.Count == 0 ? 0 : a.Average(x => x.IsCorrect ? 1.0 : 0.0);
        });
        var bySubject = answers.GroupBy(a => a.TopicId).ToDictionary(g => g.Key, g => g.Average(x => x.IsCorrect ? 1.0 : 0.0));
        var byDiff = Enum.GetValues<Difficulty>().ToDictionary(d => d, d =>
        {
            var a = answers.Where(x => x.Difficulty == d).ToList();
            return a.Count == 0 ? 0 : a.Average(x => x.IsCorrect ? 1.0 : 0.0);
        });
        var times = answers.Select(a => a.SecondsSpent).ToList();
        var exams = await _db.ExamResults.OrderBy(e => e.CompletedAt).Select(e => new ValueTuple<DateTime, int>(e.CompletedAt, e.ScorePercent)).ToListAsync(cancellationToken);
        var retention = cards.Count == 0 ? 0 : cards.Average(c => c.Repetitions >= 2 ? 1.0 : c.Repetitions * 0.4);
        var mistakes = await _db.MistakeEntries.GroupBy(m => m.MistakeType).ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
        var weekly = await _db.StudySessions.ToListAsync(cancellationToken);
        var weekGroups = weekly.GroupBy(s => DateOnly.FromDateTime(s.StartedAt.Date.AddDays(-(int)s.StartedAt.DayOfWeek + 1)))
            .Select(g => (g.Key, g.Sum(x => x.SecondsSpent) / 60)).ToList();
        var trend = await _db.ReadinessSnapshots.OrderBy(s => s.TakenAt).Select(s => new ValueTuple<DateTime, int>(s.TakenAt, s.Score)).ToListAsync(cancellationToken);
        var due = await _db.Flashcards.CountAsync(f => f.DueAt <= DateTime.UtcNow, cancellationToken);
        var coverage = topics.Select(t => new TopicCoverageRow(
            t.Id, t.Title, t.ExamPart,
            contents.Any(c => c.TopicId == t.Id && c.ContentType == ContentType.Theory),
            cards.Any(c => c.TopicId == t.Id),
            contents.Any(c => c.TopicId == t.Id && c.ContentType is ContentType.SingleChoice or ContentType.Sql or ContentType.Calculation or ContentType.MockExamTask),
            mastery.GetValueOrDefault(t.Id))).ToList();

        return new StatisticsSnapshot(mastery, bySubject, byDiff, times, exams, retention, mistakes, weekGroups, trend, due, coverage);
    }
}
