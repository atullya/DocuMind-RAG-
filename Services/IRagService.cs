using RAGApp.Models;

namespace RAGApp.Services;

/// <summary>
/// The main RAG orchestrator - ties everything together
/// </summary>
public interface IRagService
{
    Task<RagResponse> AskAsync(string question);
    Task AddDocumentAsync(string content, string source);
    int GetDocumentCount();
    List<Document> GetAllDocuments();
}