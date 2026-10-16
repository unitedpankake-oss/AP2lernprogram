using Ap2Master.Domain;
using Ap2Master.Domain.Coverage;
using Ap2Master.Domain.Evaluation;
using Ap2Master.Domain.Learning;
using Ap2Master.Domain.ProjectManagement;
using Ap2Master.Domain.Validation;
using FluentAssertions;

namespace Ap2Master.Domain.Tests;

public class Sm2SchedulerTests
{
    [Fact]
    public void Again_resets_repetitions_and_schedules_soon()
    {
        var now = new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);
        var r = Sm2Scheduler.Review(2.5, 4, 10, FlashcardRating.NochEinmal, now);
        r.Repetitions.Should().Be(0);
        r.DueAt.Should().Be(now.AddMinutes(10));
        r.Easiness.Should().BeGreaterThanOrEqualTo(1.3);
    }

    [Fact]
    public void Good_first_review_uses_one_day_interval()
    {
        var now = new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc);
        var r = Sm2Scheduler.Review(2.5, 0, 0, FlashcardRating.Gut, now);
        r.Repetitions.Should().Be(1);
        r.IntervalDays.Should().Be(1);
    }
}

public class ReadinessCalculatorTests
{
    [Fact]
    public void Weighted_score_is_transparent_and_capped()
    {
        var r = ReadinessCalculator.Calculate(1, 1, 1, 1, 1, 1, 1, 1, 1, 50);
        r.Score.Should().Be(100);
        r.Reasons.Should().Contain(x => x.Contains("verändert"));
        r.Reasons.Should().Contain(x => x.Contains("kein Bestehensversprechen"));
    }
}

public class CoverageCalculatorTests
{
    private static ControlFlowGraph Graph() => ControlFlowGraph.IfThenElse();

    [Fact]
    public void Then_path_alone_is_half_branch_coverage()
    {
        var r = CoverageCalculator.Calculate(Graph(), ["p-then"]);
        r.BranchCoverage.Should().BeApproximately(0.5, 0.01);
        r.PathCoverage.Should().BeApproximately(0.5, 0.01);
        r.StatementCoverage.Should().BeGreaterThan(0.5);
    }

    [Fact]
    public void Union_of_both_paths_is_full_coverage()
    {
        var r = CoverageCalculator.Calculate(Graph(), ["p-then", "p-else"]);
        r.BranchCoverage.Should().Be(1);
        r.PathCoverage.Should().Be(1);
        r.StatementCoverage.Should().Be(1);
    }

    [Fact]
    public void Minimal_branch_set_needs_both_paths()
    {
        CoverageCalculator.MinimalPathSet(Graph(), "branch").Should().HaveCount(2);
    }
}

public class NetworkPlanTests
{
    [Fact]
    public void Critical_path_and_buffers_match_manual_calculation()
    {
        var r = NetworkPlanCalculator.Calculate(
        [
            new("A", "A", 3, Array.Empty<string>()),
            new("B", "B", 4, ["A"]),
            new("C", "C", 2, ["A"]),
            new("D", "D", 5, ["B", "C"])
        ]);
        r.ProjectDuration.Should().Be(12); // 3+4+5
        r.Activities.Single(a => a.Id == "C").GesamtPuffer.Should().Be(2);
        r.CriticalPath.Should().BeEquivalentTo(["A", "B", "D"]);
    }
}

public class FormulaAndSqlTests
{
    [Fact]
    public void Canonical_formulas()
    {
        FormulaLibrary.Wirtschaftlichkeit(80, 50).Should().Be(1.6);
        FormulaLibrary.Rentabilitaet(15, 100).Should().Be(15);
        FormulaLibrary.Produktivitaet(200, 50).Should().Be(4);
        FormulaLibrary.IsKnownCorrectFormula("wirtschaftlichkeit", "Ertrag / Aufwand").Should().BeTrue();
    }

    [Fact]
    public void Sql_result_comparison_is_order_insensitive_by_default()
    {
        var a = new[] { new Dictionary<string, object?> { ["Name"] = "Ada", ["Id"] = 1 } };
        var b = new[] { new Dictionary<string, object?> { ["id"] = 1, ["name"] = "Ada" } };
        SqlResultComparer.AreEqual(a, b, false).Should().BeTrue();
    }

    [Fact]
    public void Sorting_evaluator_detects_unsorted()
    {
        SortingEvaluator.IsSorted(new[] { 1, 3, 2 }).Should().BeFalse();
        SortingEvaluator.IsSorted(new[] { 1, 2, 3 }).Should().BeTrue();
    }
}

public class LegalFactRuleTests
{
    [Fact]
    public void Expired_facts_are_detected()
    {
        var fact = new Entities.LegalFact
        {
            ValidFrom = new DateOnly(2025, 1, 1),
            ValidUntil = new DateOnly(2025, 12, 31),
            VerificationStatus = VerificationStatus.Verified
        };
        LegalFactRules.IsExpired(fact, new DateOnly(2026, 9, 2)).Should().BeTrue();
        LegalFactRules.IsCurrent(fact, new DateOnly(2025, 6, 1)).Should().BeTrue();
    }
}
