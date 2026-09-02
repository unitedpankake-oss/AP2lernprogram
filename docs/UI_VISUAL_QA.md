# UI visual QA

Pass against `DesignReferences/` mockups. Do not mark a screen finished without opening it in the running app.

## Shell

- [ ] Dark navy canvas, cobalt active nav, icon + text
- [ ] Collapsed sidebar: icons + tooltips, width ~72
- [ ] Search Strg+K finds sections and topics
- [ ] Countdown uses the saved exam date (or “Kein Prüfungstermin”)
- [ ] Bell lists only real due cards / missed plan / mistakes / open exam
- [ ] Profile chip shows `UserSettings.DisplayName`, not “Max Schulz”
- [ ] Light and HighContrast apply without restart
- [ ] 1280×720: content scrolls, primary actions remain visible
- [ ] 1920×1080: dashboard grid uses three columns
- [ ] 150 % scaling: no clipped caption buttons

## Dashboard

- [ ] Greeting uses the learner name
- [ ] Ring value equals `ReadinessBreakdown.Score`
- [ ] Teil cards show 0 % when there are no answers (not mockup 82 %)
- [ ] Weekly bars match study sessions
- [ ] Heatmap cells use mastery; empty stays dark with caption
- [ ] `Training starten` opens a real section

## Later screens

- [ ] SQL-Labor: Schema-Tree, Tabs, DataGrid-Ergebnis, Punkte aus Evaluator
- [ ] Probeklausur / WiSo: Timer, Navigator 1…n, markierte Aufgaben
- [ ] Lernkarten: Flip-Animation (aus bei Reduced Motion)
- [ ] Lernplan: Woche, Drag-and-Drop, Tastatur ← / →
