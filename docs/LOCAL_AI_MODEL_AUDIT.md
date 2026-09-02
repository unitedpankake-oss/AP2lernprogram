# Local AI model audit

Scanned: `C:\Users\ivan.ho\source\repos\AP2lernprogram\Models` (recursive, 2026-09-02).  
Previously assumed path `C:\Users\ivan.ho\Work\Models` is treated as a **legacy** location only.  
Host: ~32 GB RAM, 22 logical processors. Ollama was **not** installed at scan time.  
Original files were **not** modified, moved, renamed, converted or deserialized.

## Selection

| Role | Choice | Why |
| --- | --- | --- |
| **Primary chat** | **None from this folder.** Runtime: optional **Ollama** `qwen2.5:7b-instruct` (lightweight `qwen2.5:3b-instruct`). | No GGUF, no Hugging Face chat tree, no GenAI ONNX, no tokenizer. Every file is object detection, TensorRT, or EasyOCR weights. |
| **Second model** | **Not selected.** | No embedding GGUF/ONNX. CRAFT/`latin_g2` are OCR, not chat or embeddings. `.pth` is not loaded (pickle). |

Do **not** download 7B/3B weights without confirmation. The app talks to a **local** Ollama HTTP endpoint (`http://127.0.0.1:11434`) only if the user enables it and Ollama is running.

Default models directory in code: `Ap2Master.Domain.AiDefaults.ModelsDirectory` → `C:\Users\ivan.ho\source\repos\AP2lernprogram\Models`. Existing databases that still pointed at `Work\Models` are migrated to this folder on startup.

## Inventory

23 files: 7× `.engine`, 7× `.onnx` (~80 MB, graph strings `images`/`output0`/`obb`), 7× `.pt`, 2× `.pth`. No chat artefacts.

