using AI_Raports_Generators.Data;
using AI_Raports_Generators.Models.Domains;
using AI_Raports_Generators.Models.ViewModels;
using AI_Raports_Generators.Services;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AI_Raports_Generators.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        private readonly AITestService _aiTestService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TestController(AITestService aiTestService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _aiTestService = aiTestService;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Test() => View();
        [HttpPost]
        public async Task<IActionResult> GenerateReport(string postTitle, string hashtags, int wordCount, double temperature)
        {
            var user = await _userManager.GetUserAsync(User);

            // Sprawdzenie limitu 5 dokumentów w bieżącym miesiącu
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var docsThisMonth = await _context.GeneratedDocuments
                .Where(d => d.UserId == user.Id && d.CreatedAt >= startOfMonth)
                .CountAsync();

            if (!user.IsPremium && docsThisMonth >= 5)
            {
                TempData["Error"] = "Limit 5 dokumentów na miesiąc został osiągnięty.";
                return RedirectToAction("Index");
            }

            // Generowanie posta przez AI
            var aiReport = await _aiTestService.GenerateReportAsync(
                input: "",
                temperature: temperature,
                wordCount: wordCount,
                hashtags: hashtags,
                postTitle: postTitle
            );

            // **Nie zapisujemy jeszcze w bazie**
            ViewBag.AIReport = aiReport;
            ViewBag.PostTitle = postTitle;
            return View("Test");
        }


        [HttpPost]
        public async Task<IActionResult> SaveEditedReport(string editedReport, string postTitle)
        {
            var user = await _userManager.GetUserAsync(User);

            // Zapis do bazy
            _context.GeneratedDocuments.Add(new GeneratedDocument
            {
                Title = postTitle,
                Content = editedReport,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            });
            await _context.SaveChangesAsync();

            // Po zapisaniu przekierowanie do widoku MyDocuments w folderze Report
            var documents = await _context.GeneratedDocuments
                .Where(d => d.UserId == user.Id)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            var emails = await _context.GeneratedEmails
                .Where(e => e.UserId == user.Id)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var vm = new MyDocumentsViewModel
            {
                Documents = documents,
                Emails = emails
            };

            return RedirectToAction("MyDocuments", "Raport");
        }
        [HttpGet]
        public async Task<IActionResult> EditPost(int id)
        {
            var post = await _context.GeneratedDocuments.FindAsync(id);
            if (post == null) return NotFound();

            var model = new EditPostViewModel
            {
                Title = post.Title,
                Content = post.Content
            };

            return View("EditPost", model); // nowy widok
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(int id, EditPostViewModel model)
        {
            var post = await _context.GeneratedDocuments.FindAsync(id);
            if (post == null) return NotFound();

            post.Title = model.Title;
            post.Content = model.Content;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Post zaktualizowany!";

            return RedirectToAction("MyDocuments", "Raport"); // lub Raport, tam gdzie jest widok
        }


        [HttpPost]
        public IActionResult SaveReportAsPdf(string editedReport)
        {
            var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "arial.ttf");
            if (!System.IO.File.Exists(fontPath))
                throw new FileNotFoundException("Nie znaleziono pliku czcionki TTF.", fontPath);

            var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new iText.Layout.Document(pdf);


            document.SetFont(font);
            document.Add(new Paragraph(editedReport));
            document.Close();

            var pdfBytes = ms.ToArray();
            return File(pdfBytes, "application/pdf", "post.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> MyDocuments()
        {
            var user = await _userManager.GetUserAsync(User);

            var documents = await _context.GeneratedDocuments
                .Where(d => d.UserId == user.Id)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            var emails = await _context.GeneratedEmails
                .Where(e => e.UserId == user.Id)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var vm = new MyDocumentsViewModel
            {
                Documents = documents,
                Emails = emails
            };

            return RedirectToAction("MyDocuments", "Raport");

        }
    }
}
