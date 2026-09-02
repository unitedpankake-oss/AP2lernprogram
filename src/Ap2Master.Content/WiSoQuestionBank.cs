using Ap2Master.Domain;
using Ap2Master.Domain.Entities;

namespace Ap2Master.Content;

public static class WiSoQuestionBank
{
    public static IReadOnlyList<LearningContent> Create(IReadOnlyList<Topic> topics, DateTime now)
    {
        var list = new List<LearningContent>();
        void Q(string id, string topic, string title, string q, int correct, string[] options, string[] why, bool multi = false)
        {
            var type = multi ? ContentType.MultipleChoice : ContentType.SingleChoice;
            var c = new LearningContent
            {
                Id = id,
                Title = title,
                TopicId = topic,
                ExamPart = ExamPart.WiSo,
                Difficulty = Difficulty.Pruefung,
                ContentType = type,
                EstimatedMinutes = 2,
                Points = 1,
                Question = q,
                Answer = options[correct],
                Explanation = why[correct],
                SourceDocument = "Amtliche Quellen + korrigierter AP2-Kanon",
                SourceSection = title,
                SourceType = "wiso-bank",
                VerifiedAt = now,
                VerificationStatus = VerificationStatus.Verified,
                ContentVersion = "2026.1",
                TagsCsv = "wiso,pruefung"
            };
            for (var i = 0; i < options.Length; i++)
            {
                c.Options.Add(new AnswerOption
                {
                    Id = Guid.NewGuid(),
                    LearningContentId = id,
                    Text = options[i],
                    IsCorrect = multi ? why[i].StartsWith("Richtig:") : i == correct,
                    Explanation = why[i],
                    SortOrder = i
                });
            }
            if (multi) c.Answer = string.Join('|', c.Options.Where(o => o.IsCorrect).Select(o => o.Text));
            list.Add(c);
        }

        Q("wiso-01", "t3-textform", "Textform Ausbildungsvertrag",
            "Welche Aussage zur Abfassung des Berufsausbildungsvertrags gilt seit dem 1.8.2024?",
            1,
            ["Er ist nur mit eigenhändiger Unterschrift aller Parteien wirksam.", "Die wesentlichen Inhalte sind in Textform abzufassen; bei elektronischer Übermittlung ist der Empfang nachzuweisen.", "Eine mündliche Absprache ersetzt jede Abfassung.", "Es genügt eine WhatsApp-Sprachnachricht ohne speicherbaren Text."],
            ["Historische Schriftform-only-Regel, überholt.", "§ 11 BBiG i. d. F. BVaDiG.", "Abfassung bleibt Pflicht.", "Textform verlangt lesbaren Text auf dauerhaftem Datenträger."]);

        Q("wiso-02", "t3-kuendigung-azubi", "Kündigung Ausbildung",
            "Was gilt für die Kündigung eines Berufsausbildungsvertrags?",
            2,
            ["Sie folgt stets derselben Form wie die Vertragsabfassung nach § 11.", "Nach der Probezeit ist jederzeit eine formlose SMS genug.", "Es gilt § 22 BBiG mit eigenen Voraussetzungen und Form; das ist von § 11 zu trennen.", "Eine Kündigung ist nach der Probezeit ausgeschlossen."],
            ["Vermischung § 11 und § 22.", "Formlose SMS erfüllt § 22 nicht.", "Richtig.", "Nach Probezeit aus wichtigem Grund bzw. durch Azubi mit Frist möglich."]);

        Q("wiso-03", "t3-ende-ausbildung", "Ende bei Bestehen",
            "Die Abschlussprüfung wird vor Ablauf der Ausbildungszeit bestanden. Wann endet das Ausbildungsverhältnis?",
            1,
            ["Am Kalendertag der letzten Prüfungsleistung ohne weiteren Akt.", "Mit der Bekanntgabe des Ergebnisses durch den Prüfungsausschuss.", "Erst mit Ablauf des im Vertrag genannten Enddatums trotz Bestehen.", "Am Tag der Zeugnisübergabe durch den Betrieb."],
            ["Zu unbestimmt, Gesetz stellt auf Bekanntgabe ab.", "§ 21 Abs. 2 BBiG.", "Vorzeitiges Ende ist gesetzlich vorgesehen.", "Nicht der gesetzliche Anknüpfungspunkt."]);

        Q("wiso-04", "t3-mutterschutz", "Mutterschutz nach Geburt",
            "Wie lange dauert die Schutzfrist nach der Entbindung grundsätzlich?",
            0,
            ["8 Wochen, bei Früh- und Mehrlingen grundsätzlich 12 Wochen.", "Immer 14 Wochen nach der Geburt.", "6 Wochen nach der Geburt.", "Nur 3 Tage."],
            ["§ 3 Abs. 2 MuSchG.", "Verwechselt oft 6+8=14 mit der Nachfrist.", "6 Wochen sind typisch vor der Entbindung.", "Unzutreffend."]);

        Q("wiso-05", "t1-wirtschaftlichkeit", "Wirtschaftlichkeit",
            "Welche Formel beschreibt die Wirtschaftlichkeit?",
            2,
            ["Gewinn / eingesetztes Kapital × 100", "Ausbringungsmenge / Einsatzmenge", "Ertrag / Aufwand", "Ertrag − Aufwand"],
            ["Das ist Rentabilität.", "Das ist Produktivität.", "Richtig.", "Das ist ein Erfolg, aber nicht die Wirtschaftlichkeit als Quotient."]);

        Q("wiso-06", "t3-amtsgericht", "Amtsgericht 2026",
            "Welche allgemeine Wertgrenze gilt 2026 für Zivilsachen beim Amtsgericht (ohne Sonderzuständigkeit)?",
            2,
            ["2.000 €", "5.000 €", "10.000 €", "25.000 €"],
            ["Zu niedrig.", "Historische Grenze bis 31.12.2025.", "Seit 1.1.2026.", "Nicht die allgemeine AG-Grenze."],
            false);
        list[^1].ValidFrom = new DateOnly(2026, 1, 1);
        list[^1].LegalBasis = "GVG";
        list[^1].TimeSensitiveFix();

        Q("wiso-07", "t3-kartellverbot", "Kartellverbot",
            "Welche Aussage zum Kartellrecht ist aktuell zutreffend?",
            1,
            ["Normen- und Typenkartelle sind nach Anmeldung grundsätzlich erlaubt.", "Wettbewerbsbeschränkende Vereinbarungen sind grundsätzlich verboten; Ausnahmen folgen Freistellungsregeln (u. a. § 2 GWB).", "Jedes Oligopol ist ein verbotenes Kartell.", "Preisabsprachen sind erlaubt, wenn sie schriftlich sind."],
            ["Überholte GWB-Lehrformel.", "Richtig.", "Marktform ≠ Kartell.", "Schriftform heilt keinen Verstoß."]);

        Q("wiso-08", "t3-bbg", "BBG 2026",
            "Wie hoch ist 2026 die jährliche Beitragsbemessungsgrenze in der allgemeinen Rentenversicherung?",
            2,
            ["69.750 €", "77.400 €", "101.400 €", "124.800 €"],
            ["Das ist die BBG KV/PV.", "Das ist die allgemeine JAEG.", "SVBezGrV 2026.", "Knappschaftliche RV."]);
        list[^1].ValidFrom = new DateOnly(2026, 1, 1);
        list[^1].ValidUntil = new DateOnly(2026, 12, 31);
        list[^1].LegalBasis = "SVBezGrV 2026";

        Q("wiso-09", "t3-jaeg", "JAEG",
            "Wofür ist die Jahresarbeitsentgeltgrenze maßgeblich?",
            0,
            ["Für die Versicherungspflicht in der gesetzlichen Krankenversicherung (Wechselmöglichkeit PKV).", "Für die maximale Höhe jedes SV-Beitrags in allen Zweigen.", "Für den gesetzlichen Mindestlohn.", "Für die Zuständigkeit des Amtsgerichts."],
            ["Richtig.", "Das ist die BBG je Zweig.", "MiLoG.", "GVG."]);

        Q("wiso-10", "t3-mindestlohn", "Mindestlohn 2026",
            "Wie hoch ist der allgemeine gesetzliche Mindestlohn ab 1.1.2026?",
            1,
            ["12,82 €", "13,90 €", "14,60 €", "15,00 €"],
            ["Wert 2025.", "BMAS / MiLoG 2026.", "Wert ab 1.1.2027.", "Politische Forderung, nicht der gesetzliche Satz 2026."]);
        list[^1].ValidFrom = new DateOnly(2026, 1, 1);
        list[^1].ValidUntil = new DateOnly(2026, 12, 31);

        Q("wiso-11", "t3-probezeit", "Probezeit BBiG",
            "Welche Dauer der Probezeit im Ausbildungsverhältnis ist gesetzlich zulässig?",
            2,
            ["Mindestens eine Woche, höchstens sechs Monate.", "Beliebig, wenn tariflich.", "Mindestens ein Monat, höchstens vier Monate.", "Genau drei Monate in jedem Fall."],
            ["Zu kurz/zu lang.", "Tarif kann nicht unter das gesetzliche Minimum.", "§ 20 BBiG.", "3 Monate sind üblich, nicht die einzige legale Dauer."]);

        Q("wiso-12", "t3-hra-hrb", "HRA HRB",
            "Wo wird eine GmbH eingetragen?",
            1,
            ["HRA", "HRB", "Genossenschaftsregister", "Vereinsregister"],
            ["Personengesellschaften/Kaufleute.", "Kapitalgesellschaften.", "eG.", "e. V."]);

        Q("wiso-13", "t3-gmbh", "GmbH Kapital",
            "Wie hoch ist das gesetzliche Mindeststammkapital einer GmbH?",
            2,
            ["1 €", "12.500 €", "25.000 €", "50.000 €"],
            ["UG-Minimum.", "Mindesteinzahlung bei Bargründung, nicht das Stammkapital.", "GmbH.", "AG."]);

        Q("wiso-14", "t3-fuenf-zweige", "Fünf Zweige",
            "Welcher Zweig gehört nicht zu den fünf Zweigen der Sozialversicherung?",
            3,
            ["Unfallversicherung", "Arbeitslosenversicherung", "Pflegeversicherung", "Private Haftpflichtversicherung"],
            ["Gehört dazu.", "Gehört dazu.", "Gehört dazu.", "Privatrecht, kein SV-Zweiz."]);

        Q("wiso-15", "t3-top", "TOP",
            "Was bedeutet das TOP-Prinzip im Arbeitsschutz?",
            0,
            ["Technische vor organisatorischen vor persönlichen Maßnahmen.", "Tarif vor Organisation vor Personalrat.", "Training, Organisation, Prävention als gleichrangig ohne Reihenfolge.", "Totale Optimierung der Produktivität zuerst."],
            ["Richtig.", "Falsche Auflösung.", "Reihenfolge ist wesentlich.", "Kein Arbeitsschutzprinzip."]);

        Q("wiso-16", "t3-guenstigkeit", "Günstigkeit",
            "Was besagt das Günstigkeitsprinzip?",
            1,
            ["Der Arbeitgeber darf jederzeit vom Tarif nach unten abweichen.", "Für Beschäftigte günstigere vertragliche Abweichungen vom Tarif bleiben wirksam.", "Betriebsvereinbarungen ersetzen immer den Tarif.", "Das älteste Recht gilt."],
            ["Unzutreffend.", "Richtig.", "Tarifvorrang.", "Unzutreffend."]);

        Q("wiso-17", "t3-sql-null-wiso-skip", "Geschäftsfähigkeit",
            "Ab welchem Alter ist eine Person voll geschäftsfähig?",
            2,
            ["14 Jahre", "16 Jahre", "18 Jahre", "21 Jahre"],
            ["Strafrechtliche Stufen, nicht voll geschäftsfähig.", "Beschränkt geschäftsfähig.", "§ 2 BGB Volljährigkeit.", "Historisch."]);
        list[^1].TopicId = "t3-geschaeftsfaehigkeit";

        Q("wiso-18", "t3-inner-join-skip", "UV Träger",
            "Wer trägt die Beiträge zur gesetzlichen Unfallversicherung?",
            0,
            ["Allein der Arbeitgeber (Umlage, Berufsgenossenschaft).", "Hälftig AG und AN.", "Allein der Arbeitnehmer.", "Die Krankenkasse aus dem Zusatzbeitrag."],
            ["Richtig.", "Gilt nicht für UV.", "Nein.", "Nein."]);
        list[^1].TopicId = "t3-kv-pv-rv-av-uv";

        Q("wiso-19", "t3-inflation", "Inflation",
            "Was misst der Verbraucherpreisindex in erster Linie?",
            1,
            ["Den Aktienindex der DAX-Unternehmen.", "Die Preisentwicklung eines repräsentativen Warenkorbs privater Haushalte.", "Den Leitzins der EZB.", "Das Staatsdefizit."],
            ["Nein.", "Richtig.", "Geldpolitik.", "Fiskal."]);

        Q("wiso-20", "t3-ezb", "EZB-Ziel",
            "Was ist das vorrangige Ziel der EZB?",
            0,
            ["Preisstabilität im Euroraum.", "Vollbeschäftigung als gleichrangiges Mandat wie bei der Fed.", "Festsetzung des gesetzlichen Mindestlohns.", "Abschluss von Tarifverträgen."],
            ["Richtig.", "EZB-Primat ist Preisstabilität.", "Gesetzgeber/Mindestlohnkommission.", "Tarifautonomie."]);

        Q("wiso-21", "t3-agg-merkmale", "AGG",
            "Welches Merkmal gehört zu den im AGG genannten Diskriminierungsgründen?",
            2,
            ["Wohnort", "Lieblingssprache der Fachabteilung", "Geschlecht", "Sternzeichen"],
            ["Kein AGG-Merkmal.", "Nein.", "Ja.", "Nein."]);

        Q("wiso-22", "t3-betriebsrat", "Betriebsrat",
            "Ab welcher regelmäßigen Beschäftigtenzahl kann ein Betriebsrat gewählt werden?",
            1,
            ["2", "5", "50", "500"],
            ["Zu niedrig.", "BetrVG.", "Schwelle für andere Gremien/Mitbestimmungsebenen, nicht die Wahlschwelle.", "Konzernvorstellungen."]);

        Q("wiso-23", "t3-verzug", "Mahnung",
            "Welche Aussage zum Schuldnerverzug ist richtig?",
            2,
            ["Es sind gesetzlich immer drei Mahnungen nötig.", "Eine Rechnung setzt immer automatisch Verzug.", "Eine Mahnung setzt bei Fälligkeit Verzug, sofern sie nicht entbehrlich ist.", "Verzug gibt es nur bei Verbrauchern."],
            ["Mythos.", "Nicht immer.", "Richtig.", "Auch unternehmern."]);

        Q("wiso-24", "t3-ruesselpflicht", "§ 377 HGB",
            "Für wen gilt die kaufmännische Untersuchungs- und Rügeobliegenheit?",
            0,
            ["Für beiderseitige Handelsgeschäfte, nicht für Verbraucherkäufe.", "Für jeden Privatkauf im Internet.", "Nur für Immobilien.", "Nur für Ausbildungsverträge."],
            ["Richtig.", "Verbraucher werden nicht so behandelt.", "Nein.", "Nein."]);

        Q("wiso-25", "t3-liniensysteme", "Stab",
            "Welche Aussage zum Stabliniensystem ist zutreffend?",
            1,
            ["Stäbe haben dasselbe Weisungsrecht wie Linienvorgesetzte.", "Stäbe beraten die Linie, weisen in der reinen Form nicht an.", "Es gibt keine klare Instanz.", "Es ist identisch mit der Matrix."],
            ["Nein.", "Richtig.", "Linie bleibt.", "Matrix = zwei Weisungsdimensionen."]);

        Q("wiso-26", "t3-verursacher", "Umweltprinzipien",
            "Was beschreibt das Verursacherprinzip?",
            0,
            ["Wer Schäden verursacht, soll die Kosten tragen.", "Die Allgemeinheit zahlt immer zuerst.", "Maßnahmen erst nach Vollbeweis jedes Schadens.", "Nur der Staat darf emittieren."],
            ["Richtig.", "Gemeinlast.", "Gegenteil von Vorsorge, nicht Verursacher.", "Nein."]);

        Q("wiso-27", "t3-liquiditaet", "Liquidität",
            "Warum kann ein Unternehmen trotz Gewinn zahlungsunfähig werden?",
            2,
            ["Gewinn und Liquidität sind identisch.", "AfA erhöht die Bankguthaben.", "Ein- und Auszahlungen fallen zeitlich anders an als Erträge und Aufwendungen.", "Gewinnsteuern sind illegal."],
            ["Genau nicht.", "AfA ist nicht zahlungswirksam.", "Richtig.", "Nein."]);

        Q("wiso-28", "t3-ug", "UG",
            "Welche Aussage zur UG (haftungsbeschränkt) ist richtig?",
            1,
            ["Sie benötigt 25.000 € Stammkapital wie die GmbH.", "Sie kann mit geringem Stammkapital gegründet werden und muss 25 % des Jahresüberschusses in eine Rücklage einstellen, bis 25.000 € erreicht sind.", "Gesellschafter haften wie bei der OHG unbeschränkt.", "Sie wird im HRA eingetragen."],
            ["Das ist die GmbH.", "Richtig.", "Trennungsprinzip.", "HRB."]);

        Q("wiso-29", "t3-frieden", "Friedenspflicht",
            "Was bedeutet relative Friedenspflicht?",
            0,
            ["Während der Laufzeit darf über die tariflich geregelten Materien nicht gestreikt werden.", "Es darf niemals gestreikt werden.", "Nur der Staat darf zum Streik aufrufen.", "Friedenspflicht gilt nur für Beamte."],
            ["Richtig.", "Zu absolut.", "Tarifautonomie.", "Beamte haben kein Streikrecht, das ist ein anderes Institut."]);

        Q("wiso-30", "t3-schilder", "Sicherheitszeichen",
            "Wie sind Rettungszeichen gekennzeichnet?",
            2,
            ["Rot, rund, Querbalken", "Gelb, dreieckig", "Grün, quadratisch/rechteckig mit Piktogramm", "Blau, rund, ohne Bedeutung der Form"],
            ["Verbot.", "Warnung.", "Rettung.", "Gebot ist blau rund – Form gehört dazu."]);

        Q("wiso-31", "t3-nachwg", "Nachweisgesetz",
            "Was bewirkt das Fehlen eines Nachweises nach NachwG grundsätzlich?",
            1,
            ["Der Arbeitsvertrag ist stets nichtig.", "Der Vertrag kann gleichwohl bestehen; der Arbeitgeber verletzt eine Nachweispflicht (bußgeldbewehrt).", "Es entsteht automatisch ein Ausbildungsverhältnis.", "Der Lohnanspruch entfällt."],
            ["Wirksamkeit ≠ Nachweis.", "Richtig.", "Nein.", "Nein."]);

        Q("wiso-32", "t3-ksg", "KSchG Wartezeit",
            "Welche Wartezeit gilt typischerweise für den allgemeinen Kündigungsschutz nach KSchG?",
            2,
            ["1 Woche", "4 Monate wie BBiG-Probezeit", "6 Monate", "2 Jahre"],
            ["Nein.", "Andere Rechtsquelle.", "§ 1 KSchG.", "Zu lang."]);

        Q("wiso-33", "t3-soziale-marktwirtschaft", "Soziale Marktwirtschaft",
            "Welche Kombination beschreibt die soziale Marktwirtschaft am treffendsten?",
            0,
            ["Wettbewerb und Privateigentum plus sozialer Ausgleich und Ordnungsrahmen.", "Zentrale Mengenplanung aller Güter.", "Vollständiges Laissez-faire ohne Kartellrecht.", "Nur Staatsunternehmen."],
            ["Richtig.", "Planwirtschaft.", "Unsoziale Marktidee ohne Ordnung.", "Nein."]);

        Q("wiso-34", "t3-angebot-nachfrage", "Gleichgewichtspreis",
            "Was entsteht, wenn der Preis über dem Gleichgewichtspreis liegt?",
            1,
            ["Nachfrageüberhang", "Angebotsüberhang", "Immer ein Monopol", "Inflation per Definition"],
            ["Das wäre unter dem GG-Preis.", "Richtig.", "Nicht zwingend.", "Preisniveau ≠ Markträumung eines Gutes."]);

        Q("wiso-35", "t2-tls", "TLS modern",
            "Was ist ein Kennzeichen eines modernen TLS-Ablaufs?",
            2,
            ["Der Browser sendet das Passwort unverschlüsselt an Port 80.", "Der Pre-Master-Secret wird ausschließlich mit dem statischen RSA-Schlüssel des Servers entschlüsselt; (EC)DHE ist unüblich.", "Zertifikat prüfen, Schlüsselvereinbarung z. B. per (EC)DHE, anschließend symmetrische Session-Verschlüsselung.", "Hashing von Passwörtern ersetzt TLS vollständig."],
            ["HTTP klartext.", "Historisches RSA-Key-Transport-Bild, nicht der empfohlene moderne Normalfall.", "Richtig.", "Andere Schicht/Problem."]);
        list[^1].ExamPart = ExamPart.Teil2;
        list[^1].TopicId = "t2-tls";
        list[^1].TechnicalStandardVersion = "TLS 1.3";

        return list;
    }
}

internal static class ContentFixes
{
    public static void TimeSensitiveFix(this LearningContent c)
    {
        c.TimeSensitiveNote();
    }

    private static void TimeSensitiveNote(this LearningContent c) => c.TagsCsv = string.IsNullOrEmpty(c.TagsCsv) ? "zeitabhaengig" : c.TagsCsv + ",zeitabhaengig";
}
