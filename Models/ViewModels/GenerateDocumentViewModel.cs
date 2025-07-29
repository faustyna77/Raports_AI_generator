using System.ComponentModel.DataAnnotations;

namespace AI_Raports_Generators.Models.ViewModels
{
    public class GenerateDocumentViewModel
    {
        [Required]
        public string Prompt { get; set; }

        public string? Title { get; set; }

        public string? GeneratedContent { get; set; }
        public SendOptionsViewModel SendOptions { get; set; } = new();
    }
}
