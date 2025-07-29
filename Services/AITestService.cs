using System.Text;
using System.Text.Json;

namespace AI_Raports_Generators.Services
{
    public class AITestService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AITestService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenRouter:ApiKey"];

            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }
        }

       


                public async Task<string> GenerateReportAsync(string input, string model)
        {
            var prompt = $"Wygeneruj profesjonalne sprawozdanie na podstawie poniższych danych:\n{input}";

            var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    model = model,
                    messages = new[]
                    {
                new { role = "user", content = prompt }
                    }
                }),
                Encoding.UTF8, "application/json"
            );

            var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }

    }
}
