namespace Ap2Master.Domain.Learning;

public sealed record Sm2Result(
    double Easiness,
    int Repetitions,
    int IntervalDays,
    DateTime DueAt);

public static class Sm2Scheduler
{
    public static Sm2Result Review(
        double easiness,
        int repetitions,
        int intervalDays,
        FlashcardRating rating,
        DateTime utcNow)
    {
        var q = (int)rating;
        var ef = easiness;
        if (q < 3)
        {
            return new Sm2Result(
                Easiness: Math.Max(1.3, ef - 0.20),
                Repetitions: 0,
                IntervalDays: 0,
                DueAt: utcNow.AddMinutes(10));
        }

        ef = Math.Max(1.3, ef + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02)));
        var nextReps = repetitions + 1;
        var nextInterval = nextReps switch
        {
            1 => 1,
            2 => 6,
            _ => Math.Max(1, (int)Math.Round(intervalDays * ef))
        };

        if (rating == FlashcardRating.Schwer)
        {
            nextInterval = Math.Max(1, (int)Math.Round(Math.Max(intervalDays, 1) * 1.2));
        }
        else if (rating == FlashcardRating.Einfach)
        {
            nextInterval = Math.Max(nextInterval, (int)Math.Round(nextInterval * 1.3));
        }

        return new Sm2Result(ef, nextReps, nextInterval, utcNow.Date.AddDays(nextInterval));
    }
}
