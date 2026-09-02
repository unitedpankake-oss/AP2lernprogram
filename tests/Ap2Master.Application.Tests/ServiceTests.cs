using Ap2Master.Application.Planning;
using Ap2Master.Content;
using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.ExamEngine;
using Ap2Master.Generators;
using FluentAssertions;

namespace Ap2Master.Application.Tests;

public class LearningPlanTests
{
    [Fact]
    public void Plan_covers_days_until_exam_and_includes_simulations()
    {
        var settings = new UserSettings
        {
            ExamDate = new DateTime(2026, 12, 1),
            MinutesPerDay = 60,
            AvailableWeekdaysCsv = "1,2,3,4,5",
            DifficultSubjectsCsv = "SQL"
        };
        var topics = TopicCatalog.ToEntities();
        var plan = new LearningPlanService().Build(settings, topics, new DateTime(2026, 9, 2));
        plan.Activities.Should().NotBeEmpty();
        plan.Activities.Should().Contain(a => a.Kind == ActivityKind.WiSoSimulation);
        plan.Activities.Should().Contain(a => a.Kind == ActivityKind.MockTeil1);
        plan.Activities.Select(a => a.Date).Max().DayNumber.Should().BeLessThanOrEqualTo(new DateOnly(2026, 12, 1).DayNumber);
    }

    [Fact]
    public void Missed_work_is_moved_forward_not_dropped()
    {
        var plan = new LearningPlan
        {
            MinutesPerDay = 60,
            Activities =
            [
                new PlannedActivity { Id = Guid.NewGuid(), Date = new DateOnly(2026, 8, 1), EstimatedMinutes = 30, Title = "alt" },
                new PlannedActivity { Id = Guid.NewGuid(), Date = new DateOnly(2026, 9, 10), EstimatedMinutes = 20, Title = "neu" }
            ]
        };
        new LearningPlanService().RescheduleMissed(plan, new DateOnly(2026, 9, 2));
        plan.Activities.First(a => a.Title == "alt").Date.DayNumber.Should().BeGreaterThanOrEqualTo(new DateOnly(2026, 9, 2).DayNumber);
        plan.Activities.First(a => a.Title == "alt").IsMissed.Should().BeTrue();
    }
}

public class ExamScoringTests
{
    [Fact]
    public void Single_choice_scores_only_the_correct_option()
    {
        var q = new LearningContent
        {
            Id = "q1", TopicId = "t", Points = 2, ContentType = ContentType.SingleChoice, Answer = "A",
            Options = { new AnswerOption { Text = "A", IsCorrect = true }, new AnswerOption { Text = "B", IsCorrect = false } }
        };
        ExamScoringService.IsCorrect(q, "A").Should().BeTrue();
        ExamScoringService.IsCorrect(q, "B").Should().BeFalse();
        var session = new ExamSession { Kind = ExamKind.WiSo, StartedAt = DateTime.UtcNow.AddMinutes(-20), SubmittedAt = DateTime.UtcNow, AnswersJson = """{"q1":"A"}""" };
        var result = new ExamScoringService().Score(session, [q], DateTime.UtcNow);
        result.ScorePercent.Should().Be(100);
        result.ActualMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public void WiSo_integrity_rejects_wrong_count()
    {
        var act = () => ExamScoringService.EnsureWiSoIntegrity(Array.Empty<LearningContent>());
        act.Should().Throw<InvalidOperationException>();
    }
}

public class GeneratorTests
{
    [Fact]
    public void Same_seed_reproduces_same_task()
    {
        var tpl = ExerciseTemplateCatalog.All.First(t => t.Id == "tpl-wirtschaftlichkeit");
        var a = SeededTemplateEngine.GenerateFrom(tpl, 12345);
        var b = SeededTemplateEngine.GenerateFrom(tpl, 12345);
        a.TaskText.Should().Be(b.TaskText);
        a.Solution.Should().Be(b.Solution);
        a.ParametersJson.Should().Be(b.ParametersJson);
    }

    [Fact]
    public void Different_seeds_change_parameters()
    {
        var tpl = ExerciseTemplateCatalog.All.First(t => t.Id == "tpl-wirtschaftlichkeit");
        SeededTemplateEngine.GenerateFrom(tpl, 1).TaskText.Should().NotBe(SeededTemplateEngine.GenerateFrom(tpl, 2).TaskText);
    }
}

public class PseudocodeTests
{
    [Fact]
    public void Maximum_algorithm_returns_expected_value()
    {
        var src = """
            SET max = liste[0]
            FOR i FROM 1 TO n-1
            IF liste[i] > max THEN
            SET max = liste[i]
            ENDIF
            ENDFOR
            RETURN max
            """;
        var engine = new PseudocodeEngine();
        var r = engine.Run(src, new Dictionary<string, object?> { ["liste"] = new List<int> { 3, 9, 2 }, ["n"] = 3 });
        r.Success.Should().BeTrue();
        Convert.ToInt32(r.ReturnValue).Should().Be(9);
    }
}
