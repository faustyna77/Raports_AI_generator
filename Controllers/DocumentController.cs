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
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        // Widok generowania dokumentu
        [HttpGet]
        public IActionResult Generate()
        {
            return View(new GenerateDocumentViewModel());
        }

        // Obsługa generowania i zapisu dokumentu
        [HttpPost]
        public async Task<IActionResult> Generate(GenerateDocumentViewModel model, string submit)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!ModelState.IsValid) return View(model);

            switch (submit)
            {
                case "generate":
                    model.GeneratedContent = await _aiService.GenerateContentAsync(model.Prompt);
                    return View(model);

                case "save":
                    var now = DateTime.UtcNow;
                    var startOfMonth = new DateTime(now.Year, now.Month, 1);
                    var docsThisMonth = await _context.GeneratedDocuments
                        .Where(d => d.UserId == user.Id && d.CreatedAt >= startOfMonth)
                        .CountAsync();

                    if (!user.IsPremium && docsThisMonth >= 5)
                    {
                        TempData["Error"] = "Limit 5 dokumentów na miesiąc został osiągnięty. Wykup wersję premium.";
                        return View(model);
                    }

                    var document = new GeneratedDocument
                    {
                        Title = model.Title ?? "Dokument AI",
                        Content = model.GeneratedContent,
                        CreatedAt = DateTime.UtcNow,
                        UserId = user.Id
                    };

                    _context.GeneratedDocuments.Add(document);
                    await _context.SaveChangesAsync();

                    // Wysyłka e-mail / Google Drive (Make.com)
                    if (model.SendOptions != null && model.SendOptions.SendToEmail)
                    {
                        using var httpClient = new HttpClient();
                        httpClient.DefaultRequestHeaders.Add("x-make-apikey", "my-secret-key");

                        var payload = new
                        {
                            title_doc = document.Title,
                            content_doc = document.Content,
                            user_email = user.Email,
                            emailToSend = model.SendOptions.Email,
                            saveToDrive = model.SendOptions.SaveToDrive,
                            googleDriveLink = model.SendOptions.GoogleDriveLink,
                            createdAt = document.CreatedAt,
                            userId = user.Id
                        };

                        var json = JsonConvert.SerializeObject(payload);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var webhookUrl = "https://hook.eu2.make.com/qdug1uf3v8kxhjpdxctxdgip3m9ac6jp";
                        await httpClient.PostAsync(webhookUrl, content);
                    }

                    TempData["Success"] = "Dokument zapisany!";
                    return RedirectToAction("MyDocuments");

                default:
                    return View(model);
            }
        }

        // Edycja dokumentu
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var document = await _context.GeneratedDocuments.FindAsync(id);
            if (document == null) return NotFound();

            var model = new GenerateDocumentViewModel
            {
                Title = document.Title,
                Prompt = document.Title,
                GeneratedContent = document.Content
            };

            ViewBag.DocumentId = id;
            return View("Generate", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, GenerateDocumentViewModel model)
        {
            var document = await _context.GeneratedDocuments.FindAsync(id);
            if (document == null) return NotFound();

            document.Title = model.Title;
            document.Content = model.GeneratedContent;
            document.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Dokument zaktualizowany!";
            return RedirectToAction("MyDocuments");
        }

        // Widok wszystkich dokumentów i e-maili
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

            // **Zawsze przekazujemy MyDocumentsViewModel**
            var vm = new MyDocumentsViewModel
            {
                Documents = documents,
                Emails = emails
            };

            return View(vm);
        }

        // Pobranie dokumentu jako PDF
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var document = await _context.GeneratedDocuments.FindAsync(id);
            if (document == null) return NotFound();

            var stream = new MemoryStream();
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

        // Usuwanie dokumentu
        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var doc = await _context.GeneratedDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            _context.GeneratedDocuments.Remove(doc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Dokument został usunięty.";
            return RedirectToAction("MyDocuments");
        }

        // Usuwanie e-maila
        [HttpPost]
        public async Task<IActionResult> DeleteEmail(int id)
        {
            var email = await _context.GeneratedEmails.FindAsync(id);
            if (email == null) return NotFound();

            _context.GeneratedEmails.Remove(email);
            await _context.SaveChangesAsync();

            TempData["Success"] = "E-mail został usunięty.";
            return RedirectToAction("MyDocuments");
        }

        // (Opcjonalnie) podgląd do drukowania e-maila
        [HttpGet]
        public async Task<IActionResult> DownloadEmailPdf(int id)
        {
            var email = await _context.GeneratedEmails.FindAsync(id);
            if (email == null) return NotFound();

            var stream = new MemoryStream();
            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text(email.Topic).FontSize(20).Bold();
                    page.Content().Text(email.GeneratedContent).FontSize(12);
                });
            });

            pdf.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"{email.Topic}.pdf");
        }
    }
}
