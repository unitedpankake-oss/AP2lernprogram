# KI-Assistent – Fehlerbehebung

| Status / Symptom | Bedeutung | Maßnahme |
| --- | --- | --- |
| Modelle werden gesucht … | Katalog scannt den Modellordner | Warten; Ordner unter Einstellungen prüfen |
| Modell wird geprüft … | Datei/Ollama wird validiert | Bei Timeout Ollama-Status prüfen |
| Modell wird geladen … | Gewichte oder Ollama-Session | Bei OOM kleineres Modell |
| KI-Assistent ist bereit | Chatmodell geladen | Generieren möglich |
| Tokenizer fehlt | ONNX ohne Tokenizer | Kein Chat; GenAI-Bundle oder GGUF/Ollama |
| Format wird nicht unterstützt | Detektor/Engine/Pickle | Datei ist kein LLM |
| Nicht genügend Arbeitsspeicher | OOM | 3B-Quant, CPU, andere Apps schließen |
| GPU-Laden fehlgeschlagen – CPU wird versucht | GPU-Pfad gescheitert | CPU/Ollama |
| Kein geeignetes Sprachmodell gefunden | Kein GGUF/Chat-ONNX, Ollama offline | Extraktive Hinweise; optional Ollama starten |
| Strenger Prüfungsmodus | Assistent gesperrt | Nach Abgabe oder Modus in Einstellungen |
| OCR deaktiviert | Nur `.pth` vorhanden | ONNX-OCR oder multimodales GGUF |
| Ollama ist nicht erreichbar | Port 11434 zu | `ollama serve`, nur localhost |
| Antworten ohne Quellen | Kein FTS-Treffer | Themenname in der Frage nennen |
| UI hängt nicht, Abbrechen geht nicht | Request schon beendet | Neu senden; bei Ollama Prozess prüfen |

## Build / Tests

```powershell
dotnet build Ap2Master.sln
dotnet test Ap2Master.sln
```

Optionale echte Modell-Tests sind mit `[Trait("Category","Integration")]` markiert und standardmäßig übersprungen.

## Datenbank

Ältere `ap2master.db` ohne KI-Tabellen: Beim Start legt `AiSchema.EnsureAsync` die Tabellen mit `CREATE TABLE IF NOT EXISTS` an. Nach **Alles zurücksetzen** die App neu starten, damit der Katalog erneut gesät wird.
