using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.Domain.Learning;
using FluentAssertions;

namespace Ap2Master.Domain.Tests;

public class TopicBoardBuilderTests
{
    private static Topic T(string id, string title, ExamPart part, string? parent, int sort)
        => new() { Id = id, Title = title, ExamPart = part, ParentTopicId = parent, SortOrder = sort };

    private static IReadOnlyList<Topic> Sample() =>
    [
        T("t1", "Teil 1", ExamPart.Teil1, null, 0),
        T("t1-uml", "UML", ExamPart.Teil1, "t1", 1),
        T("t1-uml-klasse", "Klassendiagramm", ExamPart.Teil1, "t1-uml", 2),
        T("t1-uml-usecase", "Use Case", ExamPart.Teil1, "t1-uml", 3),
        T("t1-sql", "SQL", ExamPart.Teil1, "t1", 4),
        T("t2", "Teil 2", ExamPart.Teil2, null, 10),
        T("t2-oop", "OOP", ExamPart.Teil2, "t2", 11),
        T("t3", "WiSo", ExamPart.WiSo, null, 20),
        T("t3-agb", "BGB", ExamPart.WiSo, "t3", 21)
    ];

    [Fact]
    public void Groups_topics_by_exam_part_then_cluster()
    {
        var boards = TopicBoardBuilder.Build(Sample());

        boards.Should().HaveCount(3);
        boards[0].Part.Should().Be(ExamPart.Teil1);
        boards[0].Clusters.Select(c => c.Id).Should().Equal("t1-uml", "t1-sql");
        boards[0].Clusters[0].Decks.Select(d => d.Id).Should().Equal("t1-uml-klasse", "t1-uml-usecase");
        boards[1].Clusters.Should().ContainSingle(c => c.Id == "t2-oop");
        boards[2].Clusters.Should().ContainSingle(c => c.Id == "t3-agb");
    }

    [Fact]
    public void Roots_are_not_shown_as_decks()
    {
        var decks = TopicBoardBuilder.Build(Sample()).SelectMany(b => b.Clusters).SelectMany(c => c.Decks);

        decks.Select(d => d.Id).Should().NotContain(["t1", "t2", "t3"]);
    }

    [Fact]
    public void Card_stats_roll_up_to_cluster_and_part()
    {
        var stats = new Dictionary<string, TopicCardStats>
        {
            ["t1-uml-klasse"] = new(10, 4),
            ["t1-uml-usecase"] = new(5, 1),
            ["t1-sql"] = new(2, 2)
        };

        var boards = TopicBoardBuilder.Build(Sample(), stats);
        var teil1 = boards[0];
        var uml = teil1.Clusters.Single(c => c.Id == "t1-uml");

        uml.TotalCards.Should().Be(15);
        uml.DueCards.Should().Be(5);
        teil1.TotalCards.Should().Be(17);
        teil1.DueCards.Should().Be(7);
    }

    [Fact]
    public void Cluster_without_children_becomes_its_own_deck()
    {
        var sql = TopicBoardBuilder.Build(Sample())[0].Clusters.Single(c => c.Id == "t1-sql");
        sql.Decks.Should().ContainSingle(d => d.Id == "t1-sql");
    }

    [Fact]
    public void Parts_use_distinct_family_colors_and_topics_differ_inside_a_part()
    {
        TopicPalette.PartAccent(ExamPart.Teil1).Should().NotBe(TopicPalette.PartAccent(ExamPart.Teil2));
        TopicPalette.PartAccent(ExamPart.Teil2).Should().NotBe(TopicPalette.PartAccent(ExamPart.WiSo));

        var a = TopicPalette.TopicAccent(ExamPart.Teil1, "t1-uml-klasse");
        var b = TopicPalette.TopicAccent(ExamPart.Teil1, "t1-sql");
        a.Should().NotBe(b);
        TopicPalette.TopicAccent(ExamPart.Teil1, "t1-uml-klasse").Should().Be(a);
    }
}
