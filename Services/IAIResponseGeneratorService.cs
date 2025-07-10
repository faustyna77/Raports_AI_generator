
namespace AI_Raports_Generators.Services
{
    public interface IAIResponseGeneratorService
    {
        Task<string> GenerateResponseAsync(string description, string documentType, string nameOfCompany);
    }
}
