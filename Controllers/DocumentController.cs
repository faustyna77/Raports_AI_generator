using AI_Raports_Generators.Data;
using AI_Raports_Generators.Models;
using AI_Raports_Generators.Models.Domains;
using AI_Raports_Generators.Models.ViewModels;
using AI_Raports_Generators.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json; // Upewnij się, że masz pakiet Newtonsoft.Json


namespace AI_Raports_Generators.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IAIContentGeneratorService _aiService;

        public DocumentController(
            IAIContentGeneratorService aiService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _aiService = aiService;
            _userManager = userManager;
            _context = context;
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

            var user = await _userManager.GetUserAsync(User);
           
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);


            var docsThisMonth = await _context.GeneratedDocuments
                .Where(d => d.UserId == user.Id && d.CreatedAt >= startOfMonth)
                .CountAsync();

            // ✅ Limit dla nie-premium
            if (!user.IsPremium && docsThisMonth >= 5)
            {
               TempData["Error"] = "Limit 5 dokumentów na miesiąc został osiągnięty. Wykup wersję premium.";
                return View(model);

            }

            // ✅ Generowanie
            var result = await _aiService.GenerateContentAsync(model.Prompt);
            model.GeneratedContent = result;

            // ✅ Zapis do bazy
            _context.GeneratedDocuments.Add(new GeneratedDocument
            {
                Title = model.Title ?? "Dokument AI",
                Content = result,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            });

            await _context.SaveChangesAsync();
            // 📤 Po wygenerowaniu i zapisaniu dokumentu, wyślij do webhooka Make.com
            using var httpClient = new HttpClient();

            // 🛡️ Dodaj API Key jeśli jest wymagany w Make.com
            httpClient.DefaultRequestHeaders.Add("x-make-apikey", "my-secret-key");

            var payload = new
            {
                title_doc = model.Title ?? "Dokument AI",
                content_doc = result,
               
                user_email = user.Email, // email zalogowanego użytkownika
                sendToEmail = model.SendOptions.SendToEmail,
                emailToSend = model.SendOptions.Email, // adres wpisany w formularzu
                saveToDrive = model.SendOptions.SaveToDrive,
                googleDriveLink = model.SendOptions.GoogleDriveLink,
                createdAt = DateTime.UtcNow,
                userId = user.Id
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var webhookUrl = "https://hook.eu2.make.com/qdug1uf3v8kxhjpdxctxdgip3m9ac6jp";


            await httpClient.PostAsync(webhookUrl, content);



            return View(model);
        }
    }
}
