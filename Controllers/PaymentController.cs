using Azure;
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

        public PaymentController(KebabDbContext context,
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

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];
            var endpointSecret = _configuration["Stripe:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, endpointSecret);

                if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session?.Metadata == null || !session.Metadata.ContainsKey("cartId") || !session.Metadata.ContainsKey("address") || !session.Metadata.ContainsKey("email"))
                    {
                        _logger.LogWarning("Brak cartId");
                        return BadRequest("cartId missing");
                    }

                    var cartIdStr = session.Metadata["cartId"];
                    if (!Guid.TryParse(cartIdStr, out var cartId))
                    {
                        _logger.LogWarning("Złe cartId");
                        return BadRequest("cartId invalid");
                    }

                    var addressStr = session.Metadata["address"];

                    var emailStr = session.Metadata["email"];



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

                    //_logger.LogInformation("---KOSZYK---\n{cart}", JsonConvert.SerializeObject(cart, Formatting.Indented));

                    if (cart == null)
                    {
                        _logger.LogWarning("Brak cartu");
                        return NotFound("Cart not found");
                    }

                    cart.IsPaid = true;
                    cart.Email = emailStr;
                    cart.Address = addressStr;

                    var response = new CartResponse
                    {
                        Id = cart.Id,
                        Total = cart.Total,
                        CreatedAt = cart.CreatedAt,
                        IsFinished = cart.IsFinished,
                        CartItems = cart.CartItems.Select(ci => new CartItemInCartResponse
                        {
                            Id = ci.Id,
                            MenuItemName = ci.MenuItem.Name,
                            MeatName = ci.MeatType.Name,
                            SouceName = ci.Souce.Name,
                            ExtraNames = ci.ExtraIngredientsLinks
                                            .Select(link => link.ExtraIngredient.Name)
                                            .ToList(),
                            Size = ci.Size,
                            TotalPrice = ci.TotalPrice
                        }).ToList()
                    };

                    // Signall


                    await _hubContext.Clients.All.SendAsync("NewOrder", response);

                    await _emailService.SendHtmlEmail(
                        emailStr,
                        "Potwierdzenie płatności - Kebab King",
                        "PaymentConfirmation.html",
                        new Dictionary<string, string>
                        {
                            { "UserEmail", emailStr },
                            { "CartId", cart.Id.ToString() }
                        }
                    );


                    _context.Entry(cart).Property(c => c.IsPaid).IsModified = true;
                    await _context.SaveChangesAsync();


                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError($"StripeException: {ex.Message}");
                return BadRequest("Stripe validation failed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd ogólny: {ex.Message}");
                return StatusCode(500, "Internal error");
            }
        }

        [HttpPost("create-payment-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutRequest request)
        {
            await _hubContext.Clients.All.SendAsync("NewOrder", "response");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            Guid CartId = Guid.Parse(request.CartId);
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
               .FirstOrDefaultAsync(c => c.Id == CartId);

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
            } else
            {
                if (!await _azureMapsDistanceService.IsWithinDeliveryRangeAsync(request.Address))
                {
                    _logger.LogWarning("Address znajduję się poza sferą dostawy");
                    return BadRequest("Address znajduję się poza sferą dostawy (30 km)");
                }
            }

            var cartItemsHtml = string.Join("", cart.CartItems.Select(ci => $@"
              <li>
                Mięso: {ci.MeatType.Name}, Sos: {ci.Souce.Name}<br/>
                Dodatki: {string.Join(", ", ci.ExtraIngredientsLinks.Select(e => e.ExtraIngredient.Name))}<br/>
                Cena: {ci.TotalPrice} zł
              </li>
            "));

            var totalPrice = (long)(cart.Total * 100);

            var option = new SessionCreateOptions
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
                                Name = "Zamówienie Kebab" + cart.Id.ToString().Substring(0,8),
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
                    { "email", request.Email },
                }
            };

            var service = new SessionService();
            var session = service.Create(option);


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
                }
            );

            return Ok(new { sessionId = session.Id });
        }
        
    }
}
