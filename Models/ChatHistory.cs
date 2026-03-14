using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAI.Chat;

namespace RAGApp.Models
{
    public class ChatHistory
    {
        public List<ChatMessage> Messages{get;set;}=new List<ChatMessage>();
        public int DocumentCount { get; set; } = 10;
    }
    public class ChatMessage
    {
        public string Role { get; set; } // "user" or "assistant"
        public string Content { get; set; }
        public List<string>? Sources{get;set;}
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}