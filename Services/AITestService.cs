using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace AI_Raports_Generators.Services
{
    public class AITestService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AITestService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GenerateReportAsync(
            string input,
            double temperature = 0.7,
            int wordCount = 500,
            string? hashtags = null,
            string? postTitle = null)
        {
            var apiKey = _configuration["Google:ApiKey"];
            var selectedModel = _httpContextAccessor.HttpContext?.Session.GetString("SelectedModel")
                                ?? "gemini-2.0-flash";

            var prompt = $"Napisz profesjonalny post na bloga na temat: \"{postTitle}\".\n" +
                         $"Treść powinna zawierać około {wordCount} słów.\n" +
                         (!string.IsNullOrEmpty(hashtags) ? $"Dodaj na końcu hashtagi: {hashtags}.\n" : "") +
                         $"Dodatkowe informacje: {input}.\n" +
                         "Post powinien być spójny, atrakcyjny i wartościowy dla czytelnika.";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = temperature,
                    topP = 0.95
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{selectedModel}:generateContent?key={apiKey}";

            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };

            HttpResponseMessage response;
            try
            {
                response = await httpClient.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Błąd połączenia z Gemini API: " + ex.Message);
                return string.Empty;
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("🌐 Response JSON: " + json);

            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("⚠️ Odpowiedź API była pusta.");
                return string.Empty;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0 &&
                    candidates[0].TryGetProperty("content", out var contentElement) &&
                    contentElement.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0 &&
                    parts[0].TryGetProperty("text", out var textElement))
                {
                    return textElement.GetString() ?? string.Empty;
                }

                Console.WriteLine("⚠️ Nie znaleziono wygenerowanego tekstu w odpowiedzi API.");
            }
            catch (JsonException jex)
            {
                Console.WriteLine("❌ Błąd parsowania JSON: " + jex.Message);
            }

            return string.Empty;
        }
    }
}
