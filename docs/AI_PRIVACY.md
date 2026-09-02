# KI-Datenschutz

Alle Inferenzschritte sind **standardmäßig lokal**.

| Regel | Umsetzung |
| --- | --- |
| Keine Prompt-Uploads | Kein Cloud-HTTP. Ollama nur `127.0.0.1`. |
| Keine Telemetrie | Es gibt keinen Telemetrie-Client. |
| Keine API-Schlüssel im Quelltext | Optionale Geheimnisse nur über DPAPI (`ISecureSecretStore`), nicht für den Assistenten benötigt. |
| Unterhaltungen | SQLite `%LocalAppData%\Ap2Master\ap2master.db`, löschbar unter Einstellungen. |
| Logs | Keine vollständigen Prompts in gewöhnlichen Logs. |
| Modelldateien | Scan nur lesend; keine Konvertierung, kein unsicheres Unpickle. |
| Nicht-lokal | Wird nicht angeboten. Ollama-URL ist localhost. Warnung in den Einstellungen. |

Lernmaterial, OCR-Text, SQL und Code stehen in `<daten>` und gelten **nicht** als Systemanweisungen (Prompt-Injection-Abwehr).

Es werden kein verstecktes Chain-of-Thought und keine vollständigen Systemprompts in der Historie gespeichert — nur sichtbare Nachrichten, Quellen, Hinweisstufe, Modellname und Dauer.
