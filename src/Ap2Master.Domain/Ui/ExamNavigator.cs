namespace Ap2Master.Domain.Ui;

public enum ExamNavKind
{
    Unanswered = 0,
    Current = 1,
    Answered = 2,
    Marked = 3
}

/// <summary>
/// Zustand einer Navigatorzelle. Current schlägt alles; Markiert bleibt sichtbar, auch wenn schon geantwortet.
/// Farbe allein reicht nicht — die UI zeigt zusätzlich Häkchen, Lesezeichen oder leeren Rahmen.
/// </summary>
public static class ExamNavigator
{
    public static ExamNavKind Classify(bool isCurrent, bool isAnswered, bool isMarked)
    {
        if (isCurrent) return ExamNavKind.Current;
        if (isMarked) return ExamNavKind.Marked;
        if (isAnswered) return ExamNavKind.Answered;
        return ExamNavKind.Unanswered;
    }

    public static string Glyph(ExamNavKind kind) => kind switch
    {
        ExamNavKind.Answered => "\uE73E",
        ExamNavKind.Marked => "\uE735",
        ExamNavKind.Current => "\uECCA",
        _ => ""
    };
}
