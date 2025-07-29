using AI_Raports_Generators.Models.ViewModels;
using System;

namespace AI_Raports_Generators.Models.Domains
{
    public class GeneratedDocument
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public byte[]? PdfFile { get; set; } 



    }
}
