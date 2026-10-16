using System.Text.Json;
using Ap2Master.Domain;
using Ap2Master.Domain.Coverage;
using Ap2Master.Domain.Entities;

namespace Ap2Master.Content;

public static class SpecialExercises
{
    public static IReadOnlyList<LearningContent> Create(IReadOnlyList<Topic> topics, DateTime now)
    {
        var cfg = ControlFlowGraph.IfThenElse();

        return
        [
            new LearningContent
            {
                Id = "cov-demo-1",
                Title = "Zweigüberdeckung berechnen",
                TopicId = "t2-branch-cov",
                ExamPart = ExamPart.Teil2,
                Difficulty = Difficulty.Pruefung,
                ContentType = ContentType.ControlFlow,
                EstimatedMinutes = 10,
                Points = 8,
                Question = "Ein if-then-else hat zwei Pfade. Welche Zweigüberdeckung erreichen Sie, wenn nur der then-Pfad ausgeführt wird?",
                Answer = "50%",
                Explanation = "2 von 4 Kanten liegen auf dem then-Pfad nicht vollständig – in diesem Graphen then: s1->s2 und s2->s4 (2 von 4 = 50 %).",
                PayloadJson = JsonSerializer.Serialize(cfg),
                SourceDocument = "NormalizedSyllabus",
                SourceSection = "Whitebox",
                SourceType = "coverage",
                VerifiedAt = now,
                VerificationStatus = VerificationStatus.Verified,
                ContentVersion = "1.0.0"
            },
            new LearningContent
            {
                Id = "sql-demo-join",
                Title = "Tickets mit Kundenname",
                TopicId = "t2-sql-join",
                ExamPart = ExamPart.Teil2,
                Difficulty = Difficulty.Pruefung,
                ContentType = ContentType.Sql,
                EstimatedMinutes = 12,
                Points = 8,
                Question = "Listen Sie Titel und Kundenname aller Tickets mit Status 'offen' per INNER JOIN.",
                Answer = "SELECT t.Titel, k.Name FROM Ticket t INNER JOIN Kunde k ON t.KundenId = k.Id WHERE t.Status = 'offen';",
                Explanation = "Bewertung über die Ergebnismenge, nicht über Stringgleichheit.",
                PayloadJson = """{"requireOrder":false}""",
                SourceDocument = "HelpDesk-Labor",
                SourceSection = "JOIN",
                SourceType = "sql",
                VerifiedAt = now,
                VerificationStatus = VerificationStatus.Verified,
                HintsJson = """["JOIN ... ON","WHERE nach dem Join"]""",
                ContentVersion = "1.0.0"
            },
            new LearningContent
            {
                Id = "pseudo-max",
                Title = "Maximum in einer Liste",
                TopicId = "t2-minmax",
                ExamPart = ExamPart.Teil2,
                Difficulty = Difficulty.Grundlagen,
                ContentType = ContentType.Pseudocode,
                EstimatedMinutes = 12,
                Points = 8,
                Question = "Schreiben Sie Pseudocode, der das Maximum von liste mit n Elementen (Index 0..n-1) zurückgibt.",
                Answer = "SET max = liste[0]\nFOR i FROM 1 TO n-1\nIF liste[i] > max THEN SET max = liste[i] ENDIF\nENDFOR\nRETURN max",
                Explanation = "Initialisierung mit dem ersten Element vermeidet den Fehler bei negativen Zahlen.",
                PayloadJson = """{"tests":[{"liste":[3,9,2],"n":3,"expected":9},{"liste":[-4,-1,-7],"n":3,"expected":-1}]}""",
                SourceDocument = "NormalizedSyllabus",
                SourceSection = "Algorithmen",
                SourceType = "pseudocode",
                VerifiedAt = now,
                VerificationStatus = VerificationStatus.Verified,
                ContentVersion = "1.0.0"
            },
            new LearningContent
            {
                Id = "uml-include-dir",
                Title = "Include-Richtung",
                TopicId = "t1-uml-include-extend",
                ExamPart = ExamPart.Teil1,
                Difficulty = Difficulty.Pruefung,
                ContentType = ContentType.Uml,
                EstimatedMinutes = 8,
                Points = 4,
                Question = "Wohin zeigt der «include»-Pfeil?",
                Answer = "Zum included Use Case (vom Basis-Use-Case aus).",
                Explanation = "include ist zwingende Einbindung; der Pfeil zeigt auf den eingebundenen Fall. extend zeigt auf den Basis-Use-Case.",
                SourceDocument = "NormalizedSyllabus",
                SourceSection = "Use-Case",
                SourceType = "uml",
                VerifiedAt = now,
                VerificationStatus = VerificationStatus.Verified,
                ContentVersion = "1.0.0"
            },
            Mock("mock-t1-stake", "t1-stakeholder", ExamPart.Teil1, "Stakeholder im Helpdesk-Projekt",
                "Nennen Sie vier Stakeholder eines internen Helpdesk-Projekts und ordnen Sie je eine Kommunikationsstrategie gemäß Einfluss/Interesse zu.",
                "Auftraggeber/Serviceleitung: eng führen; Agenten: informieren und einbinden; Datenschutz: zufriedenstellen; IT-Betrieb: eng führen oder zufriedenstellen je nach Einfluss.",
                20, 12, now),
            Mock("mock-t2-sql", "t2-sql-select", ExamPart.Teil2, "Auswertung offener Tickets",
                "Formulieren Sie eine SQL-Abfrage, die je Ort die Anzahl offener Tickets ermittelt und nur Orte mit mindestens 2 offenen Tickets ausgibt.",
                "SELECT k.Ort, COUNT(*) FROM Ticket t JOIN Kunde k ON t.KundenId=k.Id WHERE t.Status='offen' GROUP BY k.Ort HAVING COUNT(*)>=2;",
                20, 15, now)
        ];
    }

