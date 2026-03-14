using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAGApp.Models
{
    public class RagResponse
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public List<Document> RetrievedDocuments { get; set; }
        public double ProcessingTimeMs { get; set; }
        public bool Success { get; set; } = true;
        public string ErrorMessage { get; set; }


    }
}