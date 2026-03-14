using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RAGApp.Models
{
    public class RagRequest
    {
        [Required(ErrorMessage = "Question is required.")]
        [StringLength(1000, ErrorMessage = "Question cannot exceed 1000 characters.")]
        public string Question{get;set;}
    }
}