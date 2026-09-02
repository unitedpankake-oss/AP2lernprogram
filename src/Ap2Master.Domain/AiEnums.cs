namespace Ap2Master.Domain;

public enum AiRuntimeKind
{
    Auto = 0,
    Ollama = 1,
    LlamaSharp = 2,
    OnnxGenAi = 3,
    ExtractiveFallback = 4,
    Fake = 5
}

public enum LocalModelTask
{
    Unknown = 0,
    ChatInstruct = 1,
    Embedding = 2,
    ObjectDetection = 3,
    OcrDetection = 4,
    OcrRecognition = 5,
    TensorRtEngine = 6,
    VisionMultimodal = 7
}

public enum ExamAssistantMode
{
    Full = 0,
    Disabled = 1,
    LanguageHelp = 2,
    Accessibility = 3
}

public enum AssistantHintLevel
{
    None = 0,
    Denkanstoss = 1,
    Vorgehen = 2,
    Teillosung = 3,
    Musterloesung = 4
}

public enum AssistantIntent
{
    Chat = 0,
    ExplainSimpler = 1,
    ExplainStepByStep = 2,
    Hint = 3,
    Example = 4,
    TranslateRu = 5,
    TranslateEn = 6,
    Glossary = 7,
    AnalyseAnswer = 8,
    ExplainMistake = 9,
    SimilarExercise = 10,
    Flashcards = 11,
    Summarise = 12,
    TranslateUk = 13,
    TranslateEs = 14
}

public static class AiDefaults
{
    public const string ModelsDirectory = @"C:\Users\ivan.ho\source\repos\AP2lernprogram\Models";
    public const string LegacyModelsDirectory = @"C:\Users\ivan.ho\Work\Models";
    public const string OllamaBaseUrl = "http://127.0.0.1:11434";
    public const string RecommendedOllamaModel = "qwen2.5:7b-instruct";
    public const string LightweightOllamaModel = "qwen2.5:3b-instruct";
    public const int DefaultMaxContextTokens = 8192;
    public const double DefaultTemperature = 0.2;
}
