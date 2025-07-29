

    using Microsoft.AspNetCore.Mvc;
    using Stripe.Checkout;

    namespace AI_Raports_Generators.Controllers
    {
        public class StripeController : Controller
        {

        [HttpGet]
        public IActionResult Buy()
        {
            return View();
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
                            UnitAmount = 9900, // 99.00 zł
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
                    SuccessUrl = "https://twojadomena.pl/success",
                    CancelUrl = "https://twojadomena.pl/cancel"
                };

                var service = new SessionService();
                Session session = service.Create(options);

                return Redirect(session.Url);
            }
        }
    }


