using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ap2Master.Domain;
using Ap2Master.Domain.Entities;

namespace Ap2Master.Content;

public sealed record TeachingFact(
    string TopicId,
    string Core,
    string Detail,
    string Example,
    string ExamTrap,
    string? Formula = null,
    string? LegalBasis = null,
    bool TimeSensitive = false,
    DateOnly? ValidFrom = null,
    DateOnly? ValidUntil = null,
    DateTime? VerifiedAt = null,
    VerificationStatus Status = VerificationStatus.Verified,
    string SourceDocument = "NormalizedSyllabus+AmtlicheQuellen",
    string SourceSection = "Lehrkanon AP2 FIAE",
    string? TechnicalStandardVersion = null);

public sealed class ContentCatalogFactory : Ap2Master.Application.Abstractions.IContentCatalog
{
    public IReadOnlyList<Topic> Topics { get; }
    public IReadOnlyList<LearningContent> Contents { get; }
    public IReadOnlyList<Flashcard> Flashcards { get; }
    public IReadOnlyList<ExerciseTemplate> Templates { get; }
    public IReadOnlyList<LegalFact> LegalFacts { get; }
    public IReadOnlyList<ContentConflict> Conflicts { get; }
    public IReadOnlyList<ExamDefinition> Exams { get; }
    public IReadOnlyList<SourceReference> Sources { get; }

    public ContentCatalogFactory()
    {
        Topics = TopicCatalog.ToEntities();
        var facts = TeachingFacts.All.ToDictionary(f => f.TopicId, StringComparer.Ordinal);
        var contents = new List<LearningContent>();
        var cards = new List<Flashcard>();
        var sources = new List<SourceReference>();
        var now = new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc);

        foreach (var topic in Topics)
        {
            if (!facts.TryGetValue(topic.Id, out var fact))
                fact = TeachingFacts.Synthesize(topic);

            var theory = ExpandTheory(topic, fact);
            contents.Add(MakeContent(topic, fact, ContentType.Theory, $"{topic.Title} – Theorie", theory, theory, now));
            contents.Add(MakeContent(topic, fact, ContentType.CheatSheet, $"{topic.Title} – Spickzettel", fact.Core, fact.Formula ?? fact.Core, now));

            var cardSpecs = new (string front, string back, string? formula)[]
            {
                ($"Was ist der Kern von „{topic.Title}“?", fact.Core, fact.Formula),
                ($"Welche typische Prüfungsfalle gibt es bei „{topic.Title}“?", fact.ExamTrap, null),
                ($"Nennen Sie ein prüfungsnahes Beispiel zu „{topic.Title}“.", fact.Example, null)
            };
            if (!string.IsNullOrWhiteSpace(fact.Formula))
                cardSpecs = cardSpecs.Append(($"Formel: {topic.Title}", fact.Formula!, fact.Formula)).ToArray();

            for (var i = 0; i < cardSpecs.Length; i++)
            {
                var id = $"fc-{topic.Id}-{i + 1}";
                var lc = MakeContent(topic, fact, ContentType.Flashcard, cardSpecs[i].front, cardSpecs[i].front, cardSpecs[i].back, now, id);
                contents.Add(lc);
                cards.Add(new Flashcard
                {
                    Id = id,
                    LearningContentId = id,
                    TopicId = topic.Id,
                    Front = cardSpecs[i].front,
                    Back = cardSpecs[i].back,
                    Formula = cardSpecs[i].formula,
                    DueAt = now
                });
            }

            contents.Add(BuildSingleChoice(topic, fact, now));
            sources.Add(new SourceReference
            {
                Id = Guid.NewGuid(),
                DocumentName = fact.SourceDocument,
                Section = fact.SourceSection,
                TopicId = topic.Id,
                ContentType = "Theory+Flashcard+SingleChoice",
                OverlapsOtherSource = fact.SourceDocument.Contains("Vorbereitungskurs") || fact.SourceDocument.Contains("Lernzettel"),
                TimeSensitive = fact.TimeSensitive,
                Verified = fact.Status is VerificationStatus.Verified or VerificationStatus.CorrectedFromSource,
                HasExercises = true,
                HasFlashcards = true,
                Notes = fact.LegalBasis ?? ""
            });
        }

