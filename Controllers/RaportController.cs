using AI_Raports_Generators.Data;
using AI_Raports_Generators.Data; // jeśli ApplicationDbContext tam się znajduje
using AI_Raports_Generators.Models.Domains;
using AI_Raports_Generators.Models.ViewModels;
using AI_Raports_Generators.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using QuestPDF.Infrastructure;


namespace AI_Raports_Generators.Controllers
{
    public class RaportController : Controller
    {
        
        private readonly IAIResponseGeneratorService _ai;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public RaportController(IAIResponseGeneratorService ai,
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
        {
            _ai = ai;
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var document = await _context.GeneratedDocuments.FindAsync(id);
            if (document == null)
                return NotFound();

            var stream = new MemoryStream();
            var userId = _userManager.GetUserId(User);
            var count = await _context.GeneratedDocuments
                .Where(d => d.UserId == userId && d.CreatedAt.Month == DateTime.UtcNow.Month)
                .CountAsync();


            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text(document.Title).FontSize(20).Bold();
                    page.Content().Text(document.Content).FontSize(12);
                });
            });

            pdf.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"{document.Title}.pdf");
        }


        [HttpGet]
        public IActionResult SpecialRaport()
        {
            var model = new SpecialRaport
            {
                DocumentTypes = new List<SelectListItem>
        {
            new SelectListItem("Reklamacja", "Reklamacja"),
            new SelectListItem("Podanie", "Podanie"),
            new SelectListItem("Skarga", "Skarga")
        }
            };
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> MyDocuments()
        {
            var userId = _userManager.GetUserId(User);
            var documents = await _context.GeneratedDocuments
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(documents);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var document = await _context.GeneratedDocuments.FindAsync(id);
            if (document == null) return NotFound();

            var model = new SpecialRaport
            {
                DocumentType = document.Title.Split(' ')[0],
                NameOfCompany = document.Title.Split("do").LastOrDefault()?.Trim(),
                Description = "", // opcjonalnie
                GeneratedResponse = document.Content
            };

            ViewBag.DocumentId = id;
            return View("EditRaport", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, SpecialRaport model)
        {
            var document = await _context.GeneratedDocuments.FindAsync(id);
            if (document == null) return NotFound();

            document.Content = model.GeneratedResponse ?? "";
            document.Title = $"{model.DocumentType} do {model.NameOfCompany}";
            document.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction("MyDocuments");
        }



        [HttpPost]
        public async Task<IActionResult> SpecialRaport(SpecialRaport model, string submit)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (submit == "generate")
            {
                var result = await _ai.GenerateResponseAsync(model.Description, model.DocumentType, model.NameOfCompany);
                model.GeneratedResponse = result;

                // Zachowaj listę rozwijaną
                model.DocumentTypes = new List<SelectListItem>
        {
            new SelectListItem("Reklamacja", "Reklamacja"),
            new SelectListItem("Podanie", "Podanie"),
            new SelectListItem("Skarga", "Skarga")
        };
                var userId = _userManager.GetUserId(User);
                var count = await _context.GeneratedDocuments
                    .Where(d => d.UserId == userId && d.CreatedAt.Month == DateTime.UtcNow.Month)
                    .CountAsync();

                // Pobierz liczbę dokumentów użytkownika w danym miesiącu
                var documentCount = await _context.GeneratedDocuments
                    .Where(d => d.UserId == userId && d.CreatedAt.Month == DateTime.UtcNow.Month)
                    .CountAsync();

                // Przykładowy warunek dla użytkowników bez premium (tu zakładamy, że nie mają pola "IsPremium")
                if (documentCount >= 5)
                {
                    ModelState.AddModelError("", "W wersji podstawowej możesz wygenerować maksymalnie 5 dokumentów miesięcznie. Zaktualizuj konto, aby korzystać bez limitu.");

                    // Przywrócenie listy rozwijanej, aby widok się nie zepsuł
                    model.DocumentTypes = new List<SelectListItem>
    {
        new SelectListItem("Reklamacja", "Reklamacja"),
        new SelectListItem("Podanie", "Podanie"),
        new SelectListItem("Skarga", "Skarga")
    };

                    return View(model);
                }

                return View(model); // wyświetl z tekstem do edycji
            }
            else if (submit == "save")
            {
                var document = new GeneratedDocument
                {
                    Title = $"{model.DocumentType} do {model.NameOfCompany}",
                    Content = model.GeneratedResponse,
                    CreatedAt = DateTime.UtcNow,
                    UserId = _userManager.GetUserId(User)
                };

                _context.GeneratedDocuments.Add(document);
                await _context.SaveChangesAsync();
                using var httpClient = new HttpClient();

                // 🛡️ Dodaj API Key jeśli jest wymagany w Make.com
                httpClient.DefaultRequestHeaders.Add("x-make-apikey", "my-secret-key");

                var payload = new
                {
                    title_raport = document.Title,
                    content_raport = document.Content,
                   
                    sendToEmail_fromRaport = model.SendOptions.SendToEmail,
                    emailToSend_fromRaport = model.SendOptions.Email,
                    saveToDrive_fromRaport = model.SendOptions.SaveToDrive,

                    googleDriveLink_fromRaport = model.SendOptions.GoogleDriveLink,
                    createdAt_Raport = DateTime.UtcNow
                   
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var webhookUrl = "https://hook.eu2.make.com/qdug1uf3v8kxhjpdxctxdgip3m9ac6jp";


                await httpClient.PostAsync(webhookUrl, content);



                return RedirectToAction("MyDocuments");
            }

            return View(model);
        }




    }


}
