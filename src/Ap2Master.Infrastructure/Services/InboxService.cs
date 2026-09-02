using Ap2Master.Application.Abstractions;
using Ap2Master.Domain;
using Ap2Master.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ap2Master.Infrastructure.Services;

public sealed class InboxService : IInboxService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    public InboxService(AppDbContext db, IClock clock) { _db = db; _clock = clock; }

    public async Task<IReadOnlyList<InboxItem>> GetAsync(CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var items = new List<InboxItem>();

        var due = await _db.Flashcards.CountAsync(f => f.DueAt <= now, cancellationToken);
        if (due > 0)
            items.Add(new("due", "Fällige Lernkarten", due + " Karten warten auf Wiederholung.", AppSection.Lernkarten, "due"));

        var missed = await _db.PlannedActivities.CountAsync(a => a.IsMissed && !a.IsCompleted, cancellationToken);
        if (missed > 0)
            items.Add(new("missed", "Verpasste Lerneinheiten", missed + " Einheiten wurden neu geplant oder sind offen.", AppSection.Lernplan, "missed"));

        var mistakes = await _db.MistakeEntries.CountAsync(m => !m.IsCorrected, cancellationToken);
        if (mistakes > 0)
            items.Add(new("mistakes", "Offene Fehler", mistakes + " Einträge im Fehlerbuch.", AppSection.Fehlerbuch, "mistake"));

        var openExam = await _db.ExamSessions.AnyAsync(s => !s.IsSubmitted, cancellationToken);
        if (openExam)
            items.Add(new("exam", "Offene Probeklausur", "Eine Klausur ist noch nicht abgegeben.", AppSection.Probeklausuren, "exam"));

        var todayOpen = await _db.PlannedActivities.CountAsync(a => a.Date == today && !a.IsCompleted, cancellationToken);
        if (todayOpen > 0)
            items.Add(new("today", "Heutiger Plan", todayOpen + " Einheiten stehen heute noch aus.", AppSection.Lernplan, "plan"));

        return items;
    }
}
