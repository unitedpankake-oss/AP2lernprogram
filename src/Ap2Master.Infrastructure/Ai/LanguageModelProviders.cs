using System.Runtime.CompilerServices;
using Ap2Master.Application.Ai;
using Ap2Master.Domain;

namespace Ap2Master.Infrastructure.Ai;

public sealed class FakeLanguageModelProvider : ILocalLanguageModelProvider
{
    public string Name => "Fake";
    public bool IsLoaded { get; private set; }
    public string StatusGerman { get; private set; } = "Testdouble bereit";
    public bool FailLoad { get; set; }
    public bool WasCancelled { get; private set; }
    public string LastPrompt { get; private set; } = "";

    public Task<ModelValidationResult> ValidateAsync(LocalModelDefinition model, CancellationToken cancellationToken)
    {
        if (!model.SuitableForChat)
            return Task.FromResult(new ModelValidationResult(false, "Format wird nicht unterstützt", false));
        return Task.FromResult(new ModelValidationResult(true, "Testdouble akzeptiert", true));
    }

    public Task LoadAsync(LocalModelDefinition model, ModelLoadOptions options, IProgress<ModelLoadProgress>? progress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (FailLoad) throw new InvalidOperationException("Nicht genügend Arbeitsspeicher");
        progress?.Report(new ModelLoadProgress("Modell wird geladen …", 50));
        IsLoaded = true;
        StatusGerman = "KI-Assistent ist bereit";
        progress?.Report(new ModelLoadProgress(StatusGerman, 100));
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<AssistantToken> GenerateAsync(LocalGenerationRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        LastPrompt = request.UserPrompt;
        if (request.UserPrompt.Length > request.MaxContextTokens * 4)
        {
            yield return new AssistantToken("Kontextlimit überschritten. Kürzen Sie die Anfrage.", true, true);
            yield break;
        }
        var text = "FAKE:" + request.UserPrompt;
        if (text.Length > request.MaxTokens)
            text = text[..request.MaxTokens];
        foreach (var chunk in Chunk(text, 24))
        {
            cancellationToken.ThrowIfCancellationRequested();
            WasCancelled = cancellationToken.IsCancellationRequested;
            yield return new AssistantToken(chunk);
            await Task.Yield();
        }
        yield return new AssistantToken("", false, true);
    }

    public Task UnloadAsync(CancellationToken cancellationToken)
    {
        IsLoaded = false;
        StatusGerman = "Modell entladen";
        return Task.CompletedTask;
    }

    private static IEnumerable<string> Chunk(string s, int n)
    {
        for (var i = 0; i < s.Length; i += n)
            yield return s.Substring(i, Math.Min(n, s.Length - i));
    }
}

public sealed class OllamaLanguageModelProvider : ILocalLanguageModelProvider
{
    private readonly HttpClient _http;
    private string _model = AiDefaults.RecommendedOllamaModel;
    private readonly string _base;

    public OllamaLanguageModelProvider(HttpClient http, string? baseUrl = null)
    {
        _http = http;
        _base = (baseUrl ?? AiDefaults.OllamaBaseUrl).TrimEnd('/');
    }

    public string Name => "Ollama";
    public bool IsLoaded { get; private set; }
    public string StatusGerman { get; private set; } = "Ollama nicht geladen";

    public async Task<ModelValidationResult> ValidateAsync(LocalModelDefinition model, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            var r = await _http.GetAsync(_base + "/api/tags", cts.Token);
            if (!r.IsSuccessStatusCode)
                return new ModelValidationResult(false, "Kein geeignetes Sprachmodell gefunden", false);
            return new ModelValidationResult(true, "Ollama erreichbar (nur lokal).", true);
        }
        catch
        {
            return new ModelValidationResult(false, "Kein geeignetes Sprachmodell gefunden", false);
        }
    }

