using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs;

public class BackendHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
        Console.WriteLine($"Signal received: {message}. Refreshing UI data...");
    }
}