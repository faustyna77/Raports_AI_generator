using AI_Raports_Generators.Data;
using AI_Raports_Generators.Models.Domains;
using AI_Raports_Generators.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DocumentsApiController : ControllerBase
{
    private readonly IAIResponseGeneratorService _ai;
    private readonly ApplicationDbContext _context;

    public DocumentsApiController(IAIResponseGeneratorService ai, ApplicationDbContext context)
    {
        _ai = ai;
        _context = context;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateRequestDto request)
    {
        var result = await _ai.GenerateResponseAsync(request.Description, request.DocumentType, request.NameOfCompany);

        // (Opcjonalnie) zapis do bazy
        var doc = new GeneratedDocument
        {
            Title = $"{request.DocumentType} do {request.NameOfCompany}",
            Content = result,
            CreatedAt = DateTime.UtcNow,
            UserId = null // z Make.com nie będzie usera
        };

        _context.GeneratedDocuments.Add(doc);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            result,
            title = doc.Title
        });
    }
}

public class GenerateRequestDto
{
    public string DocumentType { get; set; }
    public string NameOfCompany { get; set; }
    public string Description { get; set; }
}
