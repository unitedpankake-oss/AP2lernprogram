using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.Domain.Evaluation;
using Ap2Master.Domain.Validation;

namespace Ap2Master.Application.Validation;

public sealed class CatalogValidator
{
    public IReadOnlyList<ValidationIssue> Validate(
        IReadOnlyList<Topic> topics,
        IReadOnlyList<LearningContent> contents,
        IReadOnlyList<ExerciseTemplate> templates)
    {
        var issues = new List<ValidationIssue>();
        var topicIds = topics.Select(t => t.Id).ToHashSet(StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var topicSeen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var topic in topics)
        {
            if (!topicSeen.Add(topic.Id))
                issues.Add(new ValidationIssue("duplicate-topic", $"Doppelte Themen-Id '{topic.Id}'."));
            if (topic.ParentTopicId is { } p && !topicIds.Contains(p) && p.Length > 0)
                issues.Add(new ValidationIssue("invalid-parent", $"Elternknoten '{p}' fehlt für '{topic.Id}'."));
        }

        foreach (var content in contents)
            issues.AddRange(ContentRules.Validate(content, topicIds, seen));

        foreach (var group in contents.Where(c => c.ContentType is ContentType.SingleChoice or ContentType.MultipleChoice))
        {
            var texts = group.Options.Select(o => o.Text.Trim()).ToList();
            if (texts.Count != texts.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                issues.Add(new ValidationIssue("ambiguous-mc", "Mehrdeutige Antwortoptionen.", group.Id));
        }

        foreach (var t in templates)
        {
            if (!topicIds.Contains(t.TopicId))
                issues.Add(new ValidationIssue("invalid-topic", $"Vorlage '{t.Id}' verweist auf unbekanntes Thema.", t.Id));
            if (t.ParameterSchemaJson.Contains("\"min\"") && t.ParameterSchemaJson.Contains("\"max\""))
            {
                // range sanity is checked at generation time
            }
        }

        foreach (var content in contents.Where(c => c.ContentType == ContentType.Calculation && c.Formula() is null))
        {
            if (content.Title.Contains("Wirtschaftlichkeit", StringComparison.OrdinalIgnoreCase)
                && content.Answer is not null
                && !FormulaLibrary.IsKnownCorrectFormula("wirtschaftlichkeit", content.Answer)
                && !content.Answer.Contains('/'))
            {
                issues.Add(new ValidationIssue("incorrect-formula", "Wirtschaftlichkeitsformel weicht ab.", content.Id));
            }
        }

        return issues;
    }
}

public static class LearningContentExtensions
{
    public static string? Formula(this LearningContent content) =>
        content.PayloadJson.Contains("formula", StringComparison.OrdinalIgnoreCase) ? content.Answer : null;
}
