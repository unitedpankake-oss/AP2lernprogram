using Ap2Master.Domain;
using Ap2Master.Domain.Entities;

namespace Ap2Master.Content;

public static class LegalFactCatalog
{
    private static readonly DateTime V = new(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc);

    public static IReadOnlyList<LegalFact> All { get; } =
    [
        Fact("lf-textform", "t3-textform", "Ausbildungsvertrag in Textform",
            "Die wesentlichen Inhalte des Berufsausbildungsvertrags sind nach § 11 BBiG in Textform abzufassen (seit 1.8.2024). Eine eigenhändige Unterschrift ist nicht mehr Wirksamkeitsvoraussetzung der Abfassung.",
            "Nur klassische Schriftform mit Unterschrift mache den Vertrag gültig.",
            "§ 11 BBiG, § 126b BGB", new DateOnly(2024, 8, 1), null, "https://www.gesetze-im-internet.de/bbig_2005/__11.html"),
        Fact("lf-kuendigung-form", "t3-kuendigung-azubi", "Kündigung nach § 22 BBiG",
            "Kündigungen des Ausbildungsverhältnisses folgen § 22 BBiG inklusive der dort vorgeschriebenen Form. Das ist von der Textform der Vertragsabfassung nach § 11 zu trennen.",
            "Wer den Vertrag in Textform abfassen darf, dürfe auch analog in Textform kündigen.",
            "§ 22 BBiG", new DateOnly(2020, 1, 1), null, "https://www.gesetze-im-internet.de/bbig_2005/__22.html"),
        Fact("lf-ende-bekanntgabe", "t3-ende-ausbildung", "Ende bei Bestehen",
            "Bei vorzeitigem Bestehen endet das Ausbildungsverhältnis mit der Bekanntgabe des Ergebnisses durch den Prüfungsausschuss (§ 21 Abs. 2 BBiG).",
            "Ende automatisch am Tag der Prüfung ohne Bekanntgabe.",
            "§ 21 Abs. 2 BBiG", new DateOnly(2020, 1, 1), null, "https://www.gesetze-im-internet.de/bbig_2005/__21.html"),
        Fact("lf-mutterschutz", "t3-mutterschutz", "Schutzfrist nach der Geburt",
            "Die Schutzfrist nach der Entbindung beträgt grundsätzlich 8 Wochen, bei Früh- und Mehrlingsgeburten grundsätzlich 12 Wochen (§ 3 Abs. 2 MuSchG).",
            "Nach der Geburt immer 14 Wochen.",
            "§ 3 Abs. 2 MuSchG", new DateOnly(2018, 1, 1), null, "https://www.gesetze-im-internet.de/muschg_2018/__3.html"),
        Fact("lf-ag-10000", "t3-amtsgericht", "Amtsgericht Streitwert",
            "Ab 1.1.2026 liegt die allgemeine zivilrechtliche Wertgrenze des Amtsgerichts bei 10.000 €, vorbehaltlich besonderer Zuständigkeiten.",
            "Allgemeine Grenze 5.000 € (bis 31.12.2025 historisch).",
            "GVG", new DateOnly(2026, 1, 1), null, "Justizstandort-Stärkungsgesetz / GVG"),
        Fact("lf-bbg-rv", "t3-bbg", "BBG RV/AV 2026",
            "Beitragsbemessungsgrenze allgemeine RV und AV 2026: 101.400 € jährlich / 8.450 € monatlich, bundeseinheitlich.",
            null, "SVBezGrV 2026", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "https://www.gesetze-im-internet.de/svbezgrv_2026/"),
        Fact("lf-bbg-kv", "t3-bbg", "BBG KV/PV 2026",
            "Beitragsbemessungsgrenze KV/PV 2026: 69.750 € jährlich / 5.812,50 € monatlich.",
            null, "SVBezGrV 2026", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "https://www.gesetze-im-internet.de/svbezgrv_2026/"),
        Fact("lf-jaeg", "t3-jaeg", "JAEG 2026",
            "Allgemeine Jahresarbeitsentgeltgrenze 2026: 77.400 €. Besondere JAEG: 69.750 €.",
            "JAEG = BBG", "§ 6 SGB V, SVBezGrV 2026", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "https://www.gesetze-im-internet.de/svbezgrv_2026/"),
        Fact("lf-kv-satz", "t3-beitragsfinanzierung", "KV-Beitrag 2026",
            "Allgemeiner KV-Beitragssatz 14,6 %. Durchschnittlicher Zusatzbeitrag 2026: 2,9 % (kassenindividuell abweichend, hälftig getragen).",
            null, "SGB V, BMG", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "https://www.bundesgesundheitsministerium.de/beitraege"),
        Fact("lf-pv-satz", "t3-beitragsfinanzierung", "PV-Beitrag 2026",
            "PV-Beitragssatz 3,6 % (je 1,8 %). Kinderlosenzuschlag 0,6 Prozentpunkte ab 23 Jahren zulasten der Versicherten; weitere Differenzierung nach Kinderzahl.",
            null, "SGB XI", new DateOnly(2025, 1, 1), new DateOnly(2026, 12, 31), "vdek / SGB XI"),
        Fact("lf-rv-av", "t3-beitragsfinanzierung", "RV- und AV-Sätze 2026",
            "RV 18,6 % (je 9,3 %). AV 2,6 % (je 1,3 %).",
            null, "SGB III, SGB VI", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "DRV"),
        Fact("lf-mindestlohn", "t3-mindestlohn", "Mindestlohn 2026",
            "Gesetzlicher Mindestlohn ab 1.1.2026: 13,90 € brutto je Zeitstunde; ab 1.1.2027 14,60 €.",
            "12,82 € (Wert 2025) als aktuell.",
            "MiLoG / 5. Mindestlohnanpassungsverordnung", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "https://www.bmas.de"),
        Fact("lf-kartell", "t3-kartellverbot", "Kartellverbot aktuell",
            "Wettbewerbsbeschränkende Vereinbarungen sind nach § 1 GWB grundsätzlich verboten; Freistellung nach § 2 GWB möglich. Keine allgemeine Erlaubnis durch bloße Anmeldung eines Normen- oder Typenkartells.",
            "Normen- und Typenkartell ist anmeldepflichtig und deshalb grundsätzlich erlaubt.",
            "§§ 1, 2 GWB", new DateOnly(2005, 7, 1), null, "https://www.gesetze-im-internet.de/gwb/"),
        Fact("lf-wirtschaftlichkeit", "t1-wirtschaftlichkeit", "Formel Wirtschaftlichkeit",
            "Wirtschaftlichkeit = Ertrag / Aufwand.",
            "Wirtschaftlichkeit = Gewinn / Kapital",
            "BWL-Lehrkanon AP2", new DateOnly(2000, 1, 1), null, "intern"),
        Fact("lf-rentabilitaet", "t1-rentabilitaet", "Formel Rentabilität",
            "Rentabilität = Gewinn / eingesetztes Kapital × 100.",
            "Rentabilität = Ertrag / Aufwand",
            "BWL-Lehrkanon AP2", new DateOnly(2000, 1, 1), null, "intern"),
        Fact("lf-produktivitaet", "t1-wirtschaft", "Formel Produktivität",
            "Produktivität = Ausbringungsmenge / Einsatzmenge.",
            null, "BWL-Lehrkanon AP2", new DateOnly(2000, 1, 1), null, "intern")
    ];

