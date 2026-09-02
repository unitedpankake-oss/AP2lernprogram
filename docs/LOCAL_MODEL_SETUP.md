# Local model setup

## Directory

Default: `C:\Users\ivan.ho\source\repos\AP2lernprogram\Models` (änderbar unter Einstellungen → KI-Assistent).  
Älterer Pfad `C:\Users\ivan.ho\Work\Models` wird beim Start auf den Repository-Ordner umgestellt, falls er noch gespeichert ist.

Die Anwendung **scannt rekursiv**, verändert Originaldateien aber nicht.

## Was aktuell funktioniert

1. **Ohne Chatmodell (Ist-Zustand)**  
   Assistent nutzt AP2-FTS5-Auszüge. YOLO/ONNX/`.engine`/`.pt`/`.pth` werden **nicht** als Chatbot geladen.

2. **Ollama (empfohlen, lokal)**  
   - Ollama installieren und starten (`http://127.0.0.1:11434`).  
   - Modell **selbst** ziehen, die App lädt nichts herunter:  
     `ollama pull qwen2.5:7b-instruct`  
     oder leicht: `ollama pull qwen2.5:3b-instruct`  
   - In den Einstellungen den Modellnamen eintragen und **Modell testen**.

3. **GGUF später**  
   Legen Sie eine Instruct-GGUF-Datei in den Modellordner. Der Katalog wählt sie als Primärmodell (Runtime LLamaSharp). Die aktuelle Build enthält noch **keinen** LLamaSharp-NuGet-Laufzeitkern — Validierung erkennt GGUF, das Laden verweist auf Ollama, bis ein GGUF-Runtime-Paket ergänzt wird.

4. **Generatives ONNX**  
   Nur mit `tokenizer.json` / `genai_config.json` im selben Ordner. Die vorhandenen Detektor-ONNX-Dateien werden abgelehnt.

## OCR / Screenshots

CRAFT (`craft_mlt_25k.pth`) und `latin_g2.pth` sind EasyOCR-Gewichte (Pickle). Sie werden **nicht** deserialisiert. Für OCR brauchen Sie:

- ein **ONNX-OCR**-Modell, oder
- ein **multimodales GGUF** (z. B. LLaVA)

Dann können Zwischenablage, Datei und Drag&Drop aktiviert werden. Screenshots werden nicht dauerhaft gespeichert, außer der Lernende speichert sie.

## Hardware

~32 GB RAM reichen für 3B–7B Instruct (Q4) auf CPU; GPU beschleunigt Ollama, wenn vorhanden. TensorRT-`.engine` ist an die ursprüngliche GPU/TRT-Version gebunden und wird ignoriert.
