namespace LinkService.Application.Interfaces;

// Port for real-time browser push notifications.
// The Web layer provides the SignalR implementation; Application stays decoupled from transport.
public interface ILinkNotifier
{
    Task NotifyLinkNamedAsync(Guid linkId, string name, Guid userId, CancellationToken ct = default);
}
