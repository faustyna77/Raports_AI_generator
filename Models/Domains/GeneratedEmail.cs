using System;

namespace AI_Raports_Generators.Models.Domains
{
    public class GeneratedEmail
    {
        public int Id { get; set; } // <- To musi być klucz główny
        public string EmailAddress { get; set; } = null!;
        public string Topic { get; set; } = null!;
        public string Purpose { get; set; } = null!;
        public string GeneratedContent { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public DateTime? UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
