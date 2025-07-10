using AI_Raports_Generators.Models;
using AI_Raports_Generators.Models.ViewModels;
using AI_Raports_Generators.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI_Raports_Generators.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly IAIContentGeneratorService _aiService;

        public DocumentController(IAIContentGeneratorService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet]
        public IActionResult Generate()
        {
            return View(new GenerateDocumentViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Generate(GenerateDocumentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _aiService.GenerateContentAsync(model.Prompt);
            model.GeneratedContent = result;

            // tu później dodamy zapis do PDF i do bazy
            return View(model);
        }
    }
}
