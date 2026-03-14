using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RAGApp.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(IConfiguration config, ILogger<EmbeddingService> logger)
    {
        _httpClient = new HttpClient();
        _apiKey = config["RagSettings:OpenAiApiKey"]!;
        _model = config["RagSettings:EmbeddingModel"] ?? "text-embedding-3-small";
        _logger = logger;
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        int maxRetries = 5;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation(
                    "📡 Embedding (attempt {A}): {Text}",
                    attempt, text[..Math.Min(50, text.Length)]);

                var request = new { input = text, model = _model };
                var json = JsonSerializer.Serialize(request);

                // ── Build fresh request each time ──
                using var httpRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.openai.com/v1/embeddings");

                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _apiKey);
                httpRequest.Content =
                    new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);

                // ── Handle Rate Limit (429) ──
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var waitSeconds = attempt * 5; // 5s, 10s, 15s, 20s, 25s
                    _logger.LogWarning(
                        "⏳ Rate limited! Waiting {S}s before retry {A}/{M}",
                        waitSeconds, attempt, maxRetries);

                    await Task.Delay(waitSeconds * 1000);
                    continue;
                }

                // ── Handle other errors ──
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "❌ API Error {Code}: {Body}",
                        response.StatusCode, errorBody);

                    // If it's a billing/auth issue, don't retry
                    if (response.StatusCode == HttpStatusCode.Unauthorized ||
                        response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new Exception(
                            $"OpenAI Auth Error: {errorBody}. " +
                            "Check your API key and billing.");
                    }

                    await Task.Delay(attempt * 2000);
                    continue;
                }

                // ── SUCCESS: Parse the embedding ──
                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);

                var embeddingArray = doc.RootElement
                    .GetProperty("data")[0]
                    .GetProperty("embedding");

                var embedding = new float[embeddingArray.GetArrayLength()];
                int i = 0;
                foreach (var val in embeddingArray.EnumerateArray())
                    embedding[i++] = val.GetSingle();

                _logger.LogInformation("✅ Embedding received ({Len} dimensions)",
                    embedding.Length);

                return embedding;
            }
            catch (HttpRequestException ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(
                    "🔄 Network error, retrying {A}/{M}: {Msg}",
                    attempt, maxRetries, ex.Message);

                await Task.Delay(attempt * 3000);
            }
        }

        throw new Exception("Failed to get embedding after all retries.");
    }
}