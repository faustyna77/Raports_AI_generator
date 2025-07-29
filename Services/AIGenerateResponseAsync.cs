using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace AI_Raports_Generators.Services
{
    public class AIResponseGeneratorService : IAIResponseGeneratorService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AIResponseGeneratorService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GenerateResponseAsync(string description, string documentType, string nameOfCompany)
        {
            var prompt = $"Jesteś agentem do tworzenia dokumentów. Napisz oficjalne pismo typu '{documentType}' skierowane do '{nameOfCompany}'. Szczegóły sprawy: {description}. Styl: formalny, jasny, urzędowy. Treści wygenerowane przez ciebie będą oficjalnie w PDF-ach, więc nie dopytuj na końcu, bo wystawiasz końcowy dokument.";

            var apiKey = _configuration["OpenRouter:ApiKey"];

            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(3)
            };

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://twojadomena.pl");
            httpClient.DefaultRequestHeaders.Add("X-Title", "Raport AI Generator");

            var selectedModel = _httpContextAccessor.HttpContext?.Session.GetString("SelectedModel")
                                ?? "moonshotai/kimi-k2:free";

            var body = new
            {
                model = selectedModel,
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
