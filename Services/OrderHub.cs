using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace kebabBackend.Services
{
    public class OrderHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"🟢 SignalR: client connected ({Context.ConnectionId})");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"🔴 SignalR: client disconnected ({Context.ConnectionId})");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendOrderUpdate(string orderId)
        {
            await Clients.All.SendAsync("Update", orderId);
        }
    }
}