    private static LearningContent Mock(string id, string topic, ExamPart part, string title, string q, string a, int min, int pts, DateTime now)
        => new()
        {
            Id = id,
            Title = title,
            TopicId = topic,
            ExamPart = part,
            Difficulty = Difficulty.Pruefung,
            ContentType = ContentType.MockExamTask,
            EstimatedMinutes = min,
            Points = pts,
            Question = q,
            Answer = a,
            Explanation = a,
            SourceDocument = "Probeklausur-Kanon",
            SourceSection = title,
            SourceType = "mock",
            VerifiedAt = now,
            VerificationStatus = VerificationStatus.Verified,
            ContentVersion = "1.0.0"
        };
}

public static class HelpDeskSchema
{
    public const string Sql = """
        DROP TABLE IF EXISTS Kommentar;
        DROP TABLE IF EXISTS Ticket;
        DROP TABLE IF EXISTS Geraet;
        DROP TABLE IF EXISTS Mitarbeiter;
        DROP TABLE IF EXISTS Kunde;
        CREATE TABLE Kunde (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL, Ort TEXT NOT NULL, Email TEXT);
        CREATE TABLE Mitarbeiter (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL, Rolle TEXT NOT NULL);
        CREATE TABLE Ticket (
          Id INTEGER PRIMARY KEY,
          KundenId INTEGER NOT NULL REFERENCES Kunde(Id),
          MitarbeiterId INTEGER REFERENCES Mitarbeiter(Id),
          Titel TEXT NOT NULL,
          Status TEXT NOT NULL,
          Prioritaet INTEGER NOT NULL CHECK (Prioritaet BETWEEN 1 AND 5),
          ErstelltAm TEXT NOT NULL,
          GeschlossenAm TEXT
        );
        CREATE TABLE Geraet (Id INTEGER PRIMARY KEY, KundenId INTEGER NOT NULL REFERENCES Kunde(Id), Typ TEXT NOT NULL, Kaufjahr INTEGER);
        CREATE TABLE Kommentar (Id INTEGER PRIMARY KEY, TicketId INTEGER NOT NULL REFERENCES Ticket(Id), Autor TEXT NOT NULL, Text TEXT NOT NULL);
        INSERT INTO Kunde (Id, Name, Ort, Email) VALUES
          (1,'Nordsoft AG','Hamburg','it@nordsoft.example'),
          (2,'HafenCode GmbH','Hamburg','help@hafencode.example'),
          (3,'Klarblick UG','Berlin','info@klarblick.example'),
          (4,'Aurora Systems KG','München','support@aurora.example'),
          (5,'Helpline GmbH','Köln','office@helpline.example');
        INSERT INTO Mitarbeiter (Id, Name, Rolle) VALUES
          (1,'Aylin Kaya','Agentin'),
          (2,'Jonas Weber','Agent'),
          (3,'Lea Hoffmann','Teamleitung');
        INSERT INTO Ticket (Id, KundenId, MitarbeiterId, Titel, Status, Prioritaet, ErstelltAm, GeschlossenAm) VALUES
          (1,1,1,'VPN bricht ab','offen',1,'2026-03-01',NULL),
          (2,1,2,'Drucker ohne Papier','geschlossen',4,'2026-02-11','2026-02-12'),
          (3,2,1,'Neues Konto','offen',2,'2026-03-04',NULL),
          (4,2,3,'Berechtigung SAP','offen',1,'2026-03-05',NULL),
          (5,3,2,'Passwort vergessen','geschlossen',3,'2026-01-20','2026-01-20'),
          (6,3,1,'Laptop defekt','offen',1,'2026-03-06',NULL),
          (7,4,2,'WLAN langsam','offen',3,'2026-03-07',NULL),
          (8,4,1,'Phishing-Mail','geschlossen',1,'2026-02-28','2026-03-01'),
          (9,5,3,'Onboarding Azubi','offen',2,'2026-03-08',NULL),
          (10,5,2,'Monitor flackert','offen',4,'2026-03-08',NULL),
          (11,1,1,'Zertifikat abgelaufen','offen',1,'2026-03-09',NULL),
          (12,2,2,'Spamfilter zu scharf','offen',3,'2026-03-09',NULL);
        INSERT INTO Geraet (Id, KundenId, Typ, Kaufjahr) VALUES
          (1,1,'Laptop',2024),(2,1,'Drucker',2021),(3,2,'Laptop',2025),(4,3,'Monitor',2023),(5,5,'Laptop',2022);
        INSERT INTO Kommentar (Id, TicketId, Autor, Text) VALUES
          (1,1,'Aylin Kaya','Logs angefordert'),
          (2,4,'Lea Hoffmann','Mit Fachbereich abstimmen');
        """;
}

public static class SourceDocumentInventory
{
    public const string Vorbereitungskurs = "AP2_Vorbereitungskurs_FIAE.docx";
    public const string Lernzettel = "AP2_Lernzettel 1.docx";

    public static IReadOnlyList<string> ExpectedPaths { get; } =
    [
        "LearningSources/AP2_Vorbereitungskurs_FIAE.docx",
        "LearningSources/AP2_Lernzettel 1.docx"
    ];
}
