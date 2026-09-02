# Design reference audit

Inspected: 2026-09-02. Sources live in `DesignReferences/` (copies of the ChatGPT mockups supplied in chat). The mockups are **visual references**, not assets to embed.

Existing product: `.NET 8 WPF` `Ap2Master.App`, 17 real sections, MVVM, SQLite, localization de/ru/uk/en/es. There is **no** Lexikon, Notizen, Formelsammlung or Favoriten screen in the codebase. Those mockup items are **not** implemented as fake navigation.

## Inventory

| File | Screen | Theme |
| --- | --- | --- |
| `01-dashboard.jpg` | Dashboard / Übersicht | Dark navy, card grid, analytics |
| `02-sql-labor.jpg` | SQL-Labor | Dark IDE, three panes, editor + grid |
| `03-exam-wiso.jpg` | WiSo-Probeklausur | Dark exam, timer, option cards, navigator |
| `04-exam-wiso-alt.jpg` | Same exam, alternate crop | Same language as 03 |

No Swiss/Bauhaus poster, no light weekly calendar, and no dedicated flashcard mockup were supplied. Those ideas from the written brief are taken only where they do not fight the four images.

## Shared language (all four)

| Token | Observation | WPF mapping |
| --- | --- | --- |
| Background | Deep navy / near-black, not pure black | `#0B0F19` |
| Surfaces | Layered cards one step lighter than canvas | `#161B2C` / `#1D2437` |
| Primary | Cobalt / electric blue for current + primary CTA | `#3B82F6` |
| Success | Teal / emerald, always with a check or label | `#22C55E` |
| Warning | Amber (countdown, due) | `#F59E0B` |
| Danger | Red only for submit / destructive | `#DC2626` |
| Secondary category | Violet for marked-for-review and “next” cards | `#8B5CF6` |
| Radius | 8–14 px on cards, 8 px on buttons, 16 px on chips | 6 / 10 / 14 / 20 |
| Type | Clean sans, high contrast white / muted grey | Segoe UI Variable |
| Density | Information-dense, little empty theatre | 8/12/16/24 padding |
| Nav | Icon + text, active = left bar + soft blue fill | `NavButton` template |
| Header | Search, exam countdown, bell, avatar initials | Shell chrome |

## Per-screen notes

### Dashboard (`01-dashboard.jpg`)

**Strengths:** Clear greeting + countdown; large readiness ring; “Heute lernen” as the work card; Teil 1 / Teil 2 / WiSo as equal mini-cards; due cards + streak; weekly bars; topic heatmap; next-step card with tags.

**Weaknesses:** Fake learner “Louis”, fake 78/82/76/75 scores, Lexikon/Notizen/Erfolge in the rail, neon glow that will look cheap if copied as `DropShadowEffect` everywhere.

**Keep:** Card grid, ring, three exam-part cards, today list, heatmap, next recommendation, `Training starten`.

**Do not copy:** Invented percentages. Bind real `ReadinessBreakdown`, answers, planned activities, study sessions.

### SQL-Labor (`02-sql-labor.jpg`)

**Strengths:** Three-pane desktop lab; schema tree; `.sql` tabs; AvalonEdit-like editor; result grid; computed 14/14 criteria list; read-only shield.

**Weaknesses:** “Notizen / Favoriten” in the rail; glowing chrome; 14/14 must never be hardcoded.

**Keep for a later phase:** Schema tree, tabs, DataGrid, task panel, evaluator. Shell/theme from this image is used now.

### Exam (`03` / `04`)

**Strengths:** Distraction-free question card; option rows with selected border; timer as a number not a spinning animation; 1…n navigator with **shape + color** (check, bookmark, fill); footer Zurück / Weiter / red Abgeben.

**Weaknesses:** “Max Schulz”, Strg+E vs Strg+K inconsistency across mockups, exam-mode lock footer that does not exist as a product mode today.

**Keep for a later phase:** Timer, option cards, navigator, confirmation on submit. Shell from this image is used now.

## Conflicts to resolve (one product)

| Conflict | Decision |
| --- | --- |
| Mockup nav invents Lexikon / Notizen / Favoriten / Erfolge | Keep the **17 real sections**, grouped (Übersicht / Lernen / Labs / Prüfung / System) |
| Search shortcut Strg+K vs Strg+E vs Strg+F | **Strg+K** (dashboard mockup + Windows convention). Placeholder mentions the shortcut |
| Heavy glow / glass | 1 px border + 8 % accent fill. No blur, no looping glow |
| Light calendar reference (text brief, no image) | Not a third visual family. Light theme is a **token inversion** of this dark system |
| Emoji in the current sidebar | Replace with Segoe MDL2 / Fluent glyphs |

## Realistic in WPF

Yes: `WindowChrome`, `Grid`/`WrapPanel` cards, `Path` ring, `ItemsControl` heatmap, LiveCharts or bar `ItemsControl` for weekly minutes, AvalonEdit, `DataGrid` virtualization, `UniformGrid` exam navigator, `ControlTemplate` buttons.

No: CSS glassmorphism, web hover parallax, embedding the JPEG mockups.

## Chosen direction

**AP2 Focus Design System** — dark navy desktop trainer, cobalt primary, teal success, amber warning, red danger, violet only for secondary category (WiSo / marked / next). Information hierarchy from the dashboard mockup; shell chrome from SQL + exam mockups; no collage of extra products.
