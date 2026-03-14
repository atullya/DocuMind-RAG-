using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAGApp.Models
{
    public class Document
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public float[] Embedding { get; set; }
        public DateTime AddedAt { get; set; }
        public string Source { get; set; }
    }
}