| Path | Format | Architecture (inferred) | Intended task | Languages | Context | Runtime | CPU | GPU | Est. memory | Chat? | Selected | Reason |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `Models\abb-st2.engine` | TensorRT `.engine` | YOLO-like stage-2 detector (ABB) | Industrial object detection | n/a | n/a | NVIDIA TensorRT (engine-specific) | no | specific TRT/GPU | ~1–2 GB VRAM | **no** | no | Not a language model; engine is GPU/TRT-locked |
| `Models\abb-st2.onnx` | ONNX (~80 MB) | YOLO-like; graph strings `images`/`output0`/`obb` | Object detection | n/a | n/a | ONNX Runtime | yes (slow) | CUDA EP | ~1 GB | **no** | no | No tokenizer / `genai_config.json`; detection heads |
| `Models\abb-st2.pt` | PyTorch pickle | YOLO weights | Object detection | n/a | n/a | PyTorch | yes | CUDA | ~1 GB | **no** | no | Unsafe pickle; not chat |
| `Models\eaton-st2.engine` | TensorRT | YOLO-like (Eaton) | Object detection | n/a | n/a | TensorRT | no | specific | ~1–2 GB | no | no | Same as ABB engine |
| `Models\eaton-st2.onnx` | ONNX | YOLO-like | Object detection | n/a | n/a | ONNX Runtime | yes | CUDA | ~1 GB | no | no | Detection |
| `Models\eaton-st2.pt` | `.pt` pickle | YOLO | Object detection | n/a | n/a | PyTorch | yes | CUDA | ~1 GB | no | no | Unsafe; not chat |
| `Models\harting-st2.engine` | TensorRT | YOLO-like (Harting) | Object detection | n/a | n/a | TensorRT | no | specific | ~1–2 GB | no | no | Not LLM |
| `Models\harting-st2.onnx` | ONNX | YOLO-like | Object detection | n/a | n/a | ONNX Runtime | yes | CUDA | ~1 GB | no | no | Detection |
| `Models\harting-st2.pt` | `.pt` pickle | YOLO | Object detection | n/a | n/a | PyTorch | yes | CUDA | ~1 GB | no | no | Unsafe; not chat |
| `Models\schneider_electric-st2.engine` | TensorRT | YOLO-like (Schneider) | Object detection | n/a | n/a | TensorRT | no | specific | ~1–2 GB | no | no | Not LLM |
| `Models\schneider_electric-st2.onnx` | ONNX | YOLO-like | Object detection | n/a | n/a | ONNX Runtime | yes | CUDA | ~1 GB | no | no | Detection |
| `Models\schneider_electric-st2.pt` | `.pt` pickle | YOLO | Object detection | n/a | n/a | PyTorch | yes | CUDA | ~1 GB | no | no | Unsafe; not chat |
| `Models\siemens-st2.engine` | TensorRT | YOLO-like (Siemens) | Object detection | n/a | n/a | TensorRT | no | specific | ~1–2 GB | no | no | Not LLM |
| `Models\siemens-st2.onnx` | ONNX | YOLO-like | Object detection | n/a | n/a | ONNX Runtime | yes | CUDA | ~1 GB | no | no | Detection |
| `Models\siemens-st2.pt` | `.pt` pickle | YOLO | Object detection | n/a | n/a | PyTorch | yes | CUDA | ~1 GB | no | no | Unsafe; not chat |
| `Models\stage1_obb_nms.engine` | TensorRT | OBB detector + NMS | Oriented box detection | n/a | n/a | TensorRT | no | specific | ~1–2 GB | no | no | Filename `obb_nms` |
| `Models\stage1_obb_nms.onnx` | ONNX | OBB + NMS | Oriented box detection | n/a | n/a | ONNX Runtime | yes | CUDA | ~1 GB | no | no | Detection |
| `Models\stage1_obb_nms.pt` | `.pt` pickle | OBB YOLO | Oriented box detection | n/a | n/a | PyTorch | yes | CUDA | ~1 GB | no | no | Unsafe; not chat |
| `Models\stage2_obb_nms.engine` | TensorRT | OBB stage 2 | Object detection | n/a | n/a | TensorRT | no | specific | ~1–2 GB | no | no | Detection |
| `Models\stage2_obb_nms.onnx` | ONNX | OBB stage 2 | Object detection | n/a | n/a | ONNX Runtime | yes | CUDA | ~1 GB | no | no | Detection |
| `Models\stage2_obb_nms.pt` | `.pt` pickle | OBB | Object detection | n/a | n/a | PyTorch | yes | CUDA | ~1 GB | no | no | Unsafe; not chat |
| `Models\craft_mlt_25k.pth` | PyTorch pickle (magic `0x80`) | CRAFT text detector | Scene-text **detection** (OCR pipeline stage 1) | multilingual text *locations* | n/a | PyTorch / EasyOCR | yes | CUDA | ~0.5–1 GB | **no** | no | Not a chatbot. Not loaded (unsafe pickle). |
| `Models\latin_g2.pth` | Weight file (first byte `0x50`, not loaded) | EasyOCR CRNN `latin_g2` | Latin **OCR recognition** | Latin scripts (DE/EN limited) | n/a | EasyOCR/PyTorch | yes | CUDA | ~200–400 MB | **no** | no | Not a chatbot. Not deserialized; OCR UI disabled until an ONNX/multimodal GGUF exists. |

**Missing in folder:** `.gguf`, `tokenizer.json`, `tokenizer.model`, `config.json`, `generation_config.json`, `.safetensors`, `.onnx.data`, Hugging Face `*.json` chat trees.

## Runtimes in the app

| Runtime | When used |
| --- | --- |
| Ollama HTTP (`127.0.0.1`) | Primary generative path if the user enables KI and Ollama serves an instruct model |
| LLamaSharp (CPU) | If a `.gguf` instruct file is added later under the models directory |
| ONNX Runtime GenAI | Only if a **generative** ONNX bundle (tokenizer + genai config) appears — current ONNX files are rejected |
| SQLite FTS5 | Knowledge search (no embedding model available) |
| Extractive fallback | Quotes AP2 chunks + deterministic evaluators when no LLM is loaded — never invents statutes |

## OCR / screenshots

Suitable **weights** exist (CRAFT + `latin_g2`) but they are not safe to load as chat or via pickle deserialization. Screenshot/OCR actions stay disabled with a German explanation until an ONNX OCR model or a multimodal GGUF is provided.
