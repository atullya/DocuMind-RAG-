using RAGApp.Models;

namespace RAGApp.Services;

public interface IVectorStoreService
{
    Task AddDocumentAsync(string content, string source = "manual");
    Task<List<string>> SearchAsync(float[] queryEmbedding, int topK = 3);
    int GetDocumentCount();
    List<Document> GetAllDocuments();
}