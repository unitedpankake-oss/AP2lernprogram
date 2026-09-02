using Ap2Master.Domain.Entities;

namespace Ap2Master.Domain.Validation;

public sealed record ValidationIssue(string Code, string Message, string? ContentId = null);

public static class ContentRules
{
    public static IReadOnlyList<ValidationIssue> Validate(LearningContent content, ISet<string> topicIds, ISet<string> seenIds)
    {
        var issues = new List<ValidationIssue>();
        if (string.IsNullOrWhiteSpace(content.Id))
            issues.Add(new ValidationIssue("missing-id", "Inhaltsobjekt ohne Id.", content.Id));
        else if (!seenIds.Add(content.Id))
            issues.Add(new ValidationIssue("duplicate-id", $"Doppelte Id '{content.Id}'.", content.Id));

        if (string.IsNullOrWhiteSpace(content.TopicId) || !topicIds.Contains(content.TopicId))
            issues.Add(new ValidationIssue("invalid-topic", $"Ungültige TopicId '{content.TopicId}'.", content.Id));

        if (string.IsNullOrWhiteSpace(content.SourceDocument) || string.IsNullOrWhiteSpace(content.SourceSection))
            issues.Add(new ValidationIssue("missing-source", "Quellenangabe fehlt.", content.Id));

        if (content.ContentType is ContentType.SingleChoice or ContentType.MultipleChoice or ContentType.Flashcard
            or ContentType.Calculation or ContentType.FillInTheBlank)
        {
            if (string.IsNullOrWhiteSpace(content.Answer) && content.Options.All(o => !o.IsCorrect))
                issues.Add(new ValidationIssue("missing-answer", "Antwort fehlt.", content.Id));
        }

        if (content.ContentType == ContentType.SingleChoice)
        {
            var correct = content.Options.Count(o => o.IsCorrect);
            if (correct != 1)
                issues.Add(new ValidationIssue("mc-single", "Single-Choice braucht genau eine richtige Option.", content.Id));
        }

        if (content.ContentType == ContentType.MultipleChoice)
        {
            var correct = content.Options.Count(o => o.IsCorrect);
            if (correct < 2)
                issues.Add(new ValidationIssue("mc-multi", "Multiple-Choice muss die Anzahl richtiger Optionen deklarieren und mindestens zwei richtige enthalten.", content.Id));
        }

        if (content.VerificationStatus == VerificationStatus.Quarantined)
            issues.Add(new ValidationIssue("quarantined", content.QuarantineReason ?? "Inhalt ist quarantänisiert.", content.Id));

        if (content.ValidUntil is { } until && until < DateOnly.FromDateTime(DateTime.UtcNow)
            && content.VerificationStatus != VerificationStatus.Historical)
            issues.Add(new ValidationIssue("expired", "Zeitabhängiger Fakt ist abgelaufen.", content.Id));

        return issues;
    }
}

public static class LegalFactRules
{
    public static bool IsExpired(LegalFact fact, DateOnly today)
        => fact.ValidUntil is { } until && until < today && fact.VerificationStatus != VerificationStatus.Historical;

    public static bool IsCurrent(LegalFact fact, DateOnly today)
        => fact.ValidFrom <= today && (fact.ValidUntil is null || fact.ValidUntil >= today)
           && fact.VerificationStatus is VerificationStatus.Verified or VerificationStatus.CorrectedFromSource;
}
