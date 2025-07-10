using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AI_Raports_Generators.Services
{
    public class AIContentGeneratorService : IAIContentGeneratorService
    {
        private readonly IConfiguration _configuration;

        public AIContentGeneratorService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GenerateContentAsync(string prompt)
        {
            var apiKey = _configuration["OpenRouter:ApiKey"];

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://twojadomena.pl"); // wymagane
            httpClient.DefaultRequestHeaders.Add("X-Title", "Raport AI Generator");

            var body = new
            {
                model = "openrouter/cypher-alpha:free", // możesz zmienić np. na mistralai/mixtral-8x7b
                messages = new[]
                {
            new { role = "user", content = prompt }
        }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
          
            Console.WriteLine("Response JSON: " + json);

            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString();
        }

    }
}