    public Task LoadAsync(LocalModelDefinition model, ModelLoadOptions options, IProgress<ModelLoadProgress>? progress, CancellationToken cancellationToken)
    {
        progress?.Report(new ModelLoadProgress("Modell wird geprüft …", 20));
        _model = string.IsNullOrWhiteSpace(model.FileName) ? AiDefaults.RecommendedOllamaModel : model.FileName;
        if (_model.Contains('\\') || _model.Contains('/'))
            _model = AiDefaults.RecommendedOllamaModel;
        IsLoaded = true;
        StatusGerman = "KI-Assistent ist bereit";
        progress?.Report(new ModelLoadProgress(StatusGerman, 100));
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<AssistantToken> GenerateAsync(LocalGenerationRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (request.UserPrompt.Length > request.MaxContextTokens * 4)
        {
            yield return new AssistantToken("Kontextlimit überschritten.", true, true);
            yield break;
        }

        var payload = $$"""
            {"model":{{System.Text.Json.JsonSerializer.Serialize(_model)}},"stream":true,"options":{"temperature":{{request.Temperature.ToString(System.Globalization.CultureInfo.InvariantCulture)}},"num_predict":{{request.MaxTokens}}},"messages":[{"role":"system","content":{{System.Text.Json.JsonSerializer.Serialize(request.SystemPrompt)}}},{"role":"user","content":{{System.Text.Json.JsonSerializer.Serialize(request.UserPrompt)}}}]}
            """;
        HttpResponseMessage? resp = null;
        string? connectError = null;
        try
        {
            resp = await _http.SendAsync(new HttpRequestMessage(HttpMethod.Post, _base + "/api/chat")
            {
                Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json")
            }, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }
        catch (Exception ex)
        {
            connectError = "Ollama ist nicht erreichbar (nur localhost). " + Short(ex.Message);
        }

        if (connectError is not null)
        {
            yield return new AssistantToken(connectError, true, true);
            yield break;
        }

        if (resp is null || !resp.IsSuccessStatusCode)
        {
            yield return new AssistantToken("Ollama-Fehler: " + (resp is null ? "keine Antwort" : ((int)resp.StatusCode).ToString()), true, true);
            yield break;
        }

        await using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line)) continue;
            string? chunk = null;
            var done = false;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(line);
                if (doc.RootElement.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var c))
                    chunk = c.GetString();
                if (doc.RootElement.TryGetProperty("done", out var d) && d.GetBoolean())
                    done = true;
            }
            catch (System.Text.Json.JsonException)
            {
                // skip malformed stream line
            }
            if (!string.IsNullOrEmpty(chunk))
                yield return new AssistantToken(chunk);
            if (done) break;
        }
        yield return new AssistantToken("", false, true);
    }

    public Task UnloadAsync(CancellationToken cancellationToken)
    {
        IsLoaded = false;
        StatusGerman = "Modell entladen";
        return Task.CompletedTask;
    }

    private static string Short(string s) => s.Length < 120 ? s : s[..120];
}

public sealed class LlamaSharpLanguageModelProvider : ILocalLanguageModelProvider
{
    public string Name => "LLamaSharp";
    public bool IsLoaded => false;
    public string StatusGerman { get; private set; } = "Kein GGUF geladen";

    public Task<ModelValidationResult> ValidateAsync(LocalModelDefinition model, CancellationToken cancellationToken)
    {
        if (model.Format != "gguf" || !model.SuitableForChat)
            return Task.FromResult(new ModelValidationResult(false, "Format wird nicht unterstützt", false));
        return Task.FromResult(new ModelValidationResult(true, "GGUF erkannt. LLamaSharp-CPU kann geladen werden, sobald die Datei ein Instruct-LLM ist.", true));
    }

    public Task LoadAsync(LocalModelDefinition model, ModelLoadOptions options, IProgress<ModelLoadProgress>? progress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StatusGerman = "Kein geeignetes Sprachmodell gefunden";
        return Task.FromException(new InvalidOperationException(
            "Im Modellordner liegt kein geladenes GGUF-LLM. Legen Sie eine Instruct-GGUF-Datei ab oder nutzen Sie Ollama (qwen2.5:7b-instruct). Originaldateien wurden nicht verändert."));
    }

    public async IAsyncEnumerable<AssistantToken> GenerateAsync(LocalGenerationRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();
        yield return new AssistantToken("Kein geeignetes Sprachmodell gefunden", true, true);
    }

    public Task UnloadAsync(CancellationToken cancellationToken)
    {
        StatusGerman = "Modell entladen";
        return Task.CompletedTask;
    }
}

public sealed class OnnxGenAiLanguageModelProvider : ILocalLanguageModelProvider
{
    public string Name => "OnnxGenAI";
    public bool IsLoaded => false;
    public string StatusGerman { get; private set; } = "Kein generatives ONNX";

    public Task<ModelValidationResult> ValidateAsync(LocalModelDefinition model, CancellationToken cancellationToken)
    {
        if (model.Task == LocalModelTask.ObjectDetection || model.Format == "onnx" && !model.SuitableForChat)
            return Task.FromResult(new ModelValidationResult(false, "Format wird nicht unterstützt", false));
        if (!model.SuitableForChat)
            return Task.FromResult(new ModelValidationResult(false, "Tokenizer fehlt", false));
        return Task.FromResult(new ModelValidationResult(true, "Generatives ONNX möglich.", true));
    }

    public Task LoadAsync(LocalModelDefinition model, ModelLoadOptions options, IProgress<ModelLoadProgress>? progress, CancellationToken cancellationToken)
    {
        StatusGerman = "Format wird nicht unterstützt";
        return Task.FromException(new InvalidOperationException("Die vorhandenen ONNX-Dateien sind Objektdetektoren, keine Textgeneratoren."));
    }

    public async IAsyncEnumerable<AssistantToken> GenerateAsync(LocalGenerationRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Yield();
        yield return new AssistantToken("Format wird nicht unterstützt", true, true);
    }

    public Task UnloadAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
