using System.Globalization;

namespace Ap2Master.Domain.Evaluation;

public static class SqlResultComparer
{
    public static bool AreEqual(IReadOnlyList<IReadOnlyDictionary<string, object?>> expected,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> actual,
        bool requireOrder)
    {
        if (expected.Count != actual.Count) return false;
        if (expected.Count == 0) return true;

        var expectedCols = expected[0].Keys.Select(Normalize).OrderBy(x => x).ToList();
        var actualCols = actual[0].Keys.Select(Normalize).OrderBy(x => x).ToList();
        if (!expectedCols.SequenceEqual(actualCols)) return false;

        if (requireOrder)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (NormalizeRow(expected[i]) != NormalizeRow(actual[i])) return false;
            }
            return true;
        }

        var expectedBag = expected.Select(NormalizeRow).OrderBy(x => x).ToList();
        var actualBag = actual.Select(NormalizeRow).OrderBy(x => x).ToList();
        return expectedBag.SequenceEqual(actualBag);
    }

    public static string NormalizeRow(IReadOnlyDictionary<string, object?> row)
    {
        var parts = row
            .OrderBy(kv => Normalize(kv.Key), StringComparer.Ordinal)
            .Select(kv => $"{Normalize(kv.Key)}={NormalizeValue(kv.Value)}");
        return string.Join("|", parts);
    }

    public static string Normalize(string name) => name.Trim().ToUpperInvariant();

    public static string NormalizeValue(object? value)
    {
        if (value is null or DBNull) return "NULL";
        if (value is bool b) return b ? "1" : "0";
        if (value is IFormattable f && value is not string)
            return f.ToString(null, CultureInfo.InvariantCulture) ?? "NULL";
        return Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() ?? "NULL";
    }
}

public static class SortingEvaluator
{
    public static bool IsSorted<T>(IReadOnlyList<T> values, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        for (var i = 1; i < values.Count; i++)
        {
            if (comparer.Compare(values[i - 1], values[i]) > 0) return false;
        }
        return true;
    }

    public static bool IsStableSort<TKey, TItem>(
        IReadOnlyList<TItem> original,
        IReadOnlyList<TItem> sorted,
        Func<TItem, TKey> key,
        IEqualityComparer<TItem>? itemComparer = null)
        where TKey : notnull
    {
        itemComparer ??= EqualityComparer<TItem>.Default;
        var groups = original
            .Select((item, index) => (item, index, key: key(item)!))
            .GroupBy(x => x.key)
            .ToDictionary(g => g.Key, g => g.Select(x => x.item).ToList());

        TKey? current = default;
        var seenNewGroup = true;
        var positions = new Dictionary<TKey, int>();
        foreach (var item in sorted)
        {
            var k = key(item)!;
            if (!positions.TryGetValue(k, out var pos)) pos = 0;
            if (pos >= groups[k].Count || !itemComparer.Equals(groups[k][pos], item))
                return false;
            positions[k] = pos + 1;
            if (!seenNewGroup && current is not null && !Equals(current, k) && positions.ContainsKey(k) && positions[k] > 1)
            {
                // stability is already checked via original relative order in groups
            }
            current = k;
            seenNewGroup = false;
        }

        return true;
    }
}

public static class FormulaLibrary
{
    public static double Wirtschaftlichkeit(double ertrag, double aufwand) => aufwand == 0 ? double.NaN : ertrag / aufwand;
    public static double Rentabilitaet(double gewinn, double kapital) => kapital == 0 ? double.NaN : gewinn / kapital * 100.0;
    public static double Produktivitaet(double ausbringung, double einsatz) => einsatz == 0 ? double.NaN : ausbringung / einsatz;
    public static double Roi(double gewinn, double einsatz) => einsatz == 0 ? double.NaN : gewinn / einsatz * 100.0;
    public static double Amortisation(double anschaffung, double rueckfluss) => rueckfluss == 0 ? double.NaN : anschaffung / rueckfluss;
    public static double Kapitalwert(IReadOnlyList<double> cashflows, double rate)
    {
        double npv = 0;
        for (var t = 0; t < cashflows.Count; t++)
            npv += cashflows[t] / Math.Pow(1 + rate, t);
        return npv;
    }

    public static bool IsKnownCorrectFormula(string name, string formula)
    {
        var n = NormalizeFormula(formula);
        return name.ToLowerInvariant() switch
        {
            "wirtschaftlichkeit" => n is "ertrag/aufwand" or "ertrag : aufwand",
            "rentabilität" or "rentabilitaet" => n.Contains("gewinn") && n.Contains("kapital") && n.Contains("100"),
            "produktivität" or "produktivitaet" => n is "ausbringungsmenge/einsatzmenge" or "ausbringung/einsatz",
            _ => false
        };
    }

    public static string NormalizeFormula(string formula)
        => formula.ToLowerInvariant()
            .Replace(" ", "")
            .Replace("×", "*")
            .Replace(":", "/")
            .Replace(",", ".");
}
