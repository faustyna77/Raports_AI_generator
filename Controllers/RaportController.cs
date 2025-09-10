using AI_Raports_Generators.Data;
using AI_Raports_Generators.Models.Domains;
using AI_Raports_Generators.Models.ViewModels;
using AI_Raports_Generators.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AI_Raports_Generators.Controllers
{
    [Authorize]
    public class RaportController : Controller
    {
        private readonly IAIResponseGeneratorService _ai;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RaportController(
            IAIResponseGeneratorService ai,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _ai = ai;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult SpecialRaport()
        {
            var model = new SpecialRaport
            {
                DocumentTypes = GetDocumentTypes()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SpecialRaport(SpecialRaport model, string submit)
        {
            var userId = _userManager.GetUserId(User);

            var documentsThisMonth = await _context.GeneratedDocuments
                .Where(d => d.UserId == userId && d.CreatedAt.Month == DateTime.UtcNow.Month)
                .CountAsync();

            if (documentsThisMonth >= 5)
            {
                TempData["Error"] = "W wersji podstawowej możesz wygenerować maksymalnie 5 dokumentów miesięcznie.";
                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            if (submit == "generate")
            {
                model.GeneratedResponse = await _ai.GenerateResponseAsync(
                    model.Description,
                    model.DocumentType,
                    model.NameOfCompany
                );

                model.DocumentTypes = GetDocumentTypes();
                return View(model);
            }
            else if (submit == "save")
            {
                var document = new GeneratedDocument
                {
                    Title = $"{model.DocumentType} do {model.NameOfCompany}",
                    Content = model.GeneratedResponse,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                _context.GeneratedDocuments.Add(document);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Dokument zapisany!";
                return RedirectToAction("MyDocuments");
            }

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var document = await _context.GeneratedDocuments.FindAsync(id);
            if (document == null) return NotFound();

            var model = new SpecialRaport
            {
                DocumentType = "Inny", // możesz tu ustawić domyślne albo wyciągnąć z dokumentu
                NameOfCompany = document.Title,
                GeneratedResponse = document.Content,
                DocumentTypes = GetDocumentTypes()
            };

            ViewBag.DocumentId = id;
            return View("Test", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, SpecialRaport model)
        {
            var document = await _context.GeneratedDocuments.FindAsync(id);
            if (document == null) return NotFound();

            document.Title = $"{model.DocumentType} do {model.NameOfCompany}";
            document.Content = model.GeneratedResponse;
            document.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Dokument został zaktualizowany!";
            return RedirectToAction("MyDocuments","Raport");
        }

        [HttpGet]
        public async Task<IActionResult> MyDocuments()
        {
            var userId = _userManager.GetUserId(User);

            var documents = await _context.GeneratedDocuments
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            var emails = await _context.GeneratedEmails
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var vm = new MyDocumentsViewModel
            {
                Documents = documents,
                Emails = emails
            };

            return View(vm);
        }

        // ✅ Poprawiona akcja pobierania PDF
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var document = await _context.GeneratedDocuments.FindAsync(id);
            if (document == null) return NotFound();

            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text(document.Title).FontSize(20).Bold();
                    page.Content().Text(document.Content).FontSize(12);
                });
            });

            // Generujemy od razu do byte[]
            var pdfBytes = pdf.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"{document.Title}.pdf");
        }
        [HttpGet]
        public async Task<IActionResult> Print(int id)
        {
            var document = await _context.GeneratedDocuments.FindAsync(id);
            if (document == null) return NotFound();

            return View("Print", document);
        }


        private List<SelectListItem> GetDocumentTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem("Reklamacja", "Reklamacja"),
                new SelectListItem("Podanie", "Podanie"),
                new SelectListItem("Skarga", "Skarga"),
                new SelectListItem("Prośba", "Prośba"),
                new SelectListItem("Oświadczenie", "Oświadczenie"),
                new SelectListItem("Instrukcja", "Instrukcja"),
                new SelectListItem("Zaproszenie", "Zaproszenie"),
                new SelectListItem("Zawiadomienie", "Zawiadomienie"),
                new SelectListItem("Protokół", "Protokół"),
                new SelectListItem("Umowa", "Umowa"),
                new SelectListItem("Decyzja administracyjna", "Decyzja administracyjna"),
                new SelectListItem("Wezwanie", "Wezwanie"),
                new SelectListItem("Rekomendacja", "Rekomendacja")
            };
        }
    }
}
