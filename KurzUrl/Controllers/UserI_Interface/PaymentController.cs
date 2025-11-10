using Microsoft.AspNetCore;
using Stripe;
using Microsoft.AspNetCore.Mvc;
using KurzUrl.Data.Dto;
using Stripe.Checkout;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;

namespace KurzUrl.Controllers.UserI_Interface
{
    
    [Route("api/[controller]")]
    [ApiController]
    
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentController(IConfiguration config)
        {
            _config = config;
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        }

        [HttpPost("checkout")]
        public IActionResult CreateCheckoutSession(CreateCheckoutDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(errors);
            }

                var options = new SessionCreateOptions
            {

                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions {PriceData = new SessionLineItemPriceDataOptions{ UnitAmount = dto.AmountInCents, Currency = dto.Currency, ProductData = new SessionLineItemPriceDataProductDataOptions{ Name = dto.Name}}, Quantity = 1}
                },
                SuccessUrl = dto.SuccessUrl,
                CancelUrl = dto.CancelUrl,
            };
            var service = new SessionService();
            var session = service.Create(options);
            return Ok(new { sesionId = session.Id, url = session.Url });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebHook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], "PUT THE SECRET KEY HERE"); //TO BE DONE: write the endpoint secret of the webhook

                if(stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                    //TO BE DONE: Update database
                }

                return Ok();
            }

            catch(StripeException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
