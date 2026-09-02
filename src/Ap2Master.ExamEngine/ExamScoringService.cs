using System.Text.Json;
using Ap2Master.Application.Abstractions;
using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.Domain.Learning;

namespace Ap2Master.ExamEngine;

public sealed class ExamScoringService
{
    public ExamResult Score(ExamSession session, IReadOnlyList<LearningContent> questions, DateTime utcNow)
    {
        var answers = JsonSerializer.Deserialize<Dictionary<string, string>>(session.AnswersJson)
                      ?? new Dictionary<string, string>();
        var awarded = 0;
        var possible = 0;
        var byTopic = new Dictionary<string, (int ok, int total)>();

        foreach (var q in questions)
        {
            possible += Math.Max(1, q.Points);
            answers.TryGetValue(q.Id, out var given);
            var correct = IsCorrect(q, given);
            if (correct) awarded += Math.Max(1, q.Points);
            var t = byTopic.GetValueOrDefault(q.TopicId);
            byTopic[q.TopicId] = (t.ok + (correct ? 1 : 0), t.total + 1);
        }

        var percent = possible == 0 ? 0 : (int)Math.Round(awarded * 100.0 / possible);
        var expected = session.Kind switch
        {
            ExamKind.WiSo => 60,
            ExamKind.TrainingDay => 240,
            _ => 90
        };
        var actual = Math.Max(1, (int)Math.Round(((session.SubmittedAt ?? utcNow) - session.StartedAt).TotalMinutes));
        var stats = JsonSerializer.Serialize(byTopic.ToDictionary(kv => kv.Key, kv => kv.Value.total == 0 ? 0 : kv.Value.ok * 100 / kv.Value.total));

        return new ExamResult
        {
            Id = Guid.NewGuid(),
            ExamSessionId = session.Id,
            CompletedAt = utcNow,
            ScorePercent = percent,
            PointsAwarded = awarded,
            PointsPossible = possible,
            ExpectedMinutes = expected,
            ActualMinutes = actual,
            TopicStatsJson = stats,
            ReportText = BuildReport(session, percent, awarded, possible, expected, actual)
        };
    }

    public static bool IsCorrect(LearningContent question, string? given)
    {
        if (string.IsNullOrWhiteSpace(given)) return false;
        if (question.ContentType == ContentType.SingleChoice)
        {
            var expected = question.Options.FirstOrDefault(o => o.IsCorrect)?.Text.Trim();
            return string.Equals(expected, given.Trim(), StringComparison.OrdinalIgnoreCase)
                   || question.Options.Where(o => o.IsCorrect).Any(o => o.Id.ToString() == given);
        }
        if (question.ContentType == ContentType.MultipleChoice)
        {
            var expected = question.Options.Where(o => o.IsCorrect).Select(o => o.Text.Trim()).OrderBy(x => x).ToList();
            var actual = given.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .OrderBy(x => x).ToList();
            return expected.SequenceEqual(actual, StringComparer.OrdinalIgnoreCase);
        }
        return string.Equals(question.Answer?.Trim(), given.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static void EnsureWiSoIntegrity(IReadOnlyList<LearningContent> questions)
    {
        if (questions.Count != 30)
            throw new InvalidOperationException("WiSo-Simulation braucht genau 30 Fragen.");
        foreach (var q in questions)
        {
            if (q.VerificationStatus is VerificationStatus.Historical or VerificationStatus.Quarantined)
                throw new InvalidOperationException($"Veralteter oder quarantänisierter Inhalt '{q.Id}' darf nicht in die Prüfung.");
            if (q.ContentType == ContentType.SingleChoice && q.Options.Count(o => o.IsCorrect) != 1)
                throw new InvalidOperationException($"Single-Choice '{q.Id}' hat nicht genau eine richtige Option.");
            if (q.ContentType == ContentType.MultipleChoice)
            {
                var n = q.Options.Count(o => o.IsCorrect);
                if (n < 2) throw new InvalidOperationException($"Multiple-Choice '{q.Id}' deklariert keine eindeutige Mehrfachauswahl.");
            }
        }
    }

    private static string BuildReport(ExamSession session, int percent, int awarded, int possible, int expected, int actual)
        => $"Auswertung {session.Kind}: {awarded}/{possible} Punkte ({percent}%). " +
           $"Bearbeitungszeit {actual} min bei {expected} min Soll. " +
           "Dies ist kein Bestehensversprechen der IHK.";
}

public sealed class CoverageTrainerService : ICoverageTrainer
{
    public Ap2Master.Domain.Coverage.CoverageResult Evaluate(Ap2Master.Domain.Coverage.ControlFlowGraph graph, IEnumerable<string> selectedPathIds)
        => Ap2Master.Domain.Coverage.CoverageCalculator.Calculate(graph, selectedPathIds);

    public IReadOnlyList<string> MinimalSet(Ap2Master.Domain.Coverage.ControlFlowGraph graph, string mode)
        => Ap2Master.Domain.Coverage.CoverageCalculator.MinimalPathSet(graph, mode);
}

public sealed class NetworkPlanTrainerService : INetworkPlanTrainer
{
    public Ap2Master.Domain.ProjectManagement.NetworkPlanResult Calculate(IReadOnlyList<Ap2Master.Domain.ProjectManagement.NetworkActivity> activities)
        => Ap2Master.Domain.ProjectManagement.NetworkPlanCalculator.Calculate(activities);
}
