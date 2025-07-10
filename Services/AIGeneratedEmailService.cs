using System.Text;
using System.Text.Json;

namespace AI_Raports_Generators.Services
{
    public class AIGeneratedEmailService:IAIEmailGeneratorService
    {

        private readonly IConfiguration _configuration;

        public AIGeneratedEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public async Task<string> GenerateEmailAsync(string emailAddress, string topic, string purpose)
        {
            var prompt = $" Jesteś agentem do tworzenia służbowych emaili.Napisz oficjalny mail, który jest do  {emailAddress} i, którego temat to {topic}, a sprawa w której piszesz dotyczy: {purpose}. Tresci wygenerowane przez ciebie beda oficjalnie wysyłane w mailach  wiec nie dopytuj na końcu bo wystawiasz końcowy dokument";

            var apiKey = _configuration["OpenRouter:ApiKey"];

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://twojadomena.pl");
            httpClient.DefaultRequestHeaders.Add("X-Title", "Raport AI Generator");

            var body = new
            {
                model = "openrouter/cypher-alpha:free",
                messages = new[]
                {
                new { role = "user", content = prompt }
            }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response JSON: " + json);

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString();
        }
    }
}
