using LinkService.Application.Interfaces;
using LinkService.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LinkService.Web.Notifications;

public class HubLinkNotifier(IHubContext<LinksHub> hubContext) : ILinkNotifier
{
    public Task NotifyLinkNamedAsync(Guid linkId, string name, Guid userId, CancellationToken ct = default) =>
        hubContext.Clients.Group(userId.ToString())
            .SendAsync("LinkNamed", linkId, name, ct);
}
