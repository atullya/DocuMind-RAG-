using System.Text;
using System.Text.Json;

namespace RAGApp.Services;

public class GeminiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeminiEmbeddingService> _logger;

    // ★ Will be auto-detected on first call ★
    private string? _workingModel = null;
    private string? _workingApiVersion = null;

    // All possible model + API version combinations to try
    private static readonly (string model, string version)[] ModelsToTry = {
        ("text-embedding-004",    "v1beta"),
        ("text-embedding-004",    "v1"),
        ("embedding-001",         "v1beta"),
        ("embedding-001",         "v1"),
        ("text-embedding-005",    "v1beta"),
        ("gemini-embedding-exp-03-07", "v1beta"),
    };

    public GeminiEmbeddingService(
        IConfiguration config,
        ILogger<GeminiEmbeddingService> logger)
    {
        _httpClient = new HttpClient();
        _apiKey = config["GeminiSettings:ApiKey"]!;
        _logger = logger;

        // Use configured model as first choice
        var configured = config["GeminiSettings:EmbeddingModel"];
        if (!string.IsNullOrEmpty(configured))
            _workingModel = configured;
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        // If we already found a working model, use it directly
        if (_workingModel != null && _workingApiVersion != null)
        {
            return await CallEmbeddingApi(text, _workingModel, _workingApiVersion);
        }

        // First time: try all models until one works
        _logger.LogInformation("🔍 Auto-detecting working embedding model...");

        foreach (var (model, version) in ModelsToTry)
        {
            try
            {
                _logger.LogInformation("   Trying: {Model} (API {Version})", model, version);

                var result = await CallEmbeddingApi(text, model, version);

                // ★ It worked! Remember this model for future calls ★
                _workingModel = model;
                _workingApiVersion = version;

                _logger.LogInformation(
                    "✅ Found working model: {Model} (API {Version})",
                    model, version);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "   ❌ {Model} ({Version}) failed: {Msg}",
                    model, version, ex.Message[..Math.Min(80, ex.Message.Length)]);
            }
        }

        // If ALL Gemini models fail, fall back to local embeddings
        _logger.LogWarning("⚠️ All Gemini models failed! Using local embeddings.");
        return LocalEmbedding(text);
    }

    private async Task<float[]> CallEmbeddingApi(
        string text, string model, string apiVersion)
    {
        var url = $"https://generativelanguage.googleapis.com/{apiVersion}/models/{model}:embedContent?key={_apiKey}";

        var requestBody = new
        {
            model = $"models/{model}",
            content = new
            {
                parts = new[]
                {
                    new { text = text }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"{response.StatusCode}: {error}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var values = doc.RootElement
            .GetProperty("embedding")
            .GetProperty("values");

        var embedding = new float[values.GetArrayLength()];
        int i = 0;
        foreach (var val in values.EnumerateArray())
            embedding[i++] = val.GetSingle();

        return embedding;
    }

    /// <summary>
    /// Fallback: local embedding if Gemini doesn't work at all
    /// </summary>
    private float[] LocalEmbedding(string text)
    {
        string[] vocab = {
            "python", "javascript", "java", "programming",
            "language", "web", "development", "created", "built",
            "machine", "learning", "artificial", "intelligence",
            "docker", "container", "kubernetes", "platform",
            "api", "rest", "http", "database", "sql", "postgresql",
            "react", "angular", "frontend", "backend",
            "git", "version", "control", "github", "code",
            "typescript", "microsoft", "google", "facebook",
            "open", "source", "library", "framework", "tool",
            "server", "client", "application", "system",
            "linus", "torvalds", "guido", "rossum",
            "popular", "powerful", "modern", "simple",
            "interface", "user", "component", "subset", "superset"
        };

        var words = text.ToLower().Split(' ', '.', ',', '?', '!', '(', ')');
        var vector = new float[vocab.Length];

        for (int i = 0; i < vocab.Length; i++)
            vector[i] = words.Count(w => w.Contains(vocab[i]));

        float mag = (float)Math.Sqrt(vector.Sum(v => v * v));
        if (mag > 0)
            for (int i = 0; i < vector.Length; i++)
                vector[i] /= mag;

        return vector;
    }
}