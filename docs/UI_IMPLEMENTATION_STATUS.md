# UI implementation status

Updated: 2026-09-02. Design system: **AP2 Focus**. References: `docs/DESIGN_REFERENCE_AUDIT.md`, `docs/DESIGN_SYSTEM.md`.

## Done this phase (заход 2)

| Area | Status | Notes |
| --- | --- | --- |
| SQL-Labor IDE | Done | Schema-TreeView (PRAGMA), `.sql`-Tabs, AvalonEdit, virtualized DataGrid, Plan/Vergleich-Tabs, Aufgabenpanel mit `SqlTaskEvaluator` (berechnete Punkte), Stoppuhr, Abbruch |
| Exam timer + 1…n | Done | Große Restzeit, Pills, Navigatorzellen (aktuell / beantwortet / markiert / offen = Farbe + Glyphe), OptionCards mit Buchstabe im Tag, Abgabe über `IDialogService` |
| Lernkarten-Flip | Done | ScaleX 90+90 ms, übersprungen bei `ReducedMotion` |
| Lernplan DND | Done | Wochenraster Mo–So, `MoveAsync`, Ziehen zwischen Tagen, ← / → |
| Remaining screens | Restyled | Aufgaben, Pseudocode, UML, Projekt, Coverage, Netz, Fehlerbuch, Statistik, Inhalte, Einstellungen: H1 + Card + Ghost/Danger |

## Still later

Visual QA screenshots at 1280×720 / 1920×1080 / 150 % scaling. WPF cannot be verified in a browser.

## Visual QA

After `dotnet run --project src/Ap2Master.App`: SQL-Labor (Schema aufklappen, Ausführen, Lösung prüfen), WiSo/Probeklausur (Navigator, Markieren, Timer), Lernkarten (Flip), Lernplan (Karte ziehen).
