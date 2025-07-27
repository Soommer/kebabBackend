using kebabBackend.Models.DTO;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace kebabBackend.Services
{
    public class OrderHub : Hub
    {
        public async Task NotifyNewOrder(object order)
        {
            await Clients.All.SendAsync("NewOrder", order);
        }
    }
}