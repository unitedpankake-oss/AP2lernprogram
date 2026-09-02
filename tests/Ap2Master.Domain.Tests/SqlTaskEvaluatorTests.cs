using Ap2Master.Domain.Evaluation;
using FluentAssertions;

namespace Ap2Master.Domain.Tests;

public class SqlTaskEvaluatorTests
{
    private static readonly IReadOnlyList<SqlCriterion> Criteria =
    [
        new("leftjoin", "LEFT JOIN korrekt verwendet", 3, SqlCriterionKind.SqlPattern, @"\bLEFT\s+(OUTER\s+)?JOIN\b"),
        new("countalias", "COUNT und Alias korrekt", 3, SqlCriterionKind.SqlPattern, @"\bCOUNT\s*\([^)]*\)\s+AS\s+[A-Z_][A-Z0-9_]*"),
        new("group", "GROUP BY korrekt", 2, SqlCriterionKind.SqlPattern, @"\bGROUP\s+BY\b"),
        new("order", "Sortierung korrekt", 2, SqlCriterionKind.OrderMatches),
        new("result", "Ergebnismenge korrekt", 4, SqlCriterionKind.ResultMatches)
    ];

    private const string Solution =
        "SELECT m.Id, m.Name, COUNT(t.Id) AS TicketAnzahl FROM Mitarbeiter m " +
        "LEFT JOIN Ticket t ON t.MitarbeiterId = m.Id GROUP BY m.Id, m.Name ORDER BY m.Name;";

    [Fact]
    public void Full_solution_scores_every_point()
    {
        var grade = SqlTaskEvaluator.Grade(Solution, Criteria, resultMatches: true, orderMatches: true);

        grade.Max.Should().Be(14);
        grade.Awarded.Should().Be(14);
        grade.IsPerfect.Should().BeTrue();
        grade.Criteria.Should().OnlyContain(c => c.Passed);
    }

    [Fact]
    public void Inner_join_loses_only_the_join_criterion()
    {
        var learner = Solution.Replace("LEFT JOIN", "INNER JOIN");

        var grade = SqlTaskEvaluator.Grade(learner, Criteria, resultMatches: true, orderMatches: true);

        grade.Awarded.Should().Be(11);
        grade.Criteria.Single(c => c.Id == "leftjoin").Passed.Should().BeFalse();
        grade.Criteria.Single(c => c.Id == "group").Passed.Should().BeTrue();
    }

    [Fact]
    public void Missing_alias_loses_the_alias_criterion()
    {
        var learner = Solution.Replace("COUNT(t.Id) AS TicketAnzahl", "COUNT(t.Id)");

        var grade = SqlTaskEvaluator.Grade(learner, Criteria, resultMatches: true, orderMatches: true);

        grade.Criteria.Single(c => c.Id == "countalias").Passed.Should().BeFalse();
        grade.Awarded.Should().Be(11);
    }

    [Fact]
    public void Wrong_result_set_keeps_static_points_but_loses_result_points()
    {
        var grade = SqlTaskEvaluator.Grade(Solution, Criteria, resultMatches: false, orderMatches: false);

        grade.Awarded.Should().Be(8);
        grade.IsPerfect.Should().BeFalse();
        grade.Criteria.Single(c => c.Id == "result").Awarded.Should().Be(0);
        grade.Criteria.Single(c => c.Id == "order").Awarded.Should().Be(0);
    }

    [Fact]
    public void Empty_query_scores_nothing()
    {
        var grade = SqlTaskEvaluator.Grade("   ", Criteria, resultMatches: false, orderMatches: false);

        grade.Awarded.Should().Be(0);
        grade.Ratio.Should().Be(0);
    }

    [Fact]
    public void Comments_do_not_fake_a_criterion()
    {
        var learner = "-- LEFT JOIN\n/* GROUP BY */ SELECT 1;";

        var grade = SqlTaskEvaluator.Grade(learner, Criteria, resultMatches: false, orderMatches: false);

        grade.Awarded.Should().Be(0);
    }

    [Fact]
    public void Normalization_is_case_and_whitespace_insensitive()
    {
        SqlTaskEvaluator.NormalizeSql("select\n   *\tfrom  Kunde").Should().Be("SELECT * FROM KUNDE");
        SqlTaskEvaluator.MatchesPattern("select * from a left   join b on a.id=b.id", @"\bLEFT\s+JOIN\b").Should().BeTrue();
    }
}
