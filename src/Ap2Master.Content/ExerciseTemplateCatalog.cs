using Ap2Master.Domain;
using Ap2Master.Domain.Entities;

namespace Ap2Master.Content;

public static class ExerciseTemplateCatalog
{
    public static IReadOnlyList<ExerciseTemplate> All { get; } =
    [
        Tpl("tpl-wirtschaftlichkeit", "t1-wirtschaftlichkeit", Difficulty.Pruefung, ContentType.Calculation, "Berechnen", 8, 5,
            "{{firma}} weist im Quartal einen Ertrag von {{ertrag}} € und einen Aufwand von {{aufwand}} € aus. Berechnen Sie die Wirtschaftlichkeit auf zwei Nachkommastellen.",
            "{{wirtschaftlichkeit}}",
            "Wirtschaftlichkeit = Ertrag / Aufwand. Nicht mit Rentabilität verwechseln.",
            "numeric",
            """{"firma":{"type":"company"},"ertrag":{"type":"int","min":40000,"max":120000},"aufwand":{"type":"int","min":20000,"max":39000},"wirtschaftlichkeit":{"type":"pick","items":["siehe Generator-Nachberechnung"]}}"""),
        Tpl("tpl-rentabilitaet", "t1-rentabilitaet", Difficulty.Pruefung, ContentType.Calculation, "Berechnen", 8, 5,
            "{{name}} setzt {{kapital}} € ein und erzielt {{gewinn}} € Gewinn. Berechnen Sie die Rentabilität in Prozent.",
            "{{rentabilitaet}} %",
            "Rentabilität = Gewinn / eingesetztes Kapital × 100.",
            "numeric",
            """{"name":{"type":"name"},"kapital":{"type":"int","min":50000,"max":200000},"gewinn":{"type":"int","min":5000,"max":40000},"rentabilitaet":{"type":"pick","items":["siehe Generator-Nachberechnung"]}}"""),
        Tpl("tpl-netzplan", "t1-faz-fez-saz-sez", Difficulty.Anspruchsvoll, ContentType.Calculation, "Berechnen", 15, 10,
            "Vorgang A dauert {{da}} Tage, B {{db}} Tage (Nachfolger von A), C {{dc}} Tage (Nachfolger von A, parallel zu B). Bestimmen Sie die Projektdauer und den kritischen Pfad.",
            "Projektdauer {{dauer}} Tage; kritisch ist der längere Pfad über A.",
            "Vorwärts- und Rückwärtsrechnung. Kritisch: Gesamtpuffer 0.",
            "network-plan",
            """{"da":{"type":"int","min":2,"max":6},"db":{"type":"int","min":3,"max":8},"dc":{"type":"int","min":2,"max":7},"dauer":{"type":"pick","items":["max"]}}"""),
        Tpl("tpl-sql-join", "t2-sql-join", Difficulty.Pruefung, ContentType.Sql, "Ermitteln", 12, 8,
            "Ermitteln Sie Name und Titel aller Tickets des Kunden mit Id {{kid}} per INNER JOIN.",
            "SELECT k.Name, t.Titel FROM Ticket t INNER JOIN Kunde k ON t.KundenId = k.Id WHERE k.Id = {{kid}};",
            "Join-Bedingung in ON, Filter in WHERE. Bewertung über Ergebnismenge.",
            "sql-result",
            """{"kid":{"type":"int","min":1,"max":5}}"""),
        Tpl("tpl-sql-group", "t2-sql-group", Difficulty.Pruefung, ContentType.Sql, "Ermitteln", 12, 8,
            "Geben Sie je Status die Anzahl der Tickets aus, nur Gruppen mit mindestens {{minn}} Tickets.",
            "SELECT Status, COUNT(*) AS Anzahl FROM Ticket GROUP BY Status HAVING COUNT(*) >= {{minn}};",
            "HAVING nach Aggregation.",
            "sql-result",
            """{"minn":{"type":"int","min":1,"max":3}}"""),
        Tpl("tpl-minmax", "t2-minmax", Difficulty.Grundlagen, ContentType.Pseudocode, "Entwickeln", 12, 8,
            "Gegeben ist die Liste [{{array}}]. Schreiben Sie IHK-Pseudocode, der das Maximum bestimmt und zurückgibt.",
            "SET max = liste[0]\nFOR i FROM 1 TO n-1\n  IF liste[i] > max THEN\n    SET max = liste[i]\n  ENDIF\nENDFOR\nRETURN max",
            "Mit erstem Element initialisieren, nicht mit 0.",
            "pseudocode-tests",
            """{"array":{"type":"array","minLen":4,"maxLen":8}}"""),
        Tpl("tpl-http", "t1-crud", Difficulty.Grundlagen, ContentType.Matching, "Zuordnen", 8, 4,
            "Ordnen Sie der Ressource {{resource}} die HTTP-Methode für das Erzeugen eines neuen Datensatzes zu.",
            "POST",
            "Create → POST auf die Collection.",
            "exact",
            """{"resource":{"type":"pick","items":[" /tickets"," /kunden"," /geraete"]}}"""),
        Tpl("tpl-ag", "t3-amtsgericht", Difficulty.Pruefung, ContentType.Scenario, "Entscheiden", 6, 3,
            "{{name}} klagt 2026 aus einem Kaufvertrag {{betrag}} € (keine Sonderzuständigkeit). Welches Gericht ist sachlich zuständig?",
            "{{gericht}}",
            "Ab 1.1.2026 AG bis 10.000 €, darüber LG, Sonderzuständigkeiten unberührt.",
            "exact",
            """{"name":{"type":"name"},"betrag":{"type":"int","min":3000,"max":16000},"gericht":{"type":"pick","items":["Amtsgericht oder Landgericht je Betrag"]}}"""),
        Tpl("tpl-ipv4", "t2-ipv4-ipv6", Difficulty.Grundlagen, ContentType.FillInTheBlank, "Zuordnen", 5, 2,
            "Die Adresse {{ip}} ist eine IPv4-Adresse im privaten Netz. Nennen Sie die Adressfamilie.",
            "IPv4",
            "Vier Dezimaloktetts, oft 192.168.0.0/16 privat.",
            "exact",
            """{"ip":{"type":"ipv4"}}"""),
        Tpl("tpl-uml-multi", "t1-uml-multi", Difficulty.Pruefung, ContentType.Uml, "Modellieren", 10, 6,
            "Kunde zu Ticket: ein Kunde hat viele Tickets, jedes Ticket genau einen Kunden. Nennen Sie die Multiplizitäten an beiden Enden (Kunde — Ticket).",
            "1  —  0..*",
            "1 am Kundenende, 0..* am Ticketende (Look-across).",
            "exact",
            """{}""")
    ];

    private static ExerciseTemplate Tpl(string id, string topic, Difficulty d, ContentType ct, string op, int min, int pts,
        string text, string sol, string expl, string eval, string schema)
        => new()
        {
            Id = id,
            TopicId = topic,
            Difficulty = d,
            ContentType = ct,
            Operator = op,
            EstimatedMinutes = min,
            Points = pts,
            TemplateText = text,
            SolutionTemplate = sol,
            ExplanationTemplate = expl,
            EvaluationStrategy = eval,
            HintsJson = """["Prüfen Sie Einheiten und Operator.","Grenzfälle nicht vergessen."]""",
            ParameterSchemaJson = schema,
            IsApprovedForExam = true
        };
}
