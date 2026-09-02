# Implementation status

Truthful snapshot (2026-09-02).

## Verified in this iteration

- `dotnet build Ap2Master.sln` / `dotnet test Ap2Master.sln` (KI-Assistent integriert)
- Solution `Ap2Master.sln` (.NET 8) with the requested project layout
- Domain algorithms: SM-2, readiness weights, coverage union, CPM buffers, SQL multiset compare, formula library
- Full normalized topic tree (Teil 1–3, not limited to 57 modules) with theory, flashcards and MC per topic
- Documented inventory/conflicts/legal audit; DOCX in `LearningSources/`
- Corrected facts: Textform, Kündigung vs Vertrag, Bekanntgabe, Mutterschutz 8/12, formulas, AG 10.000 €, GWB, TLS, OWASP edition, 2026 BBG/JAEG/Mindestlohn
- SQLite seed + progress persistence, backup zip, DPAPI secrets
- WPF shell, 17 German navigation targets, light/dark/high-contrast, focus, font scale, confirm reset
- Vertical slice: topic → theory → flashcards (SM-2) → generated exercise → Fehlerbuch → dashboard
- SQL lab with functional compare, preview flag, reset, EXPLAIN QUERY PLAN, AvalonEdit SQL editor
- Pseudocode interpreter with trace; UML ASCII preview; network plan; coverage calculator
- WiSo 30/60 + Teil1/Teil2/day exams with autosave and restore
- **AP2 KI-Assistent** in the same app: collapsible panel, German UI, FTS5 RAG, exam guards, Ollama fallback, extractive mode without a local LLM
- Model audit: no chat model in `C:\Users\ivan.ho\source\repos\AP2lernprogram\Models` (YOLO/OCR only); Ollama `qwen2.5:7b-instruct` optional; no unsafe `.pth` load
- Automated tests including catalog scan, chat-rejection of detectors, translation/SQL preserve, exam restrictions, persistence

## Still incomplete / honest gaps

- No generative GGUF/ONNX-GenAI in the models folder — chat needs local Ollama or a later GGUF + LLamaSharp package
- LLamaSharp runtime is a stub until an Instruct-GGUF is present
- OCR UI disabled (CRAFT/`latin_g2` are pickle `.pth`)
- Structured UML/ER editor with live ASCII preview, not a drag-and-drop canvas
- `EnsureCreated` plus `CREATE TABLE IF NOT EXISTS` for AI tables (no checked-in EF migrations)
- LiveCharts OpenTK/SkiaSharp NU1701 compatibility warnings (runtime WPF charts still build)
- Optional cloud AI remains a null provider (`ILearningContentAiProvider`)

The application is a working trainer. Prüfungsreife is a training indicator, not a pass guarantee. The assistant does not invent statutes or page numbers.

