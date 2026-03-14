namespace RAGApp.Services;

/// <summary>
/// Talks to the LLM (GPT) to generate answers
/// </summary>
public interface ILlmService
{
    Task<string> GenerateAnswerAsync(string question, List<string> contextDocs);
}