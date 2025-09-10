using AI_Raports_Generators.Data;
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
using System.Text;
using System.Threading.Tasks;

namespace AI_Raports_Generators.Controllers
{
    [Authorize]
    public class EmailController : Controller
    {
        private readonly IAIEmailGeneratorService _ai;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmailController(
            IAIEmailGeneratorService ai,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _ai = ai;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult SpecialEmail()
        {
            return View(new SpecialEmail());
        }

        [HttpPost]
        public async Task<IActionResult> SpecialEmail(SpecialEmail model, string submit)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            switch (submit)
            {
                case "generate":
                    model.GeneratedEmail = await _ai.GenerateEmailAsync(model.EmailAddress, model.Topic, model.Purpose);
                    return View(model);

                case "save":
                    {
                        var now = DateTime.UtcNow;
                        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                        var emailsThisMonth = await _context.GeneratedEmails
                            .Where(e => e.UserId == user.Id && e.CreatedAt >= startOfMonth)
                            .CountAsync();

                        if (!user.IsPremium && emailsThisMonth >= 5)
                        {
                            TempData["Error"] = "Limit 5 e-maili na miesiąc został osiągnięty. Wykup wersję premium.";
                            return View(model);
                        }

                        var emailRecord = new GeneratedEmail
                        {
                            EmailAddress = model.EmailAddress,
                            Topic = model.Topic,
                            Purpose = model.Purpose,
                            GeneratedContent = model.GeneratedEmail,
                            CreatedAt = DateTime.UtcNow,
                            UserId = user.Id
                        };

                        _context.GeneratedEmails.Add(emailRecord);
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "E-mail zapisany w bazie.";
                        return RedirectToAction("MyDocuments", "Raport"); // 👈 przekierowanie na dokumenty
                    }

                case "sendEmail":
                    {
                        using var httpClient = new HttpClient();
                        httpClient.DefaultRequestHeaders.Add("x-make-apikey", "my-secret-key");

                        var payload = new
                        {
                            to = model.EmailAddress,
                            subject = model.Topic,
                            body = model.GeneratedEmail,
                            user_email = user.Email,
                            createdAt = DateTime.UtcNow
                        };

                        var json = JsonConvert.SerializeObject(payload);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var webhookUrl = "https://hook.eu2.make.com/twoj_webhook";
                        await httpClient.PostAsync(webhookUrl, content);

                        TempData["Success"] = "E-mail został wysłany przez Make.com!";
                        return RedirectToAction("MyDocuments", "Raport"); // 👈 przekierowanie na dokumenty
                    }

                default:
                    return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditEmail(int id)
        {
            var email = await _context.GeneratedEmails.FindAsync(id);
            if (email == null) return NotFound();

            var model = new SpecialEmail
            {
                EmailAddress = email.EmailAddress,
                Topic = email.Topic,
                Purpose = email.Purpose,
                GeneratedEmail = email.GeneratedContent
            };

            ViewBag.EmailId = id;
            return View("SpecialEmail", model); // ten sam widok do edycji
        }

        [HttpPost]
        public async Task<IActionResult> EditEmail(int id, SpecialEmail model)
        {
            var email = await _context.GeneratedEmails.FindAsync(id);
            if (email == null) return NotFound();

            email.EmailAddress = model.EmailAddress;
            email.Topic = model.Topic;
            email.Purpose = model.Purpose;
            email.GeneratedContent = model.GeneratedEmail;
            email.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "E-mail zaktualizowany!";
            return RedirectToAction("MyDocuments", "Raport"); // 👈 przekierowanie na dokumenty
        }

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

        [HttpPost]
        public async Task<IActionResult> SendEmail(int id)
        {
            var email = await _context.GeneratedEmails.FindAsync(id);
            if (email == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-make-apikey", "my-secret-key");

            var payload = new
            {
                to = email.EmailAddress,
                subject = email.Topic,
                body = email.GeneratedContent,
                user_email = user.Email,
                createdAt = DateTime.UtcNow
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var webhookUrl = "https://hook.eu2.make.com/twoj_webhook";
            await httpClient.PostAsync(webhookUrl, content);

            TempData["Success"] = "E-mail został wysłany przez Make.com!";
            return RedirectToAction("MyDocuments", "Raport"); // 👈 przekierowanie na dokumenty
        }
    }
}
