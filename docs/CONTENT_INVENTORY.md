# Content inventory

Extracted: 2026-09-02 from the two DOCX packages. Raw text: `docs/extracted/`.

## Physical source documents

| Document | Path used | Original filename in repo root | Size | Status |
| --- | --- | --- | --- | --- |
| AP2 Intensivkurs / Vorbereitungskurs FIAE | `LearningSources/AP2_Vorbereitungskurs_FIAE.docx` | `AP2_Vorbereitungskurs_FIAE (1).docx` | 1 190 594 bytes | Copied and extracted (90 323 characters) |
| AP2 Lernzettel – 4-Tage-Sprint | `LearningSources/AP2_Lernzettel 1.docx` | `AP2_Lernzettel 1.docx` | 94 739 bytes | Copied and extracted (70 999 characters) |

The trainer does **not** embed the DOCX as a viewer. Structured learning objects live in `Ap2Master.Content`. Copies in the repository root may remain; canonical import path is `LearningSources/`.

## Source A – Vorbereitungskurs (57 Module, HelpDesk-Pro-Fallstudie)

Stand im Dokument: September 2026. Aufbau: Prüfungskern, Übung, Musterlösung. Acht-Wochen-Plan über Module 1–57.

| Modul | Extracted heading | Exam part | Normalized topic id(s) | Content type | Overlap with Lernzettel | Time-sensitive | Verified | Cards / exercises |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 01 | Stakeholder- und Umfeldanalyse | Teil 1 | `t1-stakeholder`, `t1-erwartungen`, `t1-einfluss-interesse` | Theory, Scenario | yes | no | yes | yes |
| 02 | UML-Klassendiagramm | Teil 1 | `t1-uml-klasse` | UML | yes | no | yes | yes |
| 03 | UML-Anwendungsfalldiagramm | Teil 1 | `t1-uml-usecase` | UML | yes | no | yes | yes |
| 04 | UML-Aktivitätsdiagramm | Teil 1 | `t1-uml-aktivitaet` | UML | yes | no | yes | yes |
| 05 | UML-Sequenzdiagramm | Teil 1 | `t1-uml-sequenz` | UML | yes | no | yes | yes |
| 06 | UML-Zustandsdiagramm | Teil 1 | `t1-uml-zustand` | UML | yes | no | yes | yes |
| 07 | ER-Modell | Teil 1 | `t1-er` | ERModel | yes | no | yes | yes |
| 08 | Mockup und Usability | Teil 1 | `t1-mockup`, `t1-uiux` | Scenario | yes | no | yes | yes |
| 09 | REST, HTTP und CRUD | Teil 1 | `t1-rest` | Theory, Matching | yes | no | yes | yes |
| 10 | Pseudocode | Teil 1/2 | `t2-pseudocode` | Pseudocode | yes | no | yes | yes |
| 11 | Vorgehensmodelle | Teil 1 | `t1-vorgehen`, `t1-scrum` | Theory | yes | no | yes | yes |
| 12 | Datenschutz und DSGVO | Teil 1 | `t1-dsgvo` | Theory | yes | no | yes | yes |
| 13 | Qualitätssicherung | Teil 1 | `t1-qs` | Theory | yes | no | yes | yes |
| 14 | Sicherheitstechnische Anforderungen | Teil 1 | `t1-security-anf` | Theory | yes | no | yes | yes |
| 15 | Entwurfsmuster | Teil 1 | `t1-factory`, `t1-mvc`, `t1-observer`, `t1-singleton` | Theory | yes | no | yes | yes |
| 16 | SQL SELECT JOIN/GROUP BY | Teil 2 | `t2-sql-select`, `t2-sql-join`, `t2-sql-group` | SQL | yes | no | yes | yes |
| 17 | SQL-DML | Teil 2 | `t2-sql-dml` | SQL | yes | no | yes | yes |
| 18 | Filtern, Zählen, Sortieren | Teil 2 | `t2-filter`, `t2-count`, `t2-sort` | Pseudocode | yes | no | yes | yes |
| 19 | Sortieralgorithmus | Teil 2 | `t2-algorithmen-sort` | Sorting | yes | no | yes | yes |
| 20 | Whitebox-/Blackbox-Tests | Teil 2 | `t2-blackbox`, `t2-whitebox` | Theory | yes | no | yes | yes |
| 21 | Coverage-Metriken | Teil 2 | `t2-coverage` | ControlFlow | yes | no | yes | yes |
| 22 | Pfade und Pfadüberdeckung | Teil 2 | `t2-pfadueberdeckung` | ControlFlow | yes | no | yes | yes |
| 23 | Unit- und Regressionstests | Teil 2 | `t2-unit`, `t2-regression` | Theory | yes | no | yes | yes |
| 24 | Semantische Fehler finden | Teil 2 | `t2-bugs` | BugFinding | yes | no | yes | yes |
| 25 | OOP-Begriffe | Teil 2 | `t2-oop` | Theory | yes | no | yes | yes |
| 26 | Verschlüsselung, TLS und Hashing | Teil 2 | `t2-krypto`, `t2-tls` | Theory | yes | edition-sensitive | yes (TLS modern) | yes |
| 27 | Normalisierung bis 3. NF | Teil 2 | `t2-nf` | Theory | yes | no | yes | yes |
| 28 | GRANT und Berechtigungen | Teil 2 | `t2-grant` | SQL | yes | no | yes | yes |
| 29 | Datenmigration und Datenimport | Teil 2 | `t2-migration` | Scenario | yes | no | yes | yes |
| 30 | Probezeit im Ausbildungsvertrag | WiSo | `t3-probezeit` | Theory | yes | no | yes | yes |
| 31 | Kündigung des Ausbildungsvertrags | WiSo | `t3-kuendigung-azubi` | Theory | yes | form-sensitive | yes | yes |
| 32 | Ende des Ausbildungsverhältnisses | WiSo | `t3-ende-ausbildung` | Theory | conflict | no | corrected | yes |
| 33 | Befristung von Arbeitsverträgen | WiSo | `t3-befristung` | Theory | yes | no | yes | yes |
| 34 | Tarifvertrag | WiSo | `t3-tarif` | Theory | yes | no | yes | yes |
| 35 | Betriebsrat und Mitbestimmung | WiSo | `t3-betriebsrat` | Theory | yes | no | yes | yes |
| 36 | Fünf Säulen der Sozialversicherung | WiSo | `t3-sv` | Theory | yes | rates 2026 | yes | yes |
| 37 | Beitragsbemessungsgrenze | WiSo | `t3-bbg` | Calculation | yes | **yes** | dated 2026 | yes |
| 38 | Generationenvertrag und Umlageverfahren | WiSo | `t3-umlage` | Theory | yes | no | yes | yes |
| 39 | Mutterschutz | WiSo | `t3-mutterschutz` | Theory | conflict (TOC vs body) | no | corrected 8/12 | yes |
| 40 | AGG | WiSo | `t3-agg` | Theory | yes | no | yes | yes |
| 41 | Arbeitsschutz und ASA | WiSo | `t3-arbeitsschutz` | Theory | yes | no | yes | yes |
| 42 | Sicherheits- und Rettungszeichen | WiSo | `t3-schilder` | Matching | yes | no | yes | yes |
| 43 | Brandschutz | WiSo | `t3-brandschutz` | Ordering | yes | no | yes | yes |
| 44 | Unternehmensformen | WiSo | `t3-unternehmen` | Theory | yes | no | yes | yes |
| 45 | Stamm- und Grundkapital | WiSo | `t3-mindestkapital` | Theory | yes | no | yes | yes |
| 46 | Handelsregister A und B | WiSo | `t3-hr` | Theory | yes | no | yes | yes |
| 47 | Wirtschaftssektoren | WiSo | `t3-sektoren` | Theory | yes | no | yes | yes |
| 48 | Wirtschaftlichkeit und Rentabilität | WiSo | `t1-wirtschaftlichkeit`, `t1-rentabilitaet` | Calculation | conflict in Lernzettel TOC | no | formulas verified | yes |
| 49 | Marktformen | WiSo | `t3-marktformen` | Theory | yes | no | yes | yes |
| 50 | Kartelle | WiSo | `t3-kartell` | Theory | conflict | no | GWB current | yes |
| 51 | Mahnverfahren und Verzug | WiSo | `t3-mahnverfahren` | Theory | yes | no | yes | yes |
| 52 | Kaufvertrag und Mängel | WiSo | `t3-kaufvertrag` | Theory | yes | no | yes | yes |
| 53 | Steuern | WiSo | `t3-steuern` | Theory | yes | **yes** | rates dated | yes |
| 54 | Inflation, EZB und Leitzins | WiSo | `t3-inflation`, `t3-ezb` | Theory | yes | policy-sensitive | conceptual | yes |
| 55 | Nachhaltigkeit und Verursacherprinzip | WiSo | `t3-nachhaltigkeit` | Theory | yes | no | yes | yes |
| 56 | Businessplan und Existenzgründung | WiSo | `t3-businessplan` | Theory | yes | no | yes | yes |
| 57 | Leitungssysteme | WiSo | `t3-leitung` | Theory | yes | no | yes | yes |

