using AI_Raports_Generators.Models.Domains;

namespace AI_Raports_Generators.Models.ViewModels
{
    public class MyDocumentsViewModel
    {
        public List<GeneratedDocument> Documents { get; set; } = new();
        public List<GeneratedEmail> Emails { get; set; } = new();
    }
}
