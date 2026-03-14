using System.Text;
using System.Text.Json;

namespace RAGApp.Services;

/// <summary>
/// Uses Gemini ONLY for generating answers (like your Python code)
/// </summary>
public class GeminiLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeminiLlmService> _logger;

    private static readonly string[] ModelsToTry = {
        "gemini-2.5-flash",
        "gemini-2.0-flash", 
        "gemini-1.5-flash",
        "gemini-1.5-pro"
    };

    private string? _workingModel = null;

    public GeminiLlmService(
        IConfiguration config,
        ILogger<GeminiLlmService> logger)
    {
        _httpClient = new HttpClient();
        _apiKey = config["GeminiSettings:ApiKey"]!;
        _logger = logger;
    }

    public async Task<string> GenerateAnswerAsync(
        string question, List<string> contextDocs)
    {
        var context = string.Join("\n", contextDocs.Select(d => $"- {d}"));

        // ★ Same prompt style as your Python code ★
        var prompt = $@"You are an assistant that answers questions using ONLY the provided context.
Do NOT use your own knowledge or make assumptions.
If the answer is not present in the context, respond: ""This information is not present in the given documents.""

Context:
{context}

Question:
{question}

Answer:";

        // Try known working model first
        if (_workingModel != null)
        {
            try
            {
                return await CallGemini(prompt, _workingModel);
            }
            catch
            {
                _workingModel = null; // Reset and try all
            }
        }

        // Auto-detect working model
        foreach (var model in ModelsToTry)
        {
            try
            {
                _logger.LogInformation("🔍 Trying: {Model}", model);
                var result = await CallGemini(prompt, model);
                _workingModel = model;
                _logger.LogInformation("✅ Using: {Model}", model);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("❌ {Model}: {Err}", model,
                    ex.Message[..Math.Min(60, ex.Message.Length)]);
            }
        }

        return "Error: Could not connect to Gemini. Check your API key.";
    }

    private async Task<string> CallGemini(string prompt, string model)
    {
        // ★ Same API your Python code uses internally ★
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

        var body = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 1000
            }
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception(err);
        }

        var respJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(respJson);

        return doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "No response.";
    }
}