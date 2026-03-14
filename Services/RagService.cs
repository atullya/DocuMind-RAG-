using System.Diagnostics;
using RAGApp.Models;

namespace RAGApp.Services;

public class RagService : IRagService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStoreService _vectorStore;
    private readonly ILlmService _llmService;
    private readonly IConfiguration _config;
    private readonly ILogger<RagService> _logger;

    public RagService(
        IEmbeddingService embeddingService,
        IVectorStoreService vectorStore,
        ILlmService llmService,
        IConfiguration config,
        ILogger<RagService> logger)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _llmService = llmService;
        _config = config;
        _logger = logger;
    }

    public async Task<RagResponse> AskAsync(string question)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // RETRIEVE
            var qEmbed = await _embeddingService.GetEmbeddingAsync(question);
            int topK = _config.GetValue<int>("RagSettings:TopK", 3);
            var docs = await _vectorStore.SearchAsync(qEmbed, topK);

            // GENERATE
            var answer = await _llmService.GenerateAnswerAsync(question, docs);

            // Convert List<string> docs to List<Document>
            var retrievedDocs = docs.Select(d => new Document { Content = d }).ToList();

            sw.Stop();
            return new RagResponse
            {
                Question = question,
                Answer = answer,
                RetrievedDocuments = retrievedDocs,
                ProcessingTimeMs = sw.ElapsedMilliseconds,
                Success = true
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "RAG failed");
            return new RagResponse
            {
                Question = question,
                Answer = $"Error: {ex.Message}",
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTimeMs = sw.ElapsedMilliseconds
            };
        }
    }

    public async Task AddDocumentAsync(string content, string source)
    {
        await _vectorStore.AddDocumentAsync(content, source);
    }

    public int GetDocumentCount() => _vectorStore.GetDocumentCount();
    public List<Document> GetAllDocuments() => _vectorStore.GetAllDocuments();
}