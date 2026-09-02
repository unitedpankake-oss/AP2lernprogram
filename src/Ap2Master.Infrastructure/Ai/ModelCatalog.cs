using Ap2Master.Application.Ai;
using Ap2Master.Domain;

namespace Ap2Master.Infrastructure.Ai;

public static class ModelFormatDetector
{
    public static string DetectFormat(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".gguf" => "gguf",
            ".onnx" => "onnx",
            ".onnx.data" => "onnx-data",
            ".safetensors" => "safetensors",
            ".bin" => "bin",
            ".pt" => "pt",
            ".pth" => "pth",
            ".engine" => "tensorrt-engine",
            ".json" => Path.GetFileName(path).ToLowerInvariant(),
            ".model" => "tokenizer.model",
            _ => ext.Trim('.')
        };
    }

    public static bool LooksLikeTokenizer(string fileName)
    {
        var n = fileName.ToLowerInvariant();
        return n is "tokenizer.json" or "tokenizer.model" or "generation_config.json"
               || n.Contains("tokenizer", StringComparison.Ordinal);
    }

    public static bool HasTokenizerNearby(string modelPath)
    {
        var dir = Path.GetDirectoryName(modelPath);
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return false;
        return Directory.EnumerateFiles(dir).Any(f => LooksLikeTokenizer(Path.GetFileName(f)));
    }

    public static bool HasGenAiConfigNearby(string modelPath)
    {
        var dir = Path.GetDirectoryName(modelPath);
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return false;
        return File.Exists(Path.Combine(dir, "genai_config.json"))
               || File.Exists(Path.Combine(dir, "config.json"));
    }
}

public static class ModelTaskClassifier
{
    public static (LocalModelTask Task, bool Chat, string Reason, string? Architecture) Classify(string path, string format)
    {
        var name = Path.GetFileName(path).ToLowerInvariant();

        if (format is "tokenizer.json" or "tokenizer.model" or "generation_config.json" or "config.json")
            return (LocalModelTask.Unknown, false, "Tokenizer- oder Konfigurationsdatei, kein Inferenzmodell.", null);

        if (format == "tensorrt-engine" || name.EndsWith(".engine"))
            return (LocalModelTask.TensorRtEngine, false, "TensorRT-.engine ist GPU-/versionsgebunden und kein Chatmodell.", "TensorRT");

        if (IsOcrDetectionName(name))
            return (LocalModelTask.OcrDetection, false, "CRAFT/Textdetektor — kein Chatmodell. .pth wird nicht unsicher geladen.", "CRAFT");

        if (IsOcrRecognitionName(name))
            return (LocalModelTask.OcrRecognition, false, "OCR-Erkenner (EasyOCR latin_g2) — kein Chatmodell. .pth wird nicht unsicher geladen.", "EasyOCR-CRNN");

        if (IsDetectionName(name) || format is "pt" or "pth" && IsDetectionName(name))
            return (LocalModelTask.ObjectDetection, false, "Objektdetektor (YOLO/OBB/Hersteller-st2) — kein Chatmodell.", "YOLO-like");

        if (format is "pt" or "pth")
            return (LocalModelTask.Unknown, false, "Unbekanntes PyTorch-Pickle. Wird nicht deserialisiert und nicht als Chat verwendet.", "PyTorch-pickle");

        if (format == "onnx")
        {
            if (ModelFormatDetector.HasGenAiConfigNearby(path) && ModelFormatDetector.HasTokenizerNearby(path))
                return (LocalModelTask.ChatInstruct, true, "Generatives ONNX mit Tokenizer/GenAI-Konfiguration.", "ONNX-GenAI");
            if (IsEmbeddingName(name))
                return (LocalModelTask.Embedding, false, "Embedding-ONNX, nicht als Chatbot.", "embedding");
            return (LocalModelTask.ObjectDetection, false, "ONNX ohne Tokenizer/GenAI-Config wird nicht als Textgenerator angenommen.", "ONNX");
        }

        if (format == "gguf")
        {
            if (IsEmbeddingName(name))
                return (LocalModelTask.Embedding, false, "GGUF-Embedding, nicht als Chatbot.", "gguf-embed");
            if (name.Contains("llava", StringComparison.Ordinal) || name.Contains("vision", StringComparison.Ordinal))
                return (LocalModelTask.VisionMultimodal, true, "Multimodales GGUF — Chat mit optionaler Bildfähigkeit.", "gguf-mm");
            return (LocalModelTask.ChatInstruct, true, "GGUF-Sprachmodell (Instruct/Chat, sofern Datei ein LLM ist).", "gguf-llm");
        }

        if (format == "safetensors")
            return (LocalModelTask.Unknown, false, "Safetensors ohne HF-Chat-Runtime in diesem Ordner nicht als Chat geladen.", "safetensors");

        return (LocalModelTask.Unknown, false, "Format wird nicht als Chat unterstützt.", format);
    }

