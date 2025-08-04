using kebabBackend.Data;
using kebabBackend.Models.DTO;
using kebabBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;

namespace kebabBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly KebabDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly AzureMapsDistanceService _azureMapsDistanceService;
        private readonly IHubContext<OrderHub> _hubContext;

        public PaymentController(
            KebabDbContext context,
            IConfiguration configuration,
            EmailService emailService,
            AzureMapsDistanceService azureMapsDistanceService,
            ILogger<PaymentController> logger,
            IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _azureMapsDistanceService = azureMapsDistanceService;
            _logger = logger;
            _hubContext = hubContext;
        }
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            HttpContext.Request.EnableBuffering();

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            HttpContext.Request.Body.Position = 0; 

            var stripeSignature = Request.Headers["Stripe-Signature"];
            var endpointSecret = _configuration["Stripe:WebhookSecret"];

            _logger.LogInformation("Webhook payload: {Body}", json);
            _logger.LogInformation("Webhook signature: {Signature}", stripeSignature);

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, endpointSecret);

                if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;

                    if (session?.Metadata == null ||
                        !session.Metadata.ContainsKey("cartId") ||
                        !session.Metadata.ContainsKey("address") ||
                        !session.Metadata.ContainsKey("email"))
                    {
                        _logger.LogWarning(" Brakuje wymaganych metadanych w sesji.");
                        return BadRequest("Brakuje danych w metadanych");
                    }

                    var cartIdStr = session.Metadata["cartId"];
                    if (!Guid.TryParse(cartIdStr, out var cartId))
                    {
                        _logger.LogWarning("Nieprawidłowy cartId: {cartId}", cartIdStr);
                        return BadRequest("Nieprawidłowy cartId");
                    }

                    var cart = await _context.carts.FirstOrDefaultAsync(c => c.Id == cartId);

                    if (cart == null)
                    {
                        _logger.LogWarning(" Koszyk {cartId} nie istnieje", cartId);
                        return NotFound("Cart not found");
                    }

                    cart.IsPaid = true;
                    cart.Email = session.Metadata["email"];
                    cart.Address = session.Metadata["address"];
                    cart.IsProcessed = false;

                    _context.Update(cart);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, " Błąd przy zapisie cartu");
                        return StatusCode(500, "Błąd zapisu do bazy danych");
                    }

                    _logger.LogInformation("Cart {cartId} updated as paid and unprocessed", cartId);
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(" Stripe exception: {Message}", ex.Message);
                return BadRequest("Stripe validation failed");
            }
            catch (Exception ex)
            {
                _logger.LogError("General exception: {Message}", ex.Message);
                return StatusCode(500, "Internal error");
            }
        }


        [HttpPost("create-payment-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            if (!Guid.TryParse(request.CartId, out var cartId))
            {
                _logger.LogWarning("Nieprawidłowy cartId.");
                return BadRequest("Nieprawidłowy cartId.");
            }

            var cart = await _context.carts
               .Include(c => c.CartItems)
                   .ThenInclude(ci => ci.MenuItem)
               .Include(c => c.CartItems)
                   .ThenInclude(ci => ci.MeatType)
               .Include(c => c.CartItems)
                   .ThenInclude(ci => ci.Souce)
               .Include(c => c.CartItems)
                   .ThenInclude(ci => ci.ExtraIngredientsLinks)
                       .ThenInclude(link => link.ExtraIngredient)
               .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                _logger.LogWarning("Złe cartId");
                return NotFound("Cart not found");
            }
            if(cart.IsPaid)
            {
                _logger.LogWarning("CartId już opłacone");
                return BadRequest("Cart is arleady paid");
            }
            
            if(cart.Address == "")
            {
                cart.Address = "PickUp";
            }
            else
            {
                if (!await _azureMapsDistanceService.IsWithinDeliveryRangeAsync(request.Address))
                    return BadRequest("Adres poza strefą dostawy (30 km)");
            }

            var cartItemsHtml = string.Join("", cart.CartItems.Select(ci => $@"
              <li>
                Mięso: {ci.MeatType.Name}, Sos: {ci.Souce.Name}<br/>
                Dodatki: {string.Join(", ", ci.ExtraIngredientsLinks.Select(e => e.ExtraIngredient.Name))}<br/>
                Cena: {ci.TotalPrice} zł
              </li>
            "));

            var totalPrice = (long)(cart.Total * 100);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "pln",
                            UnitAmount = totalPrice,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Zamówienie Kebab " + cart.Id.ToString().Substring(0, 8)
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = "https://green-flower-00291e603.2.azurestaticapps.net/success",
                CancelUrl = "https://green-flower-00291e603.2.azurestaticapps.net/cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "cartId", cart.Id.ToString() },
                    { "address", request.Address },
                    { "email", request.Email }
                }
            };

            var service = new SessionService();
            var session = service.Create(options);

            await _emailService.SendHtmlEmail(
                request.Email,
                "Potwierdzenie zamówienia - Kebab King",
                "OrderConfirmation.html",
                new Dictionary<string, string>
                {
                    { "UserEmail", request.Email },
                    { "CartId", cart.Id.ToString() },
                    { "Total", cart.Total.ToString("F2") },
                    { "Address", request.Address },
                    { "CartItems", cartItemsHtml }
                });

            return Ok(new { sessionId = session.Id });
        }
        
    }
}