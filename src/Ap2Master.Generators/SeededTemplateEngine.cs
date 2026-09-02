using System.Text.Json;
using System.Text.RegularExpressions;
using Ap2Master.Domain;
using Ap2Master.Domain.Entities;

namespace Ap2Master.Generators;

public sealed class SeededTemplateEngine : Ap2Master.Application.Abstractions.IExerciseGenerator
{
    private static readonly Regex Placeholder = new("\\{\\{(?<name>[a-zA-Z0-9_]+)(?::(?<format>[^}]+))?\\}\\}", RegexOptions.Compiled);

    public GeneratedExercise Generate(ExerciseTemplate template, int seed)
    {
        var rng = new Random(seed);
        var schema = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(template.ParameterSchemaJson)
                     ?? new Dictionary<string, JsonElement>();
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, spec) in schema)
            values[key] = Resolve(rng, spec);

        string Expand(string text) => Placeholder.Replace(text, m =>
        {
            var name = m.Groups["name"].Value;
            return values.TryGetValue(name, out var v) ? v : m.Value;
        });

        ApplyDerived(values);
        var task = Expand(template.TemplateText);
        var solution = Expand(template.SolutionTemplate);
        ValidateGenerated(template, values, task, solution);

        return new GeneratedExercise
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Seed = seed,
            TopicId = template.TopicId,
            Difficulty = template.Difficulty,
            Operator = template.Operator,
            EstimatedMinutes = template.EstimatedMinutes,
            Points = template.Points,
            TaskText = task,
            Solution = solution,
            Explanation = Expand(template.ExplanationTemplate),
            EvaluationStrategy = template.EvaluationStrategy,
            HintsJson = Expand(template.HintsJson),
            ParametersJson = JsonSerializer.Serialize(values),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static GeneratedExercise GenerateFrom(ExerciseTemplate template, int seed)
        => new SeededTemplateEngine().Generate(template, seed);

    private static string Resolve(Random rng, JsonElement spec)
    {
        if (spec.ValueKind == JsonValueKind.Object && spec.TryGetProperty("type", out var typeEl))
        {
            var type = typeEl.GetString();
            switch (type)
            {
                case "int":
                    return rng.Next(spec.GetProperty("min").GetInt32(), spec.GetProperty("max").GetInt32() + 1).ToString();
                case "amount":
                    {
                        var min = spec.GetProperty("min").GetInt32();
                        var max = spec.GetProperty("max").GetInt32();
                        if (min > max) throw new InvalidOperationException("Unmöglicher Zufallsbereich.");
                        return rng.Next(min / 100, max / 100 + 1) * 100 + ",00";
                    }
                case "pick":
                    {
                        var items = spec.GetProperty("items").EnumerateArray().Select(x => x.GetString()!).ToArray();
                        return items[rng.Next(items.Length)];
                    }
                case "name":
                    return Names[rng.Next(Names.Length)];
                case "company":
                    return Companies[rng.Next(Companies.Length)];
                case "date":
                    {
                        var start = new DateTime(2024, 1, 1);
                        return start.AddDays(rng.Next(0, 800)).ToString("dd.MM.yyyy");
                    }
                case "ipv4":
                    return $"192.168.{rng.Next(0, 16)}.{rng.Next(1, 254)}";
                case "array":
                    {
                        var n = rng.Next(spec.GetProperty("minLen").GetInt32(), spec.GetProperty("maxLen").GetInt32() + 1);
                        var vals = Enumerable.Range(0, n).Select(_ => rng.Next(0, 50)).ToArray();
                        return string.Join(", ", vals);
                    }
            }
        }
        return spec.ToString();
    }

    private static void ApplyDerived(Dictionary<string, string> values)
    {
        if (values.TryGetValue("ertrag", out var er) && values.TryGetValue("aufwand", out var au)
            && int.TryParse(er, out var ertrag) && int.TryParse(au, out var aufwand) && aufwand != 0)
            values["wirtschaftlichkeit"] = (ertrag / (double)aufwand).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        if (values.TryGetValue("gewinn", out var g) && values.TryGetValue("kapital", out var k)
            && int.TryParse(g, out var gewinn) && int.TryParse(k, out var kapital) && kapital != 0)
            values["rentabilitaet"] = (gewinn / (double)kapital * 100).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        if (values.TryGetValue("da", out var daS) && values.TryGetValue("db", out var dbS) && values.TryGetValue("dc", out var dcS)
            && int.TryParse(daS, out var da) && int.TryParse(dbS, out var db) && int.TryParse(dcS, out var dc))
            values["dauer"] = (da + Math.Max(db, dc)).ToString();
        if (values.TryGetValue("betrag", out var b) && int.TryParse(b, out var betrag))
            values["gericht"] = betrag <= 10000 ? "Amtsgericht" : "Landgericht";
    }

    private static void ValidateGenerated(ExerciseTemplate template, Dictionary<string, string> values, string task, string solution)
    {
        if (string.IsNullOrWhiteSpace(task) || string.IsNullOrWhiteSpace(solution))
            throw new InvalidOperationException($"Generierte Aufgabe '{template.Id}' ist unvollständig.");
        if (task.Contains("{{") || solution.Contains("{{"))
            throw new InvalidOperationException($"Unaufgelöste Platzhalter in '{template.Id}'.");
        if (values.TryGetValue("aufwand", out var aufwand) && values.TryGetValue("ertrag", out var ertrag))
        {
            if (ParseDe(aufwand) <= 0 || ParseDe(ertrag) <= 0)
                throw new InvalidOperationException("Unmöglicher Zufallsbereich für Wirtschaftlichkeit.");
        }
    }

    private static double ParseDe(string v) => double.Parse(v.Replace(".", "").Replace(",", "."));

    private static readonly string[] Names = ["Aylin Kaya", "Jonas Weber", "Lea Hoffmann", "Omar Haddad", "Mia Schäfer", "Noah Krüger", "Elena Rossi", "Lukas Brandt"];
    private static readonly string[] Companies = ["Helpline GmbH", "Nordsoft AG", "Bytewerk OHG", "Klarblick UG", "HafenCode GmbH", "Aurora Systems KG"];
}
