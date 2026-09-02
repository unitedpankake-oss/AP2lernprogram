# AP2 KI-Assistent

Der **AP2 KI-Assistent** ist ein Modul der bestehenden WPF-Anwendung *AP2 Master – FIAE Prüfungstrainer*. Es gibt keine zweite App.

## Was der Assistent kann

- schwierige AP2-Themen erklären und vereinfachen
- Lösungen schrittweise erläutern
- progressive Hinweise (Denkanstoß → Vorgehen → Teillösung → Musterlösung)
- Deutsch ↔ Russisch / Englisch, Fachbegriffe und IHK-Operatoren
- Antworten analysieren, nachdem die **deterministischen** Auswerter gelaufen sind
- ähnliche Aufgaben und Lernkarten vorschlagen
- Themen zusammenfassen
- Quellen aus dem lokalen AP2-Wissensbestand anzeigen

## Modelle (Stand 02.09.2026)

Im Ordner `C:\Users\ivan.ho\source\repos\AP2lernprogram\Models` liegt **kein** generatives Chatmodell. Vorhanden sind YOLO/OBB-Detektoren und EasyOCR-Gewichte. Details: `docs/LOCAL_AI_MODEL_AUDIT.md`.

| Rolle | Auswahl |
| --- | --- |
| Primär (Chat) | Keines aus dem Ordner. Optional **lokales Ollama** `qwen2.5:7b-instruct` (leicht: `qwen2.5:3b-instruct`) |
| Zweitmodell | Keines (kein Embedding-Modell). Suche über SQLite FTS5 |
| OCR | Deaktiviert — `.pth` wird nicht unsicher geladen |

Ohne LLM antwortet der Assistent **extraktiv** mit AP2-Quellenauszügen. Es werden keine Gesetze erfunden.

## Bedienung

Tastenkürzel: **Strg+Umschalt+A**. Sidebar-Button **KI-Assistent**.

Das Panel folgt dem aktuell geöffneten Bereich (Katalog, Lernkarten, Aufgaben, SQL-Labor, Pseudocode, UML, WiSo, Probeklausur, Fehlerbuch, Coverage, Netzplan, …).

## Prüfungsmodus

Die Einschränkung gilt in der Anwendungslogik, nicht nur im Prompt:

- **Streng** — Assistent bis zur Abgabe aus
- **Sprachhilfe** — nur Übersetzung und Fachbegriffe
- **Barrierefrei** — Vereinfachung/Sprache, keine Lösungshinweise
- Nach Abgabe — volle Erklärungen

Einstellung: **Einstellungen → KI-Assistent**.

## Architektur

MVVM + DI. Keine Inferenz in Code-behind.

- `IAiAssistantService` / `AiAssistantService`
- `ILocalLanguageModelProvider` (Ollama, LLamaSharp-Stub für künftige GGUF, ONNX-GenAI-Ablehnung von Detektoren, Fake in Tests)
- `IKnowledgeSearchService` (FTS5 + LIKE)
- `IModelCatalogService` (rekursiver Scan, niemals Detektoren als Chat)
- `IAssistantContextBroker`

## Konfiguration (SQLite, nicht hardcodierte Pfade)

Standardwerte entsprechen:

```json
{
  "AiAssistant": {
    "Enabled": true,
    "ModelsDirectory": "C:\\Users\\ivan.ho\\source\\repos\\AP2lernprogram\\Models",
    "PrimaryModelPath": null,
    "SecondaryModelPath": null,
    "Runtime": "Auto",
    "PreferredLanguage": "de",
    "Temperature": 0.2,
    "MaxContextTokens": 8192,
    "UseGpuWhenAvailable": true
  }
}
```

Der Modellordner steht nur in `Ap2Master.Domain.AiDefaults.ModelsDirectory` und in der gespeicherten `AiProviderConfiguration`.
