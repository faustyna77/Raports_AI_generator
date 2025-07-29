using AI_Raports_Generators.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Tesseract;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using AI_Raports_Generators.Data;
using AI_Raports_Generators.Models;
using AI_Raports_Generators.Models.Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace AI_Raports_Generators.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        private readonly AITestService _aiTestService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public TestController(
          AITestService aiTestService,
          UserManager<ApplicationUser> userManager,
          ApplicationDbContext context)
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
        public async Task<IActionResult> SaveEditedReport(string editedReport, List<IFormFile> images)
        {
            var user = await _userManager.GetUserAsync(User);

            // Zbuduj zawartość PDF — tekst + obrazy
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);

            doc.Add(new iText.Layout.Element.Paragraph(editedReport));

            foreach (var image in images)
            {
                using var imgStream = image.OpenReadStream();
                var imgData = iText.IO.Image.ImageDataFactory.Create(await ToByteArrayAsync(imgStream));
                var img = new iText.Layout.Element.Image(imgData).ScaleToFit(500, 500).SetMarginTop(10);
                doc.Add(img);
            }

            doc.Close();

            var pdfBytes = ms.ToArray();

            // Zapisz do bazy (jako tekst + PDF w bajtach)
            var newDoc = new GeneratedDocument
            {
                Title = "Edytowany raport",
                Content = editedReport,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id,
                PdfFile = pdfBytes // ← musisz dodać to pole w modelu!
            };

            _context.GeneratedDocuments.Add(newDoc);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyDocuments"); // zakładam, że tak się nazywa twoja lista
        }




        [HttpPost]
        public async Task<IActionResult> GenerateReport(IFormFile instructionFile, List<IFormFile> attachmentFiles)
        {
            var user = await _userManager.GetUserAsync(User);
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

            string instructionText = "";
            if (instructionFile != null)
            {
                using var stream = instructionFile.OpenReadStream();
                instructionText = (instructionFile.ContentType.Contains("pdf") || instructionFile.FileName.EndsWith(".pdf"))
                    ? ExtractTextFromPdf(stream)
                    : await RunOcrAsync(stream);
            }

            var attachmentTexts = new List<string>();
            foreach (var file in attachmentFiles)
            {
                using var stream = file.OpenReadStream();
                attachmentTexts.Add(await RunOcrAsync(stream));
            }

            var combinedText = instructionText + "\n\nZAŁĄCZNIKI:\n" + string.Join("\n", attachmentTexts);
           
            var model = HttpContext.Session.GetString("SelectedModel") ?? "mistralai/mistral-small-3.2-24b-instruct";
            var aiReport = await _aiTestService.GenerateReportAsync(combinedText, model);

            // Zapis do bazy
            _context.GeneratedDocuments.Add(new GeneratedDocument
            {
                Title = "Raport AI",
                Content = aiReport,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            });
            await _context.SaveChangesAsync();

            // Webhook (opcjonalnie)
            
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(900)
            };

            httpClient.DefaultRequestHeaders.Add("x-make-apikey", "my-secret-key");

            var payload = new
            {
                title_doc = "Raport AI",
                content_doc = aiReport,
                user_email = user.Email,
                createdAt = DateTime.UtcNow,
                userId = user.Id
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await httpClient.PostAsync("https://hook.eu2.make.com/qdug1uf3v8kxhjpdxctxdgip3m9ac6jp", content);

            ViewBag.AIReport = aiReport;
            return View("ReportResult");
        }


        private string ExtractTextFromPdf(Stream pdfStream)
        {
            var sb = new StringBuilder();
            using var reader = new PdfReader(pdfStream);
            using var pdf = new PdfDocument(reader);

            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                var page = pdf.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                var text = PdfTextExtractor.GetTextFromPage(page, strategy);
                sb.AppendLine(text);
            }

            return sb.ToString();
        }
        [HttpPost]
        public IActionResult SaveReportAsPdf(string editedReport)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new iText.Layout.Document(pdf);
            doc.Add(new iText.Layout.Element.Paragraph(editedReport));
            doc.Close();

            var pdfBytes = ms.ToArray();
            return File(pdfBytes, "application/pdf", "Sprawozdanie.pdf");
        }

        private async Task<string> RunOcrAsync(Stream imageStream)
        {
            using var engine = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
            using var img = Pix.LoadFromMemory(await ToByteArrayAsync(imageStream));
            using var page = engine.Process(img);
            return page.GetText();
        }

        private async Task<byte[]> ToByteArrayAsync(Stream input)
        {
            using var ms = new MemoryStream();
            await input.CopyToAsync(ms);
            return ms.ToArray();
        }


        [HttpGet]
        public async Task<IActionResult> MyDocuments()
        {
            var user = await _userManager.GetUserAsync(User);

            var documents = await _context.GeneratedDocuments
                .Where(d => d.UserId == user.Id)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(documents);
        }

    }
}
