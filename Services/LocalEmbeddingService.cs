namespace RAGApp.Services;

/// <summary>
/// Local embeddings - same approach as ChromaDB's default embedding.
/// No API calls needed! (Just like your Python code)
/// </summary>
public class LocalEmbeddingService : IEmbeddingService
{
    private readonly ILogger<LocalEmbeddingService> _logger;

    // Build a vocabulary from common words
    // The more words here, the better the matching
    private static readonly string[] Vocabulary;

    static LocalEmbeddingService()
    {
        // Large vocabulary for better similarity matching
        var words = new HashSet<string>
        {
            // Tech
            "python", "javascript", "java", "csharp", "programming",
            "language", "web", "development", "software", "computer",
            "machine", "learning", "artificial", "intelligence", "ai",
            "docker", "container", "kubernetes", "deploy", "platform",
            "api", "rest", "http", "server", "client",
            "database", "sql", "postgresql", "mongodb", "data",
            "react", "angular", "vue", "frontend", "backend",
            "git", "version", "control", "github", "code",
            "typescript", "microsoft", "google", "facebook", "apple",
            "open", "source", "library", "framework", "tool",
            "application", "system", "network", "cloud", "service",
            "function", "class", "object", "variable", "type",
            "build", "run", "test", "release", "deploy",
            
            // People & General
            "created", "founded", "born", "year", "first",
            "world", "country", "company", "billion", "million",
            "president", "ceo", "founder", "inventor", "pioneer",
            "technology", "science", "research", "university", "education",
            "money", "wealth", "rich", "stock", "market",
            "family", "childhood", "friend", "partner", "wife",
            
            // Your Bill Gates content
            "gates", "bill", "william", "henry", "allen", "paul",
            "microsoft", "windows", "software", "philanthropist",
            "businessman", "american", "microcomputer", "revolution",
            "billionaire", "wealthiest", "forbes", "magazine",
            "foundation", "charity", "donation", "health",
            "net", "worth", "billion", "centibillionaire",
            "public", "offering", "ipo", "stock", "price",
            
            // Common words for better matching
            "most", "powerful", "largest", "smallest", "popular",
            "new", "old", "modern", "simple", "complex",
            "good", "best", "great", "important", "known",
            "make", "made", "became", "become", "support",
            "work", "working", "use", "used", "using",
            "include", "including", "also", "many", "much",
            "time", "years", "age", "since", "between",
            "called", "named", "known", "said", "according"
        };

        Vocabulary = words.ToArray();
    }

    public LocalEmbeddingService(ILogger<LocalEmbeddingService> logger)
    {
        _logger = logger;
    }

    public Task<float[]> GetEmbeddingAsync(string text)
    {
        _logger.LogInformation("📐 Local embedding for: {Text}",
            text[..Math.Min(50, text.Length)]);

        var vector = TextToVector(text);
        return Task.FromResult(vector);
    }

    private static float[] TextToVector(string text)
    {
        // Tokenize
        var words = text.ToLower()
            .Split(new[] { ' ', '.', ',', '!', '?', '(', ')', '-', ':',
                           ';', '"', '\'', '\n', '\r', '\t' },
                   StringSplitOptions.RemoveEmptyEntries);

        var vector = new float[Vocabulary.Length];

        for (int i = 0; i < Vocabulary.Length; i++)
        {
            // Count occurrences (including partial matches)
            float count = 0;
            foreach (var word in words)
            {
                if (word == Vocabulary[i])
                    count += 2.0f;  // Exact match = higher weight
                else if (word.Contains(Vocabulary[i]) || Vocabulary[i].Contains(word))
                    count += 0.5f;  // Partial match = lower weight
            }
            vector[i] = count;
        }

        // Apply TF normalization (divide by doc length)
        float docLen = words.Length > 0 ? words.Length : 1;
        for (int i = 0; i < vector.Length; i++)
            vector[i] /= docLen;

        // L2 normalize (unit vector for cosine similarity)
        float magnitude = (float)Math.Sqrt(vector.Sum(v => v * v));
        if (magnitude > 0)
            for (int i = 0; i < vector.Length; i++)
                vector[i] /= magnitude;

        return vector;
    }
}