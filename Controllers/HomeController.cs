using AI_Raports_Generators.Models;
using AI_Raports_Generators.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Diagnostics;

namespace AI_Raports_Generators.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        
        public IActionResult Index()
        {
            var models = new List<string>
    {
        "openai/gpt-4",
        "openai/gpt-3.5-turbo",
        "mistralai/mistral-small-3.2-24b-instruct",
        "meta-llama/llama-3-8b-instruct"
    };

            return View(models); // ← ważne: przekazanie modelu
        }


        public IActionResult Privacy()
        {
            return View();
        }
        [Authorize]
        public IActionResult Wyswietl(string texts)

        {
            
            return View((object) texts);
        }
        [HttpPost]
        public IActionResult Form(Form model)
        {
            return View(model);
        }

        [HttpGet]
        public IActionResult Form()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult SetModel(string selectedModel)
        {
            HttpContext.Session.SetString("SelectedModel", selectedModel);
            return RedirectToAction("Index"); // lub np. RedirectToAction("Form")
        }

        public IActionResult Models()
        {
            var models = new List<string>
    {
        "mistralai/mistral-small-3.2-24b-instruct",
        "openai/gpt-3.5-turbo",
        "openai/gpt-4",
        "meta-llama/llama-3-70b-instruct"
    };

            return View(models);
        }


        [HttpPost]
        public IActionResult CreateCheckoutSession()
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 9900, // 99.00 PLN
                            Currency = "pln",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Dostęp do AI Generatora"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = "https://localhost:7149/success",
                CancelUrl = "https://localhost:7149/cancel"
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Redirect(session.Url);
        }
    }
}
