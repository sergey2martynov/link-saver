namespace NamingService.Application.Events;

// Published to 'links.named' after Claude generates a title for the link.
public record LinkNamedEvent(Guid LinkId, string Name);
