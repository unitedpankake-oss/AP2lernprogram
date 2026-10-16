# AP2 Master – FIAE Prüfungstrainer

Windows-Desktopanwendung zur systematischen Vorbereitung auf die IHK-Abschlussprüfung Teil 2 (AP2) der Fachinformatiker/-innen für Anwendungsentwicklung. Die Prüfungsreife-Kennzahl ist ein Trainingsindikator und **kein Bestehensversprechen**.

## Voraussetzungen

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Optional: WebView2-Runtime (nur falls später Diagramme im Browser-Host gerendert werden)

## Bauen, testen, starten

Im Repository-Stamm:

```powershell
dotnet restore Ap2Master.sln
dotnet build Ap2Master.sln
dotnet test Ap2Master.sln
dotnet run --project src/Ap2Master.App
```

Kurzform analog zur Vorgabe:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Ap2Master.App
```

Lernfortschritt und Katalog liegen in MongoDB Atlas, Datenbank `ap2master`. Lange Lehrtexte werden dort einmal als Brotli-Blob gespeichert und in den Aufgaben nur per Hash referenziert; beim Lesen dekodiert die App sie zurück. Das Helpdesk-SQL-Labor bleibt eine lokale SQLite-Datei unter `%LocalAppData%\Ap2Master\ap2master\helpdesk.db`, weil dort SQL-Abfragen ausgeführt werden.

## AP2 KI-Assistent

Integriertes Panel in derselben WPF-App (Strg+Umschalt+A). Standard: lokale Quellenauszüge, optional lokales Ollama. **Kein** Download großer Modelle durch die Anwendung.

- Bedienung und Architektur: `docs/AI_ASSISTANT.md`
- Modell-Audit des Ordners `C:\Users\ivan.ho\source\repos\AP2lernprogram\Models`: `docs/LOCAL_AI_MODEL_AUDIT.md`
- Einrichtung: `docs/LOCAL_MODEL_SETUP.md`
- Datenschutz: `docs/AI_PRIVACY.md`
- Fehlerbilder: `docs/AI_TROUBLESHOOTING.md`

Im gescannten Modellordner liegen aktuell nur Objektdetektoren und EasyOCR-Pickles — kein Chat-GGUF. Der Assistent bleibt nutzbar (FTS5 + deterministische Auswerter).

## Lernquellen

Kanonischer Ort:

- `LearningSources/AP2_Vorbereitungskurs_FIAE.docx`
- `LearningSources/AP2_Lernzettel 1.docx`

Die Dateien aus dem Repository-Stamm (`AP2_Vorbereitungskurs_FIAE (1).docx`, `AP2_Lernzettel 1.docx`) wurden dorthin kopiert und nach `docs/extracted/` ausgelesen. Inventur und Konflikte: `docs/CONTENT_INVENTORY.md`, `docs/CONTENT_CONFLICTS.md`.

## Architekturkurzüberblick

Siehe `docs/ARCHITECTURE.md`. Kern: WPF/MVVM, SQLite/EF Core, Offline-Zufallsgenerator, SM-2-Lernkarten, funktionaler SQL-Vergleich, berechnete Coverage, WiSo 30/60.

## Lizenz

Lerninhalte sind urheberrechtlich zu beachten. Amtliche Gesetzestexte unterliegen eigenen Nutzungsbedingungen. Der Programmcode in diesem Repository dient Ausbildungs- und Übungszwecken.
