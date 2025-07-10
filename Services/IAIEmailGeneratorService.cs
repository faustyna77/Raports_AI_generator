using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace AI_Raports_Generators.Services
{
    public interface IAIEmailGeneratorService
    {
        Task<string> GenerateEmailAsync(string emailAddress, string topic, string purpose);
    }
}

