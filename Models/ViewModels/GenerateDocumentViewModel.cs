namespace AI_Raports_Generators.Models.ViewModels
{
    public class GenerateDocumentViewModel
    {
        public string Prompt { get; set; } // co użytkownik wpisuje
        public string? GeneratedContent { get; set; } // wynik z AI (opcjonalny)
    }
}
