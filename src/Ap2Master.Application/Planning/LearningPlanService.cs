using Ap2Master.Domain;
using Ap2Master.Domain.Entities;
using Ap2Master.Domain.Learning;

namespace Ap2Master.Application.Planning;

public sealed class LearningPlanService : ILearningPlanPort
{
    private static readonly string[] PriorityTopics =
    [
        "t2-sql-select", "t2-sql-join", "t2-sql-group", "t2-algorithmen-sort", "t2-komplexitaet",
        "t1-uml-klasse", "t1-er", "t2-coverage", "t3-bbig", "t3-kuendigung", "t3-sv"
    ];

    public LearningPlan Build(UserSettings settings, IReadOnlyList<Topic> topics, DateTime utcNow)
    {
        var exam = settings.ExamDate?.Date ?? utcNow.Date.AddDays(90);
        var weekdays = settings.AvailableWeekdaysCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse).ToHashSet();
        var minutes = Math.Max(20, settings.MinutesPerDay);
        var difficult = settings.DifficultSubjectsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var leaves = topics.Where(t => topics.All(c => c.ParentTopicId != t.Id)).OrderBy(t => t.ExamPart).ThenBy(t => t.SortOrder).ToList();
        var prioritized = leaves
            .OrderByDescending(t => t.IsPriority || PriorityTopics.Contains(t.Id) || difficult.Contains(t.Id) || difficult.Contains(t.Title))
            .ThenBy(t => t.ExamPart)
            .ThenBy(t => t.SortOrder)
            .ToList();

        var plan = new LearningPlan
        {
            Id = Guid.NewGuid(),
            CreatedAt = utcNow,
            ExamDate = exam,
            MinutesPerDay = minutes,
            AvailableWeekdaysCsv = settings.AvailableWeekdaysCsv,
            IsActive = true
        };

        var date = DateOnly.FromDateTime(utcNow.Date);
        var examDate = DateOnly.FromDateTime(exam);
        var queue = new Queue<Topic>(prioritized);
        var cycle = 0;
        while (date < examDate)
        {
            var dow = (int)date.ToDateTime(TimeOnly.MinValue).DayOfWeek;
            if (dow == 0) dow = 7;
            if (!weekdays.Contains(dow))
            {
                date = date.AddDays(1);
                continue;
            }

            var remaining = minutes;
            var sort = 0;
            while (remaining > 0)
            {
                ActivityKind kind;
                string topicId;
                string title;
                int cost;

                if (cycle % 9 == 7)
                {
                    kind = ActivityKind.WiSoSimulation;
                    topicId = "t3";
                    title = "WiSo-Simulation (30 Fragen / 60 Minuten)";
                    cost = Math.Min(remaining, 60);
                }
                else if (cycle % 14 == 12)
                {
                    kind = ActivityKind.MockTeil1;
                    topicId = "t1";
                    title = "Probeklausur Teil 1";
                    cost = Math.Min(remaining, 90);
                }
                else if (cycle % 14 == 13)
                {
                    kind = ActivityKind.MockTeil2;
                    topicId = "t2";
                    title = "Probeklausur Teil 2";
                    cost = Math.Min(remaining, 90);
                }
                else if (cycle % 5 == 4)
                {
                    kind = ActivityKind.MixedRetrieval;
                    topicId = "mixed";
                    title = "Gemischte Abrufsession";
                    cost = Math.Min(remaining, 25);
                }
                else
                {
                    if (queue.Count == 0) queue = new Queue<Topic>(prioritized);
                    var topic = queue.Dequeue();
                    kind = cycle % 3 == 0 ? ActivityKind.Flashcards : cycle % 3 == 1 ? ActivityKind.Exercise : ActivityKind.Theory;
                    topicId = topic.Id;
                    title = $"{kind switch { ActivityKind.Flashcards => "Lernkarten", ActivityKind.Exercise => "Übungsaufgaben", _ => "Theorie" }}: {topic.Title}";
                    cost = Math.Min(remaining, Math.Max(15, topic.EstimatedMinutes / 2));
                }

                plan.Activities.Add(new PlannedActivity
                {
                    Id = Guid.NewGuid(),
                    LearningPlanId = plan.Id,
                    Date = date,
                    Kind = kind,
                    TopicId = topicId,
                    Title = title,
                    EstimatedMinutes = cost,
                    SortOrder = sort++
                });
                remaining -= cost;
                cycle++;
                if (sort > 6) break;
            }

            date = date.AddDays(1);
        }

        return plan;
    }

    public void RescheduleMissed(LearningPlan plan, DateOnly today)
    {
        var missed = plan.Activities.Where(a => !a.IsCompleted && a.Date < today).OrderBy(a => a.Date).ToList();
        var future = plan.Activities.Where(a => a.Date >= today).GroupBy(a => a.Date).ToDictionary(g => g.Key, g => g.Sum(x => x.EstimatedMinutes));
        foreach (var item in missed)
        {
            item.IsMissed = true;
            var target = today;
            while (future.GetValueOrDefault(target) + item.EstimatedMinutes > plan.MinutesPerDay * 1.4)
                target = target.AddDays(1);
            item.Date = target;
            future[target] = future.GetValueOrDefault(target) + item.EstimatedMinutes;
        }
    }
}

public interface ILearningPlanPort
{
    LearningPlan Build(UserSettings settings, IReadOnlyList<Topic> topics, DateTime utcNow);
    void RescheduleMissed(LearningPlan plan, DateOnly today);
}
