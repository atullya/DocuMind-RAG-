using Microsoft.AspNetCore.Mvc;
using RAGApp.Models;
using RAGApp.Services;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace RAGApp.Controllers;

public class RagController : Controller
{
    private readonly IRagService _ragService;

    // Chat history stored in memory (use DB in production)
    private static readonly ChatHistory _chatHistory = new();

    public RagController(IRagService ragService)
    {
        _ragService = ragService;
    }

    // ──────────────────────────────────
    //  GET /Rag  → Show the chat page
    // ──────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index()
    {

        _chatHistory.DocumentCount = _ragService.GetDocumentCount();
        return View(_chatHistory);
    }

    // ──────────────────────────────────
    //  POST /Rag/Ask  → Ask a question
    // ──────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Ask(RagRequest request)
    {
        if (!ModelState.IsValid)
        {
            _chatHistory.DocumentCount = _ragService.GetDocumentCount();
            return View("Index", _chatHistory);
        }

        // Add user message to history
        _chatHistory.Messages.Add(new ChatMessage
        {
            Role = "user",
            Content = request.Question
        });

        // ★ Run the RAG pipeline ★
        var response = await _ragService.AskAsync(request.Question);

        // Add assistant response to history
        _chatHistory.Messages.Add(new ChatMessage
        {
            Role = "assistant",
            Content = response.Answer,
            Sources = response.RetrievedDocuments?.Select(d => d.Content).ToList() ?? new List<string>()
        });

        _chatHistory.DocumentCount = _ragService.GetDocumentCount();

        return View("Index", _chatHistory);
    }

    // ──────────────────────────────────
    //  GET /Rag/Documents → Manage docs
    // ──────────────────────────────────
    [HttpGet]
    public IActionResult Documents()
    {
        var docs = _ragService.GetAllDocuments();
        return View(docs);
    }

    // ──────────────────────────────────
    //  POST /Rag/AddDocument → Add doc
    // ──────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> AddDocument(DocumentUploadModel model)
    {
        if (model.UploadedFile == null && string.IsNullOrWhiteSpace(model.Content))
        {
            ModelState.AddModelError("", "Either upload a PDF file or enter content.");
            return RedirectToAction("Documents");
        }

        if (!ModelState.IsValid)
            return RedirectToAction("Documents");

        string content = model.Content ?? "";
        string source = model.Source;

        if (model.UploadedFile != null && model.UploadedFile.Length > 0)
        {
            var ext = Path.GetExtension(model.UploadedFile.FileName).ToLower();
            if (ext != ".pdf" && ext != ".docx")
            {
                TempData["Error"] = "Only PDF and DOCX files are supported for upload.";
                return RedirectToAction("Documents");
            }

            using var stream = model.UploadedFile.OpenReadStream();
            var text = new System.Text.StringBuilder();

            if (ext == ".pdf")
            {
                using var pdf = PdfDocument.Open(stream);
                foreach (var page in pdf.GetPages())
                {
                    text.Append(page.Text);
                }
            }
            else if (ext == ".docx")
            {
                using var doc = WordprocessingDocument.Open(stream, false);
                var body = doc.MainDocumentPart?.Document.Body;
                if (body != null)
                {
                    foreach (var para in body.Elements<Paragraph>())
                    {
                        foreach (var run in para.Elements<Run>())
                        {
                            foreach (var textElement in run.Elements<Text>())
                            {
                                text.Append(textElement.Text);
                            }
                        }
                        text.AppendLine();
                    }
                }
            }

            content = text.ToString();
            source = model.UploadedFile.FileName;
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["Error"] = "No content extracted from PDF or entered manually.";
            return RedirectToAction("Documents");
        }

        await _ragService.AddDocumentAsync(content, source);

        TempData["Message"] = "Document added successfully!";
        return RedirectToAction("Documents");
    }

    // ──────────────────────────────────
    //  POST /Rag/ClearChat → Reset chat
    // ──────────────────────────────────
    [HttpPost]
    public IActionResult ClearChat()
    {
        _chatHistory.Messages.Clear();
        return RedirectToAction("Index");
    }
}