HelpDesk-Pro examples (tickets, REST, UML) are preserved as exercise flavour, not as unique topics.

## Source B – Lernzettel 1 (April 2026, IHK Regensburg sprint notes)

OCR-based frequency list over Sommer 2022 – Winter 2025/26. Useful for historical exam patterns; several legal/technical lines are **wrong or mixed** (see `CONTENT_CONFLICTS.md`).

| Section | Heading | Exam part | Normalized topic id(s) | Overlap | Time-sensitive | Verified | Cards / exercises |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Intro | Häufigkeitsanalyse | all | (planning weights, not content) | — | historical sample | n/a | n/a |
| 1.1 | Stakeholder & Anforderungsanalyse | Teil 1 | `t1-stakeholder` … `t1-lasten-pflichten` | yes | no | yes | yes |
| 1.2 | Vorgehensmodelle | Teil 1 | `t1-wasserfall` … `t1-devops` | yes | no | yes | yes |
| 1.3 | Projektmanagement-Werkzeuge | Teil 1 | `t1-netzplan`, `t1-gantt`, `t1-risiko` | yes | no | yes | yes |
| 1.4 | Wirtschaftlichkeit & Make-or-Buy | Teil 1 | `t1-wirtschaftlichkeit`, `t1-make-or-buy` | yes | no | body OK, TOC wrong | yes |
| 1.5.1–1.5.5 | UML five diagrams | Teil 1 | `t1-uml-*` | yes | no | yes | yes |
| 1.6–1.7 | ER, DB-Architektur | Teil 1/2 | `t1-er`, `t2-nf` | yes | no | yes | yes |
| 1.8 | REST / HTTP / CRUD | Teil 1 | `t1-rest` | yes | no | yes | yes |
| 1.9 | Mockup · UI/UX | Teil 1 | `t1-mockup` | yes | no | yes | yes |
| 1.10 | DSGVO · Sicherheit | Teil 1 | `t1-dsgvo` | yes | no | yes | yes |
| 1.11 | Architektur & Muster | Teil 1 | `t1-mvc` … `t1-di` | yes | no | yes | yes |
| 1.12 | Lizenzmodelle | Teil 1 | `t1-lizenzen` | extra vs 57 | no | yes | yes |
| 1.13 | Qualitätssicherung | Teil 1 | `t1-qs` | yes | no | yes | yes |
| 2.1 | IHK-Pseudocode | Teil 2 | `t2-pseudocode` | yes | no | yes | yes |
| 2.2 | OOP | Teil 2 | `t2-oop`, SOLID | extra vs 57 | no | yes | yes |
| 2.3 | Datenstrukturen | Teil 2 | `t2-listen` … `t2-baum` | extra vs 57 | no | yes | yes |
| 2.4 | Sortieren/Suchen | Teil 2 | `t2-algorithmen-sort`, `t2-binaersuche` | yes | no | yes | yes |
| 2.5 | O-Notation | Teil 2 | `t2-komplexitaet` | extra vs 57 | no | yes | yes |
| 2.6 | Tests / Coverage | Teil 2 | `t2-coverage` | yes | no | calculated, not hardcoded | yes |
| 2.7 | SQL-Referenz + historical IHK queries | Teil 2 | `t2-sql-*` | yes | historical examples | SQL logic yes | yes |
| 2.8 | Normalisierung | Teil 2 | `t2-nf` | yes | no | yes | yes |
| 2.9 | IT-Sicherheit / TLS / OWASP | Teil 2 | `t2-tls`, `t2-owasp` | yes | **yes** | TLS/OWASP corrected | yes |
| 2.10 | Git | Teil 2 | `t2-git` | extra vs 57 | no | yes | yes |
| 2.11 | Schnittstellen & Formate | Teil 2 | `t2-json`, `t2-xml` | extra vs 57 | no | yes | yes |
| 2.12 | Netzwerke | Teil 2 | `t2-osi`, `t2-tcpip` | extra vs 57 | no | yes | yes |
| 2.13 | Cloud | Teil 2 | `t2-cloud` | extra vs 57 | no | yes | yes |
| 3.1 | BBiG / JArbSchG | WiSo | `t3-bbig`, `t3-textform`, `t3-jarbschg` | yes | **yes** | Textform corrected | yes |
| 3.2 | Arbeitsrecht Erwachsene | WiSo | `t3-kuendigung`, `t3-kschg` | yes | no | yes | yes |
| 3.3 | Tarif / Mitbestimmung | WiSo | `t3-tarif`, `t3-betriebsrat` | yes | no | yes | yes |
| 3.4 | Sozialversicherung / BBG | WiSo | `t3-sv`, `t3-bbg` | yes | **yes** | 2026 figures | yes |
| 3.5–3.7 | Arbeitsschutz, AGG, Mutterschutz | WiSo | `t3-arbeitsschutz`, `t3-agg`, `t3-mutterschutz` | yes | TOC vs body | 8/12 weeks | yes |
| 3.8–3.11 | Wirtschaftsordnung, Unternehmen, HR, Organisation | WiSo | `t3-kartell`, `t3-unternehmen`, `t3-hr`, `t3-leitung` | yes | Kartell conflict | GWB current | yes |
| 3.12–3.13 | Kaufvertrag, Gerichte | WiSo | `t3-kaufvertrag`, `t3-gerichte` | yes | **yes** AG threshold | 10.000 € | yes |
| 3.14–3.18 | Steuern, EZB, Nachhaltigkeit, Businessplan, Kennzahlen | WiSo | matching `t3-*` / `t1-*` | yes | tax/policy | formulas verified | yes |
| Cheatsheets | SQL, UML, SV, Unternehmensformen | mixed | CheatSheet content type | yes | mixed | quarantined if stale | yes |

## Import rule

Neither file was blindly copied into exam answers. Where they overlap, the Anwendung uses the priority in `CONTENT_CONFLICTS.md`. Historical IHK SQL/UML examples stay as **historical practice items**, not as current legal keys.
