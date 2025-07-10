using System.Threading.Tasks;

namespace AI_Raports_Generators.Services
{
    public interface IAIContentGeneratorService
    {
        Task<string> GenerateContentAsync(string prompt);
       
    }
}
