using AI_Raports_Generators.Models.ViewModels;
using AI_Raports_Generators.Services;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AI_Raports_Generators.Controllers
{
    public class EmailController : Controller
    {
        private readonly IAIEmailGeneratorService _ai;

        public EmailController(IAIEmailGeneratorService ai)
        {
            _ai = ai;
        }

        [HttpGet]
        public IActionResult SpecialEmail()
        {
            return View(new SpecialEmail());
        }


        [HttpPost]
        public async Task<IActionResult> SpecialEmail(SpecialEmail model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var prompt = $"Napisz oficjalne pismo typu '{model.EmailAddress}' skierowane do '{model.Topic}'. Szczegóły sprawy: {model.Topic}. Styl: formalny, jasny, urzędowy.";

            var result = await _ai.GenerateEmailAsync(model.EmailAddress, model.Topic, model.Purpose);

            model.GeneratedEmail = result;

            return View(model);
        }



    }
}
