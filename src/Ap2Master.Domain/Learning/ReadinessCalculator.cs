namespace Ap2Master.Domain.Learning;

public sealed record ReadinessBreakdown(
    int Score,
    double TopicCoverage,
    double Recency,
    double FlashcardRecall,
    double ExerciseAccuracy,
    double DifficultyDistribution,
    double MockExamScores,
    double TimeManagement,
    double WeakSubjectImprovement,
    double RequiredTopicCompletion,
    IReadOnlyList<string> Reasons);

public static class ReadinessCalculator
{
    public static ReadinessBreakdown Calculate(
        double topicCoverage,
        double recency,
        double flashcardRecall,
        double exerciseAccuracy,
        double difficultyDistribution,
        double mockExamScores,
        double timeManagement,
        double weakSubjectImprovement,
        double requiredTopicCompletion,
        int? previousScore = null)
    {
        static double Clamp(double v) => Math.Clamp(v, 0, 1);

        topicCoverage = Clamp(topicCoverage);
        recency = Clamp(recency);
        flashcardRecall = Clamp(flashcardRecall);
        exerciseAccuracy = Clamp(exerciseAccuracy);
        difficultyDistribution = Clamp(difficultyDistribution);
        mockExamScores = Clamp(mockExamScores);
        timeManagement = Clamp(timeManagement);
        weakSubjectImprovement = Clamp(weakSubjectImprovement);
        requiredTopicCompletion = Clamp(requiredTopicCompletion);

        var weighted =
            topicCoverage * 0.16 +
            recency * 0.10 +
            flashcardRecall * 0.12 +
            exerciseAccuracy * 0.16 +
            difficultyDistribution * 0.08 +
            mockExamScores * 0.16 +
            timeManagement * 0.06 +
            weakSubjectImprovement * 0.08 +
            requiredTopicCompletion * 0.08;

        var score = (int)Math.Round(weighted * 100);
        var reasons = new List<string>
        {
            $"Themenabdeckung {Pct(topicCoverage)} (Gewicht 16%)",
            $"Aktualität der Übung {Pct(recency)} (Gewicht 10%)",
            $"Lernkarten-Abruf {Pct(flashcardRecall)} (Gewicht 12%)",
            $"Übungsgenauigkeit {Pct(exerciseAccuracy)} (Gewicht 16%)",
            $"Schwierigkeitsverteilung {Pct(difficultyDistribution)} (Gewicht 8%)",
            $"Probeklausuren {Pct(mockExamScores)} (Gewicht 16%)",
            $"Zeitmanagement {Pct(timeManagement)} (Gewicht 6%)",
            $"Verbesserung der Schwachstellen {Pct(weakSubjectImprovement)} (Gewicht 8%)",
            $"Pflichtthemen abgeschlossen {Pct(requiredTopicCompletion)} (Gewicht 8%)"
        };

        if (previousScore is int prev)
        {
            var delta = score - prev;
            reasons.Insert(0, delta == 0
                ? "Die Prüfungsreife ist gegenüber der letzten Berechnung unverändert."
                : $"Die Prüfungsreife hat sich um {delta:+0;-0} Punkte verändert.");
        }

        reasons.Add("Die Prüfungsreife ist kein Bestehensversprechen, sondern ein transparenter Trainingsindikator.");
        return new ReadinessBreakdown(
            score, topicCoverage, recency, flashcardRecall, exerciseAccuracy,
            difficultyDistribution, mockExamScores, timeManagement,
            weakSubjectImprovement, requiredTopicCompletion, reasons);
    }

    private static string Pct(double v) => $"{v * 100:0}%";
}
