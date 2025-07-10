using AI_Raports_Generators.Models.ViewModels;
using AI_Raports_Generators.Services;
using Microsoft.AspNetCore.Mvc;

namespace AI_Raports_Generators.Controllers
{
    public class RaportController : Controller
    {
        
        private readonly IAIResponseGeneratorService _ai;

        public RaportController(IAIResponseGeneratorService ai)
        {
            _ai = ai;
        }

        [HttpGet]
        public IActionResult SpecialRaport()
        {
            return View(new SpecialRaport());
        }

        [HttpPost]
        public async Task<IActionResult> SpecialRaport(SpecialRaport model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var prompt = $"Napisz oficjalne pismo typu '{model.DocumentType}' skierowane do '{model.NameOfCompany}'. Szczegóły sprawy: {model.Description}. Styl: formalny, jasny, urzędowy.";
          
            var result = await _ai.GenerateResponseAsync(model.Description,model.DocumentType,model.NameOfCompany);

            model.GeneratedResponse = result;

            return View(model);
        }

    }


}
