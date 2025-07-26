using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kebabBackend.Data;
using kebabBackend.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;

namespace kebabBackend.Services
{
    public class CartProcessingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CartProcessingService> _logger;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly EmailService _emailService;

        public CartProcessingService(
            IServiceProvider serviceProvider,
            ILogger<CartProcessingService> logger,
            IHubContext<OrderHub> hubContext,
            EmailService emailService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _hubContext = hubContext;
            _emailService = emailService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("✅ CartProcessingService uruchomiony.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<KebabDbContext>();

                    var cartsToProcess = await db.carts
                        .Where(c => c.IsPaid && !c.IsProcessed)
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.MenuItem)
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.MeatType)
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.Souce)
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.ExtraIngredientsLinks)
                                .ThenInclude(link => link.ExtraIngredient)
                        .ToListAsync(stoppingToken);

                    foreach (var cart in cartsToProcess)
                    {
                        _logger.LogInformation("🚚 Przetwarzanie zamówienia: {CartId}", cart.Id);

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
                                ExtraNames = ci.ExtraIngredientsLinks.Select(e => e.ExtraIngredient.Name).ToList(),
                                Size = ci.Size,
                                TotalPrice = ci.TotalPrice
                            }).ToList()
                        };

                        await _hubContext.Clients.All.SendAsync("NewOrder", response, stoppingToken);
                        _logger.LogInformation("📡 Wysłano SignalR NewOrder dla koszyka: {CartId}", cart.Id);

                        await _emailService.SendHtmlEmail(
                            cart.Email,
                            "Potwierdzenie płatności - Kebab King",
                            "PaymentConfirmation.html",
                            new Dictionary<string, string>
                            {
                                { "UserEmail", cart.Email },
                                { "CartId", cart.Id.ToString() }
                            }
                        );
                        _logger.LogInformation("📨 Wysłano e-mail do: {Email}", cart.Email);

                        cart.IsProcessed = true;
                        db.carts.Update(cart);
                    }

                    if (cartsToProcess.Count > 0)
                        await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Błąd w CartProcessingService");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            _logger.LogInformation("🛑 CartProcessingService zatrzymany.");
        }
    }
}
