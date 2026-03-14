# 🤖 RAG Document Chat App

A simple **Retrieval-Augmented Generation (RAG)** web application that lets you upload PDF/DOCX documents and chat with them using AI. Built with ASP.NET Core and Google's Gemini.

## ✨ Features

- 📄 **Upload Documents**: Support for PDF and DOCX files
- 💬 **AI Chat**: Ask questions about your documents
- 🔍 **Smart Search**: Uses embeddings to find relevant information
- 🎨 **Clean UI**: Black & white theme inspired by modern chat interfaces
- 🚀 **Local Processing**: Embeddings generated locally (no API costs)

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core MVC (.NET 10)
- **Frontend**: Bootstrap, HTML/CSS
- **AI**: Google Gemini 2.5 Flash (LLM)
- **Embeddings**: Local embeddings (no external API)
- **Vector Search**: Cosine similarity
- **Document Processing**: PdfPig (PDF), DocumentFormat.OpenXml (DOCX)

## 🚀 Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Google Gemini API key (free tier available)

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/yourusername/rag-document-chat.git
   cd rag-document-chat
   ```

2. **Configure API Key**
   - Get a free API key from [Google AI Studio](https://makersuite.google.com/app/apikey)
   - Update `appsettings.json`:

   ```json
   {
     "GeminiSettings": {
       "ApiKey": "your-api-key-here",
       "ChatModel": "gemini-2.5-flash"
     }
   }
   ```

3. **Run the application**

   ```bash
   dotnet build
   dotnet run
   ```

4. **Open in browser**
   - Navigate to `https://localhost:5001`
   - Upload PDF/DOCX files
   - Start chatting!

## 📖 How It Works

### RAG Process

1. **Upload**: Documents are processed and split into chunks
2. **Embed**: Text chunks converted to vectors using local embeddings
3. **Store**: Vectors stored in memory for fast retrieval
4. **Query**: User questions are embedded and matched against document vectors
5. **Generate**: Top matching chunks sent to Gemini AI for answer generation

### Architecture

```
User Question → Embedding → Vector Search → LLM Context → AI Answer
```

## 📁 Project Structure

```
RAGApp/
├── Controllers/          # MVC Controllers
├── Models/              # Data models
├── Services/            # Business logic (Embedding, LLM, RAG)
├── Views/               # Razor views
├── wwwroot/             # Static files (CSS, JS)
├── appsettings.json     # Configuration
└── Program.cs           # Application entry point
```

## 🎯 Usage Example

1. **Upload a document** (e.g., your resume.pdf)
2. **Ask questions**:
   - "What are my key skills?"
   - "Summarize my work experience"
   - "What projects have I worked on?"

The AI will provide answers based only on your uploaded documents!

## 🔧 Configuration

### Environment Variables (Optional)

Instead of `appsettings.json`, use environment variables:

```bash
export GeminiSettings__ApiKey="your-key"
export GeminiSettings__ChatModel="gemini-2.5-flash"
```

### Vector Search Settings

Adjust in `appsettings.json`:

```json
{
  "RagSettings": {
    "TopK": 3 // Number of document chunks to retrieve
  }
}
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Google Gemini](https://ai.google.dev/) for the AI models
- [PdfPig](https://github.com/UglyToad/PdfPig) for PDF processing
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) for the web framework

## 📞 Support

If you find this helpful, give it a ⭐ on GitHub!

---

**Made with ❤️ for AI enthusiasts and document lovers**</content>
<parameter name="filePath">/home/user/Desktop/RAG/RAGApp/README.md