    public static AiRuntimeKind RecommendRuntime(LocalModelDefinition? model)
    {
        if (model is null) return AiRuntimeKind.Ollama;
        return model.Format switch
        {
            "gguf" when model.SuitableForChat => AiRuntimeKind.LlamaSharp,
            "onnx" when model.SuitableForChat => AiRuntimeKind.OnnxGenAi,
            _ => AiRuntimeKind.Ollama
        };
    }

    private static bool IsDetectionName(string n) =>
        n.Contains("yolo") || n.Contains("obb") || n.Contains("-st2") || n.Contains("st2.")
        || n.Contains("nms") || n.Contains("detect");

    private static bool IsOcrDetectionName(string n) => n.Contains("craft");
    private static bool IsOcrRecognitionName(string n) => n.Contains("latin_g2") || n.Contains("easyocr") || n.Contains("crnn");
    private static bool IsEmbeddingName(string n) =>
        n.Contains("embed") || n.Contains("e5") || n.Contains("bge") || n.Contains("minilm") || n.Contains("nomic");
}

public sealed class ModelCatalogService : IModelCatalogService
{
    private static readonly HashSet<string> ModelExt = new(StringComparer.OrdinalIgnoreCase)
    {
        ".gguf", ".onnx", ".safetensors", ".bin", ".pt", ".pth", ".engine"
    };

    public IReadOnlyList<LocalModelDefinition> Scan(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return [];

        var list = new List<LocalModelDefinition>();
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(file);
            var name = Path.GetFileName(file);
            if (ModelFormatDetector.LooksLikeTokenizer(name) || name.Equals("config.json", StringComparison.OrdinalIgnoreCase)
                || name.Equals("generation_config.json", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(ToDef(file, ModelFormatDetector.DetectFormat(file)));
                continue;
            }
            if (!ModelExt.Contains(ext)) continue;
            if (ext.Equals(".bin", StringComparison.OrdinalIgnoreCase) && name.Contains("onnx", StringComparison.OrdinalIgnoreCase) == false
                && !name.Contains("pytorch", StringComparison.OrdinalIgnoreCase))
            {
                // skip random binaries unless clearly model weights
            }
            list.Add(ToDef(file, ModelFormatDetector.DetectFormat(file)));
        }
        return list.OrderBy(m => m.Path, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public LocalModelDefinition? SelectPrimary(IReadOnlyList<LocalModelDefinition> models)
        => models.FirstOrDefault(m => m.SuitableForChat && m.Task is LocalModelTask.ChatInstruct or LocalModelTask.VisionMultimodal);

    public LocalModelDefinition? SelectSecondary(IReadOnlyList<LocalModelDefinition> models)
        => models.FirstOrDefault(m => m.Task == LocalModelTask.Embedding);

    public AiRuntimeKind RecommendRuntime(LocalModelDefinition? model) => ModelTaskClassifier.RecommendRuntime(model);

    private static LocalModelDefinition ToDef(string path, string format)
    {
        var (task, chat, reason, arch) = ModelTaskClassifier.Classify(path, format);
        long size = 0;
        try { size = new FileInfo(path).Length; } catch { /* ignore */ }
        return new LocalModelDefinition(
            path, Path.GetFileName(path), format, task, chat, reason, size,
            ModelFormatDetector.HasTokenizerNearby(path), arch);
    }
}
