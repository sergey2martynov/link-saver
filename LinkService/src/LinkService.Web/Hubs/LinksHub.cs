using Microsoft.AspNetCore.SignalR;

namespace LinkService.Web.Hubs;

// SignalR hub for real-time link updates.
// On connect, the client is added to a user-scoped group using the X-User-Id header
// that Gateway forwards after validating the JWT.
public class LinksHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Headers["X-User-Id"].FirstOrDefault();
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        await base.OnConnectedAsync();
    }
}
