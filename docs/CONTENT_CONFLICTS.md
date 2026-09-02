# Content conflicts

Priority used: (1) current legislation (2) IHK framework (3) current technical standards (4) `AP2_Vorbereitungskurs_FIAE.docx` (5) `AP2_Lernzettel 1.docx`.

Extracted 2026-09-02. Obsolete lines stay available as **historical** explanations; they are never the current correct exam answer.

| Topic | Source | Extracted claim | Applied in AP2 Master | Resolution |
| --- | --- | --- | --- | --- |
| Textform § 11 BBiG | Lernzettel 3.1 | „Schriftform vorgeschrieben (BBiG § 11).“ | Wesentliche Inhalte seit 1.8.2024 in **Textform** (§ 126b BGB); Empfang nachweisen. | Legislation (BVaDiG) beats Lernzettel |
| Kündigung vs Vertrag | Vorbereitungskurs Modul 31 / Lernzettel 3.1 | Kündigung „schriftlich“; Vertrag § 11 Schriftform vermischt | **Kündigung** nach § 22 BBiG hat eigene Form; nicht mit der Vertragsabfassung nach § 11 gleichsetzen. Probezeit-Kündigung im Kurs: schriftlich, elektronische Form reicht nicht – das betrifft die **Kündigung**, nicht den Vertragstext. | Legislation; keep Kündigung Schriftform, drop Vertrag-only-Schriftform |
| Ende der Ausbildung | Lernzettel 3.1 | endet „am Tag der Prüfung“ | endet mit **Bekanntgabe** durch den Prüfungsausschuss (§ 21 Abs. 2 BBiG) | Vorbereitungskurs Modul 32 already correct; Lernzettel marked historical |
| Mutterschutz | Lernzettel Häufigkeitsliste | „8 Wochen nach, **14** bei Mehrlingen/Frühgeb.“ | grundsätzlich **8** Wochen nach Geburt, **12** bei Früh- und Mehrlingsgeburt | Vorbereitungskurs Modul 39 already corrects 14→12; TOC of Lernzettel is wrong, body 3.7 is correct |
| Wirtschaftlichkeit | Lernzettel Häufigkeitsliste | „Ertrag / Aufwand **× 100**“ als Sammelformel mit Rentabilität | Wirtschaftlichkeit = Ertrag / Aufwand; Rentabilität = Gewinn / Kapital × 100; Produktivität = Ausbringung / Einsatz | Vorbereitungskurs Modul 48 and Lernzettel 1.4/3.18 body are correct; TOC mix is historical |
| Amtsgericht | Lernzettel 3.13 | Zivilsachen bis **5.000 €** | allgemeine Wertgrenze **10.000 €** ab 1.1.2026 (besondere Zuständigkeiten unverändert) | Legislation; 5.000 € labelled historical (pre-2026 exams) |
| Kartell | Lernzettel 3.8 + Last-Minute | „Normen-/Typenkartell ist **anmeldepflichtig** — die anderen sind generell verboten“ / „= anmeldepflichtig (≠ verboten)“ | allgemeines Kartellverbot; Freistellung nach § 2 GWB möglich, kein aktueller Universal-Freifahrtschein durch Anmeldung | Vorbereitungskurs Modul 50 is closer (Wettbewerbsbeschränkung + Freistellung); Lernzettel WiSo-Falle 24 Sommer kept as **historical exam item** |
| TLS | Lernzettel 2.9 | RSA-only: Client erzeugt Sitzungsschlüssel, verschlüsselt mit Server-Public-Key | Zertifikat, Authentisierung, Schlüsselvereinbarung z. B. **(EC)DHE**, dann symmetrische Sitzung | Vorbereitungskurs Modul 26 already modern; Lernzettel flow = historical IHK simplification |
| OWASP | Lernzettel 2.9 | mix of 2017 names (Broken Authentication, Sensitive Data Exposure) without edition | always label edition (e.g. Top 10:2021 A01 Broken Access Control) | Standard |
| BBG / JAEG / Sätze | both mention BBG conceptually | no complete 2026 table in Lernzettel | 2026 values with `ValidFrom`/`ValidUntil`/`VerifiedAt` | Ordinance 2026 |

## What was kept from the DOCX files

- HelpDesk-Pro case study flavour (Vorbereitungskurs).
- Historical IHK SQL snippets (Aktienkurs, Patient) as **practice traces**, not schema of the embedded SQL lab.
- Frequency ranking in the Lernzettel (weights the adaptive plan toward SQL, UML, Coverage, WiSo).
- Correct body text in Vorbereitungskurs for Bekanntgabe, Mutterschutz 8/12, Wirtschaftlichkeit/Rentabilität, modern TLS.

## Unresolved / flagged

No remaining *unknown* legal numbers were invented. Contribution rates, BBG, JAEG and Mindestlohn stay on the 2026 audit row in `LEGAL_FACTS_AUDIT.md`. If a new ordinance appears before an exam date, mark `NeedsReviewBeforePublish`.