        contents.AddRange(WiSoQuestionBank.Create(Topics, now));
        contents.AddRange(SpecialExercises.Create(Topics, now));
        Templates = ExerciseTemplateCatalog.All;
        LegalFacts = LegalFactCatalog.All;
        Conflicts = ConflictCatalog.All;
        Exams =
        [
            new ExamDefinition { Id = "exam-teil1", Title = "Probeklausur Teil 1 (90 Minuten)", Kind = ExamKind.Teil1, DurationMinutes = 90, QuestionCount = 12, TotalPoints = 100, HideHints = true },
            new ExamDefinition { Id = "exam-teil2", Title = "Probeklausur Teil 2 (90 Minuten)", Kind = ExamKind.Teil2, DurationMinutes = 90, QuestionCount = 12, TotalPoints = 100, HideHints = true },
            new ExamDefinition { Id = "exam-wiso", Title = "WiSo-Simulation (30 Fragen / 60 Minuten)", Kind = ExamKind.WiSo, DurationMinutes = 60, QuestionCount = 30, TotalPoints = 30, HideHints = true },
            new ExamDefinition { Id = "exam-day", Title = "Kompletter AP2-Trainingstag", Kind = ExamKind.TrainingDay, DurationMinutes = 240, QuestionCount = 54, TotalPoints = 230, HideHints = true },
            new ExamDefinition { Id = "exam-custom", Title = "Eigene Prüfung", Kind = ExamKind.Custom, DurationMinutes = 45, QuestionCount = 15, TotalPoints = 15, HideHints = true }
        ];
        Contents = contents;
        Flashcards = cards;
        Sources = sources;
    }

    private static string ExpandTheory(Topic topic, TeachingFact fact)
    {
        var legal = string.IsNullOrWhiteSpace(fact.LegalBasis) ? "" : $"\n\nRechtsgrundlage: {fact.LegalBasis}";
        var formula = string.IsNullOrWhiteSpace(fact.Formula) ? "" : $"\n\nFormel: {fact.Formula}";
        var validity = fact.TimeSensitive
            ? $"\n\nZeitbezug: gültig ab {fact.ValidFrom:dd.MM.yyyy}" + (fact.ValidUntil is { } u ? $" bis {u:dd.MM.yyyy}" : "") + $", verifiziert {fact.VerifiedAt:dd.MM.yyyy}."
            : "";
        return $"{fact.Core}\n\n{fact.Detail}\n\nBeispiel:\n{fact.Example}\n\nPrüfungsfalle:\n{fact.ExamTrap}{formula}{legal}{validity}\n\nDie Prüfungsreife hängt von eigenem Training ab; ein Bestehen wird nicht zugesichert.";
    }

    private static LearningContent MakeContent(Topic topic, TeachingFact fact, ContentType type, string title, string question, string answer, DateTime now, string? id = null)
        => new()
        {
            Id = id ?? $"{type.ToString().ToLowerInvariant()}-{topic.Id}",
            Title = title,
            TopicId = topic.Id,
            ParentTopicId = topic.ParentTopicId,
            ExamPart = topic.ExamPart,
            Difficulty = topic.IsPriority ? Difficulty.Pruefung : Difficulty.Grundlagen,
            ContentType = type,
            TagsCsv = topic.TagsCsv,
            EstimatedMinutes = type == ContentType.Theory ? topic.EstimatedMinutes : 3,
            Points = type == ContentType.Theory ? 0 : 1,
            Question = question,
            Answer = answer,
            Explanation = fact.Detail,
            HintsJson = JsonSerializer.Serialize(new[] { fact.ExamTrap }),
            PayloadJson = "{}",
            SourceDocument = fact.SourceDocument,
            SourceSection = fact.SourceSection,
            SourceType = "structured-import",
            ValidFrom = fact.ValidFrom,
            ValidUntil = fact.ValidUntil,
            VerifiedAt = fact.VerifiedAt ?? now,
            VerificationStatus = fact.Status,
            LegalBasis = fact.LegalBasis,
            TechnicalStandardVersion = fact.TechnicalStandardVersion,
            ContentVersion = "1.0.0"
        };

    private static LearningContent BuildSingleChoice(Topic topic, TeachingFact fact, DateTime now)
    {
        var id = $"sc-{topic.Id}";
        var content = MakeContent(topic, fact, ContentType.SingleChoice, $"{topic.Title} – Prüfungsfrage", $"Was trifft auf „{topic.Title}“ zu?", fact.Core, now, id);
        content.Points = 1;
        content.Options =
        [
            new AnswerOption { Id = Guid.Parse(ToGuid(id, 1)), LearningContentId = id, Text = fact.Core, IsCorrect = true, Explanation = fact.Detail, SortOrder = 0 },
            new AnswerOption { Id = Guid.Parse(ToGuid(id, 2)), LearningContentId = id, Text = fact.ExamTrap, IsCorrect = false, Explanation = "Das ist die typische Falle, nicht die geltende Regel.", SortOrder = 1 },
            new AnswerOption { Id = Guid.Parse(ToGuid(id, 3)), LearningContentId = id, Text = "Das Thema ist in der AP2 nicht relevant.", IsCorrect = false, Explanation = "Das Thema gehört zum Prüfungsbereich.", SortOrder = 2 },
            new AnswerOption { Id = Guid.Parse(ToGuid(id, 4)), LearningContentId = id, Text = "Es gibt keine nachvollziehbare Unterscheidung, daher ist jede Antwort gleichwertig.", IsCorrect = false, Explanation = "Es gibt eine fachlich klare Lösung.", SortOrder = 3 }
        ];
        return content;
    }

    private static string ToGuid(string id, int n)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(id + ":" + n));
        bytes[7] = (byte)((bytes[7] & 0x0F) | 0x40);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        return new Guid(bytes).ToString();
    }
}
