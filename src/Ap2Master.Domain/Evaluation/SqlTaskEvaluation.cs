using System.Text.RegularExpressions;

namespace Ap2Master.Domain.Evaluation;

public enum SqlCriterionKind
{
    /// <summary>Statische Prüfung: Das normalisierte SQL muss zum Muster passen.</summary>
    SqlPattern = 1,
    /// <summary>Die Ergebnismenge stimmt mit der Musterlösung überein (Reihenfolge egal).</summary>
    ResultMatches = 2,
    /// <summary>Die Ergebnismenge stimmt inklusive Reihenfolge.</summary>
    OrderMatches = 3
}

public sealed record SqlCriterion(
    string Id,
    string Label,
    int Points,
    SqlCriterionKind Kind,
    string Pattern = "");

public sealed record SqlCriterionResult(string Id, string Label, int Points, int Awarded, bool Passed);

public sealed record SqlGrade(int Awarded, int Max, IReadOnlyList<SqlCriterionResult> Criteria)
{
    public bool IsPerfect => Max > 0 && Awarded == Max;
    public double Ratio => Max == 0 ? 0 : (double)Awarded / Max;

    public static SqlGrade Empty { get; } = new(0, 0, []);
}

public sealed record SqlLabTask(
    string Id,
    int Number,
    string TopicId,
    string Title,
    string Description,
    IReadOnlyList<string> Requirements,
    IReadOnlyList<string> Hints,
    string StarterSql,
    string ExpectedSql,
    bool RequireOrder,
    IReadOnlyList<SqlCriterion> Criteria)
{
    public int MaxPoints => Criteria.Sum(c => c.Points);
}

/// <summary>
/// Bewertet eine Lernlösung anhand nachvollziehbarer Kriterien. Punkte werden berechnet,
/// nicht fest hinterlegt: Musterprüfungen laufen über das normalisierte SQL, die
/// Ergebniskriterien über den tatsächlichen Vergleich der Ergebnismengen.
/// </summary>
public static class SqlTaskEvaluator
{
    private static readonly Regex LineComment = new("--[^\n\r]*", RegexOptions.Compiled);
    private static readonly Regex BlockComment = new(@"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    public static string NormalizeSql(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql)) return "";
        var text = BlockComment.Replace(sql, " ");
        text = LineComment.Replace(text, " ");
        text = Whitespace.Replace(text, " ");
        return text.Trim().ToUpperInvariant();
    }

    public static bool MatchesPattern(string? sql, string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) return false;
        var normalized = NormalizeSql(sql);
        if (normalized.Length == 0) return false;
        return Regex.IsMatch(normalized, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    public static SqlGrade Grade(
        string? learnerSql,
        IReadOnlyList<SqlCriterion> criteria,
        bool resultMatches,
        bool orderMatches)
    {
        var results = new List<SqlCriterionResult>(criteria.Count);
        foreach (var c in criteria)
        {
            var passed = c.Kind switch
            {
                SqlCriterionKind.SqlPattern => MatchesPattern(learnerSql, c.Pattern),
                SqlCriterionKind.ResultMatches => resultMatches,
                SqlCriterionKind.OrderMatches => orderMatches,
                _ => false
            };
            results.Add(new SqlCriterionResult(c.Id, c.Label, c.Points, passed ? c.Points : 0, passed));
        }

        return new SqlGrade(results.Sum(r => r.Awarded), criteria.Sum(c => c.Points), results);
    }
}
