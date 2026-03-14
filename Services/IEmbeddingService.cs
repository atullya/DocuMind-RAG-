using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAGApp.Services
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string text);
    }
}