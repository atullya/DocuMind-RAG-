using RAGApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ═══════════════════════════════════════════════════
//  LOCAL embeddings + GEMINI chat 
//  (Same architecture as your Python code!)
// ═══════════════════════════════════════════════════
builder.Services.AddSingleton<IEmbeddingService, LocalEmbeddingService>();  // Like ChromaDB default
builder.Services.AddSingleton<ILlmService, GeminiLlmService>();             // Like your Python code
builder.Services.AddSingleton<IVectorStoreService, VectorStoreService>();
builder.Services.AddSingleton<IRagService, RagService>();

builder.Logging.AddConsole();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Rag}/{action=Index}/{id?}");

Console.WriteLine("==========================================");
Console.WriteLine("  🚀 RAG App Running!");
Console.WriteLine("  📐 Embeddings: Local (no API needed)");
Console.WriteLine("  🤖 Chat: Gemini 2.5 Flash");
Console.WriteLine("==========================================");

app.Run();