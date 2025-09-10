using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace AI_Raports_Generators.Services
{
    public class AIContentGeneratorService : IAIContentGeneratorService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AIContentGeneratorService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GenerateContentAsync(string promptText)
        {
            var apiKey = _configuration["Google:ApiKey"];

            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };

            // Pobranie modelu z sesji lub ustawienie domyślnego
            var selectedModel = _httpContextAccessor.HttpContext?.Session.GetString("SelectedModel")
                                ?? "gemini-2.0-flash";

            // Body requestu do Gemini API
            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text =  $"Jesteś profesjonalnym generatorem dokumentów. Wygeneruj profesjonalne pismo na temat:{promptText} nie zadawaj żadnych pytań ani sugestii na początku ani na końcu gdyż to wszystko co wygenerujesz trafia jako odpowiedż końcowa " }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topP = 0.95
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{selectedModel}:generateContent?key={apiKey}";

            HttpResponseMessage response;
            try
            {
                response = await httpClient.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas wywołania Gemini API: " + ex.Message);
                return string.Empty;
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Response JSON: " + json);

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
                    return textElement.GetString();
                }

                Console.WriteLine("Nie znaleziono wygenerowanego tekstu w odpowiedzi API.");
            }
            catch (JsonException jex)
            {
                Console.WriteLine("Błąd parsowania JSON: " + jex.Message);
            }

            return string.Empty;
        }
    }
}
