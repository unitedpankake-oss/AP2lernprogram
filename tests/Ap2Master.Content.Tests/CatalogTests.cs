using Ap2Master.Content;
using Ap2Master.Domain;
using Ap2Master.Domain.Validation;
using FluentAssertions;

namespace Ap2Master.Content.Tests;

public class CatalogTests
{
    [Fact]
    public void Every_topic_has_a_non_shallow_teaching_fact()
    {
        foreach (var topic in TopicCatalog.All)
        {
            var fact = TeachingFacts.For(topic);
            fact.Core.Length.Should().BeGreaterThan(40, topic.Id);
            fact.ExamTrap.Length.Should().BeGreaterThan(20, topic.Id);
            fact.Example.Should().NotBeNullOrWhiteSpace();
            fact.Core.Should().NotBe(topic.Title);
        }
    }

    [Fact]
    public void Topic_ids_are_unique_and_parents_resolve()
    {
        var ids = TopicCatalog.All.Select(t => t.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
        var set = ids.ToHashSet();
        foreach (var t in TopicCatalog.All.Where(t => t.ParentId is not null))
            set.Should().Contain(t.ParentId);
    }

    [Fact]
    public void Catalog_has_flashcards_and_exercises_for_every_topic()
    {
        var cat = new ContentCatalogFactory();
        foreach (var topic in cat.Topics)
        {
            cat.Flashcards.Any(f => f.TopicId == topic.Id).Should().BeTrue($"Karten fehlen für {topic.Id}");
            cat.Contents.Any(c => c.TopicId == topic.Id && c.ContentType == ContentType.Theory).Should().BeTrue();
            cat.Contents.Any(c => c.TopicId == topic.Id && c.ContentType == ContentType.SingleChoice).Should().BeTrue();
        }
        cat.Contents.Select(c => c.Id).Should().OnlyHaveUniqueItems();
        cat.LegalFacts.Should().Contain(f => f.Id == "lf-textform");
        cat.Conflicts.Should().NotBeEmpty();
    }

    [Fact]
    public void Corrected_legal_facts_are_present()
    {
        var facts = TeachingFacts.All.ToDictionary(f => f.TopicId);
        facts["t3-textform"].Core.Should().Contain("Textform");
        facts["t3-mutterschutz"].Core.Should().Contain("8 Wochen");
        facts["t3-mutterschutz"].ExamTrap.Should().Contain("14");
        facts["t1-wirtschaftlichkeit"].Formula.Should().Contain("Ertrag");
        facts["t3-amtsgericht"].Core.Should().Contain("10.000");
        facts["t3-kartellverbot"].ExamTrap.Should().Contain("Normen-");
        facts["t2-tls"].Core.Should().Contain("DHE");
        facts["t3-mindestlohn"].Core.Should().Contain("13,90");
    }

    [Fact]
    public void WiSo_bank_has_at_least_30_current_questions()
    {
        var qs = WiSoQuestionBank.Create(TopicCatalog.ToEntities(), new DateTime(2026, 9, 2));
        qs.Count(q => q.ExamPart == ExamPart.WiSo).Should().BeGreaterThanOrEqualTo(30);
        foreach (var q in qs.Where(q => q.ContentType == ContentType.SingleChoice))
            q.Options.Count(o => o.IsCorrect).Should().Be(1, q.Id);
    }

    [Fact]
    public void Source_inventory_records_expected_docx_paths()
    {
        SourceDocumentInventory.ExpectedPaths.Should().HaveCount(2);
        SourceDocumentInventory.Vorbereitungskurs.Should().Contain("Vorbereitungskurs");
        SourceDocumentInventory.Lernzettel.Should().Contain("Lernzettel");
    }

    [Fact]
    public void Validator_flags_duplicate_ids()
    {
        var topics = TopicCatalog.ToEntities();
        var c1 = new Domain.Entities.LearningContent { Id = "x", TopicId = topics[0].Id, SourceDocument = "a", SourceSection = "b", ContentType = ContentType.Flashcard, Answer = "1" };
        var c2 = new Domain.Entities.LearningContent { Id = "x", TopicId = topics[0].Id, SourceDocument = "a", SourceSection = "b", ContentType = ContentType.Flashcard, Answer = "1" };
        var issues = new Application.Validation.CatalogValidator().Validate(topics, [c1, c2], []);
        issues.Should().Contain(i => i.Code == "duplicate-id");
    }
}
