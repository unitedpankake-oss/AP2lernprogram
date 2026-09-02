# Coverage matrix

Generated from `TopicCatalog` + both DOCX extracts (2026-09-02). Every normalized topic still has theory, at least three Lernkarten and a single-choice item (`CatalogTests`).

The 57-module Intensivkurs is the **core spine**. The Lernzettel adds Git, Netze, Cloud, Lizenzen, JArbSchG, Komplexität and data structures. `TopicCatalog` is **larger than 57** and covers the product syllabus (Teil 1–3) completely.

| Exam part | Source coverage | App topics beyond the 57 modules | Flashcards | Exercises |
| --- | --- | --- | --- | --- |
| Teil 1 | Modules 01–15 + Lernzettel 1.1–1.13 | Lasten/Pflichten, FAZ/FEZ, NPV, Accessibility, Microservices, CI/CD, DoD | yes | MC + UML structured editor + Netzplan |
| Teil 2 | Modules 16–29 + Lernzettel 2.1–2.13 | SOLID, all listed sorts, OSI/TCP, Git, SQL injection/XSS/CSRF labs | yes | SQL-Labor, Pseudocode-Engine, Coverage calculator |
| WiSo | Modules 30–57 + Lernzettel 3.1–3.18 | Textform, JArbSchG, Elterngeld basics, GWB exemptions, AG 10.000 € | yes | 30/60 engine + question bank |

## 57-module → topic mapping

See `CONTENT_INVENTORY.md` for one row per module. Priority clusters (scheduled earlier in the Lernplan): SQL, Algorithmen, UML, Testing, WiSo.

## Completeness

| Check | Result |
| --- | --- |
| Both DOCX extracted | yes (`docs/extracted/`) |
| All Intensivkurs modules 01–57 inventoried | yes |
| All Lernzettel numbered sections 1.1–3.18 inventoried | yes |
| Every `TopicCatalog` id has theory + cards + MC | yes (automated) |
| Conflicts documented and not used as current exam keys | yes |

SQL, algorithms, UML, testing and WiSo remain `IsPriority`.