    private static LegalFact Fact(string id, string topic, string title, string statement, string? historical, string legal, DateOnly from, DateOnly? until, string url)
        => new()
        {
            Id = id,
            TopicId = topic,
            Title = title,
            Statement = statement,
            HistoricalStatement = historical,
            LegalBasis = legal,
            ValidFrom = from,
            ValidUntil = until,
            VerifiedAt = V,
            VerificationStatus = VerificationStatus.Verified,
            SourceUrl = url,
            Notes = until is null ? "" : "Jahresbezogene Rechengröße, vor neuer Verordnung prüfen."
        };
}

public static class ConflictCatalog
{
    public static IReadOnlyList<ContentConflict> All { get; } =
    [
        C("t3-textform", "Nur Schriftform mit Unterschrift macht den Ausbildungsvertrag gültig.", "AP2_Lernzettel 1.docx (erwartet)",
            "Abfassung in Textform nach § 11 BBiG genügt; Empfangsnachweis bei elektronischer Übermittlung.", "BBiG / BVaDiG",
            ConflictResolution.UseLegislation, "Textform nach § 11 BBiG, Kündigung getrennt nach § 22.", true),
        C("t3-kuendigung-azubi", "Kündigung analog Textform der Vertragsabfassung.", "Skript-Vermischung (erwartet)",
            "§ 22 BBiG enthält eigene Form- und Inhaltsregeln für Kündigungen.", "BBiG",
            ConflictResolution.UseLegislation, "Vertragsabfassung ≠ Kündigung.", true),
        C("t3-ende-ausbildung", "Ende am Tag der Prüfung.", "AP2_Vorbereitungskurs_FIAE.docx (erwartet)",
            "Ende mit Bekanntgabe des Ergebnisses durch den Prüfungsausschuss.", "§ 21 Abs. 2 BBiG",
            ConflictResolution.UseLegislation, "Bekanntgabe ist der Anknüpfungspunkt.", true),
        C("t3-mutterschutz", "Nach der Geburt 14 Wochen Mutterschutz.", "Lernzettel (erwartet)",
            "Grundsätzlich 8 Wochen, Früh-/Mehrlinge 12 Wochen nach der Geburt.", "§ 3 MuSchG",
            ConflictResolution.UseLegislation, "14 Wochen verwechselt Gesamtfrist 6+8 mit Nachschutz.", true),
        C("t1-wirtschaftlichkeit", "Wirtschaftlichkeit = Gewinn/Kapital oder Ertrag−Aufwand.", "Lernzettel-Formeln (erwartet)",
            "Wirtschaftlichkeit = Ertrag/Aufwand; Rentabilität = Gewinn/Kapital×100; Produktivität = Ausbringung/Einsatz.",
            "BWL-Standard der Prüfung", ConflictResolution.UseIhkFramework, "Drei Formeln strikt trennen.", true),
        C("t3-amtsgericht", "Amtsgericht bis 5.000 €.", "Ältere Skripte / historische IHK-Aufgaben",
            "Ab 1.1.2026 allgemeine Grenze 10.000 €, Sonderzuständigkeiten unberührt.", "GVG 2026",
            ConflictResolution.UseLegislation, "5.000 € nur noch als historische Angabe.", true),
        C("t3-kartellverbot", "Normen- und Typenkartell anmeldepflichtig und grundsätzlich erlaubt.", "Ältere WiSo-Skripte zum alten GWB",
            "Grundsatz: Verbot wettbewerbsbeschränkender Vereinbarungen, Freistellung nach § 2 GWB möglich.", "GWB aktuell",
            ConflictResolution.UseLegislation, "Altes Anmeldesystem nicht als Universalregel.", true),
        C("t2-tls", "RSA-only Handshake (Pre-Master mit Server-RSA-Key) ist der normale Ablauf.", "Vorbereitungskurs-Krypto (erwartet)",
            "Modern: Zertifikat prüfen, (EC)DHE-Schlüsselvereinbarung, symmetrische Session-Cipher (TLS 1.2/1.3).", "RFC 8446",
            ConflictResolution.UseTechnicalStandard, "Historisches RSA-Key-Transport-Beispiel nur als historisch kennzeichnen.", true),
        C("t2-owasp", "OWASP-Kategorien ohne Jahreszahl mischen (A1 Injection 2017 mit A01 2021).", "Lernzettel-Listen (erwartet)",
            "Edition angeben; 2017 und 2021 nicht vermengen.", "OWASP Top 10:2021",
            ConflictResolution.UseTechnicalStandard, "Historische Listen bleiben Lernmaterial mit Label.", true)
    ];

    private static ContentConflict C(string topic, string a, string sa, string b, string sb, ConflictResolution r, string rationale, bool resolved)
        => new()
        {
            Id = Guid.NewGuid(),
            TopicId = topic,
            ClaimA = a,
            SourceA = sa,
            ClaimB = b,
            SourceB = sb,
            Resolution = r,
            ResolvedClaim = b,
            Rationale = rationale,
            IsResolved = resolved
        };
}
