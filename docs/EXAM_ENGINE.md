# Exam engine

- WiSo: exactly 30 items, 60 minutes, free navigation, mark, autosave every 15 s, submit on timeout, review after submit, mistakes → Fehlerbuch.
- Teil 1 / Teil 2: 90 minutes, hide hints, restore open sessions by `ExamKind`.
- Training day concatenates WiSo + Teil1 + Teil2 picks.
- Integrity: single-choice exactly one correct option; historical/quarantined content excluded.
- Scoring compares options/text; time expected vs actual is stored on `ExamResult`.
- Printable report text is `ExamResult.ReportText` (copy from Auswertung).
