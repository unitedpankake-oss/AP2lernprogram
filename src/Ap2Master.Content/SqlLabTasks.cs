using Ap2Master.Domain.Evaluation;

namespace Ap2Master.Content;

/// <summary>
/// Aufgaben für das SQL-Labor auf dem HelpDesk-Schema. Die Kriterien werden zur Laufzeit
/// geprüft (Muster im SQL bzw. Vergleich der Ergebnismenge), die Punkte sind daher berechnet.
/// </summary>
public static class SqlLabTasks
{
    public static IReadOnlyList<SqlLabTask> All { get; } =
    [
        new SqlLabTask(
            Id: "sql-14-join-where",
            Number: 14,
            TopicId: "t2-sql-join",
            Title: "JOIN & WHERE",
            Description: "Listen Sie Titel und Kundenname aller Tickets mit dem Status 'offen' auf.",
            Requirements:
            [
                "Verbinden Sie Ticket und Kunde über die Fremdschlüsselbeziehung.",
                "Geben Sie den Titel des Tickets und den Namen des Kunden aus.",
                "Filtern Sie auf den Status 'offen'."
            ],
            Hints:
            [
                "Die Beziehung läuft über Ticket.KundenId = Kunde.Id.",
                "Der WHERE-Filter wird nach dem JOIN ausgewertet."
            ],
            StarterSql: "SELECT\n\nFROM Ticket t\n",
            ExpectedSql: "SELECT t.Titel, k.Name FROM Ticket t INNER JOIN Kunde k ON t.KundenId = k.Id WHERE t.Status = 'offen';",
            RequireOrder: false,
            Criteria:
            [
                new SqlCriterion("join", "JOIN korrekt verwendet", 3, SqlCriterionKind.SqlPattern, @"\bJOIN\b"),
                new SqlCriterion("on", "ON-Bedingung gesetzt", 2, SqlCriterionKind.SqlPattern, @"\bON\b\s+\w+\.\w+\s*=\s*\w+\.\w+"),
                new SqlCriterion("where", "Filter auf Status", 2, SqlCriterionKind.SqlPattern, @"\bWHERE\b[\s\S]*\bSTATUS\b"),
                new SqlCriterion("result", "Ergebnismenge korrekt", 3, SqlCriterionKind.ResultMatches)
            ]),

        new SqlLabTask(
            Id: "sql-15-group-having",
            Number: 15,
            TopicId: "t2-sql-select",
            Title: "GROUP BY & HAVING",
            Description: "Ermitteln Sie je Ort die Anzahl offener Tickets und geben Sie nur Orte mit mindestens zwei offenen Tickets aus.",
            Requirements:
            [
                "Zählen Sie die offenen Tickets je Ort des Kunden.",
                "Filtern Sie die Zeilen vor der Gruppierung auf den Status 'offen'.",
                "Schränken Sie die Gruppen auf mindestens zwei Tickets ein.",
                "Sortieren Sie aufsteigend nach Ort."
            ],
            Hints:
            [
                "WHERE filtert Zeilen, HAVING filtert Gruppen.",
                "COUNT(*) zählt die Zeilen einer Gruppe."
            ],
            StarterSql: "SELECT k.Ort\nFROM Ticket t\n",
            ExpectedSql: "SELECT k.Ort, COUNT(*) AS Anzahl FROM Ticket t JOIN Kunde k ON t.KundenId = k.Id "
                         + "WHERE t.Status = 'offen' GROUP BY k.Ort HAVING COUNT(*) >= 2 ORDER BY k.Ort;",
            RequireOrder: true,
            Criteria:
            [
                new SqlCriterion("group", "GROUP BY korrekt", 3, SqlCriterionKind.SqlPattern, @"\bGROUP\s+BY\b"),
                new SqlCriterion("having", "HAVING statt WHERE für Gruppen", 3, SqlCriterionKind.SqlPattern, @"\bHAVING\b"),
                new SqlCriterion("where", "Zeilenfilter vor der Gruppierung", 2, SqlCriterionKind.SqlPattern, @"\bWHERE\b"),
                new SqlCriterion("order", "Sortierung korrekt", 1, SqlCriterionKind.OrderMatches),
                new SqlCriterion("result", "Ergebnismenge korrekt", 3, SqlCriterionKind.ResultMatches)
            ]),

        new SqlLabTask(
            Id: "sql-16-left-join-group",
            Number: 16,
            TopicId: "t2-sql-join",
            Title: "JOIN & GROUP BY",
            Description: "Ermitteln Sie die Anzahl der Tickets pro Mitarbeiter. Auch Mitarbeiter ohne Tickets sollen in der Ergebnisliste erscheinen.",
            Requirements:
            [
                "Verwenden Sie LEFT JOIN, um alle Mitarbeiter einzubeziehen.",
                "Zählen Sie die Tickets pro Mitarbeiter.",
                "Gruppieren Sie nach Mitarbeiter.",
                "Sortieren Sie nach dem Namen."
            ],
            Hints:
            [
                "Verwenden Sie COUNT(t.Id), damit Mitarbeiter ohne Tickets den Wert 0 erhalten.",
                "COUNT(*) würde bei einem LEFT JOIN fälschlich 1 zählen."
            ],
            StarterSql: "SELECT\n    m.Id,\n    m.Name\nFROM Mitarbeiter m\n",
            ExpectedSql: "SELECT m.Id, m.Name, COUNT(t.Id) AS TicketAnzahl FROM Mitarbeiter m "
                         + "LEFT JOIN Ticket t ON t.MitarbeiterId = m.Id GROUP BY m.Id, m.Name ORDER BY m.Name;",
            RequireOrder: true,
            Criteria:
            [
                new SqlCriterion("leftjoin", "LEFT JOIN korrekt verwendet", 3, SqlCriterionKind.SqlPattern, @"\bLEFT\s+(OUTER\s+)?JOIN\b"),
                new SqlCriterion("countalias", "COUNT und Alias korrekt", 3, SqlCriterionKind.SqlPattern, @"\bCOUNT\s*\([^)]*\)\s+AS\s+[A-Z_][A-Z0-9_]*"),
                new SqlCriterion("group", "GROUP BY korrekt", 2, SqlCriterionKind.SqlPattern, @"\bGROUP\s+BY\b"),
                new SqlCriterion("order", "Sortierung korrekt", 2, SqlCriterionKind.OrderMatches),
                new SqlCriterion("result", "Ergebnismenge korrekt", 4, SqlCriterionKind.ResultMatches)
            ]),

        new SqlLabTask(
            Id: "sql-17-filter-sort",
            Number: 17,
            TopicId: "t2-sql-select",
            Title: "Filtern & Sortieren",
            Description: "Geben Sie Typ und Kaufjahr aller Geräte aus, die vor 2023 gekauft wurden, aufsteigend nach Kaufjahr.",
            Requirements:
            [
                "Filtern Sie auf Kaufjahr kleiner 2023.",
                "Sortieren Sie aufsteigend nach Kaufjahr."
            ],
            Hints:
            [
                "ORDER BY steht am Ende der Abfrage.",
                "Ohne ORDER BY ist die Reihenfolge in SQL nicht garantiert."
            ],
            StarterSql: "SELECT Typ, Kaufjahr\nFROM Geraet\n",
            ExpectedSql: "SELECT Typ, Kaufjahr FROM Geraet WHERE Kaufjahr < 2023 ORDER BY Kaufjahr;",
            RequireOrder: true,
            Criteria:
            [
                new SqlCriterion("where", "Filter auf Kaufjahr", 3, SqlCriterionKind.SqlPattern, @"\bWHERE\b[\s\S]*\bKAUFJAHR\b"),
                new SqlCriterion("order", "Sortierung korrekt", 2, SqlCriterionKind.OrderMatches),
                new SqlCriterion("result", "Ergebnismenge korrekt", 3, SqlCriterionKind.ResultMatches)
            ])
    ];

    public static SqlLabTask Default => All.First(t => t.Id == "sql-16-left-join-group");
}
