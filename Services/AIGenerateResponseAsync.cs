using AI_Raports_Generators.Services;
using System.Text;
using System.Text.Json;

public class AIResponseGeneratorService : IAIResponseGeneratorService
{
    private readonly IConfiguration _configuration;

    public AIResponseGeneratorService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> GenerateResponseAsync(string description, string documentType, string nameOfCompany)
    {
        var prompt = $" Jesteś agentem do tworzenia dokumentów.Napisz oficjalne pismo typu '{documentType}' skierowane do '{nameOfCompany}'. Szczegóły sprawy: {description}. Styl: formalny, jasny, urzędowy.Itresci wygenerowane przez ciebie beda oficjalnie w pdfach wiec nie dopytuj na końcu bo wystawiasz końcowy dokument";

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
