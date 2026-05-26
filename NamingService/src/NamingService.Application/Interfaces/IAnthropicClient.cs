namespace NamingService.Application.Interfaces;

// Port for AI name generation — Infrastructure provides the real HTTP implementation.
public interface IAnthropicClient
{
    Task<string> GenerateLinkNameAsync(string url, CancellationToken ct = default);
}
