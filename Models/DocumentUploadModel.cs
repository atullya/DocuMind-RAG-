using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RAGApp.Models
{
    public class DocumentUploadModel
    {
        public string Content { get; set; } = string.Empty;

        public string Source { get; set; } = "manual";

        public IFormFile? UploadedFile { get; set; }
    }
}