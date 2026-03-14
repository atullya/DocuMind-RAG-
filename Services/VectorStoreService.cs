using RAGApp.Models;

namespace RAGApp.Services;

public class VectorStoreService : IVectorStoreService
{
    private readonly List<Document> _documents = new();
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<VectorStoreService> _logger;
    private int _idCounter = 0;

    public VectorStoreService(
        IEmbeddingService embeddingService,
        ILogger<VectorStoreService> logger)
    {
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task AddDocumentAsync(string content, string source = "manual")
    {
        var embedding = await _embeddingService.GetEmbeddingAsync(content);
        _documents.Add(new Document
        {
            Id = $"doc_{_idCounter++}",
            Content = content,
            Embedding = embedding,
            Source = source,
            AddedAt = DateTime.UtcNow
        });
        _logger.LogInformation("📄 Added document: doc_{Id}", _idCounter - 1);
    }

    public Task<List<string>> SearchAsync(float[] queryEmbedding, int topK = 3)
    {
        if (_documents.Count == 0)
            return Task.FromResult(new List<string>());

        var results = _documents
            .Select(doc => new
            {
                doc.Content,
                Score = CosineSimilarity(queryEmbedding, doc.Embedding)
            })
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => x.Content)
            .ToList();

        return Task.FromResult(results);
    }

    public int GetDocumentCount() => _documents.Count;
    public List<Document> GetAllDocuments() => _documents.ToList();

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }
        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}