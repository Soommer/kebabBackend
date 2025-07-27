using kebabBackend.Models.DTO;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace kebabBackend.Services
{
    public class OrderHub : Hub
    {
        private readonly ILogger<OrderHub> _logger;

        public OrderHub(ILogger<OrderHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("🟢 SignalR: client connected ({ConnectionId})", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogWarning("🔴 SignalR: client disconnected ({ConnectionId}), reason: {Reason}",
                Context.ConnectionId, exception?.Message ?? "none");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNewOrderAsync(CartResponse order)
        {
            try
            {
                _logger.LogInformation("📡 Wysyłam NewOrder do klientów (cartId: {CartId})", order.Id);
                await _hubContext.Clients.All.SendAsync("NewOrder", order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Nie udało się wysłać NewOrder przez SignalR (cartId: {CartId})", order.Id);
            }
        }
    }
}
