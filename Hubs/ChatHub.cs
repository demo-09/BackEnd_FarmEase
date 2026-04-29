using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;

public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var email = Context.GetHttpContext()?.Request.Query["userEmail"];

        if (!string.IsNullOrEmpty(email))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, email!);

            // ?? notify others
            await Clients.All.SendAsync("UserOnline", email.ToString());
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var email = Context.GetHttpContext()?.Request.Query["userEmail"];

        if (!string.IsNullOrEmpty(email))
        {
            await Clients.All.SendAsync("UserOffline", email.ToString());
        }

        await base.OnDisconnectedAsync(exception);
    }
}