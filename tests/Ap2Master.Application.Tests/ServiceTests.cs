using Ap2Master.Application.Planning;
using Ap2Master.Content;
using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.ExamEngine;
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

