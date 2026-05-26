using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NamingService.Application.Interfaces;

namespace NamingService.Infrastructure;

// Calls Anthropic's Messages API to generate a short title from a URL.
// Uses claude-haiku for low latency and minimal cost (~$0.00024 per link).
// API reference: https://docs.anthropic.com/en/api/messages
public class AnthropicClient(IHttpClientFactory httpClientFactory, string apiKey, ILogger<AnthropicClient> logger)
    : IAnthropicClient
{
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";

    // claude-haiku-4-5: fastest and cheapest model, more than sufficient for title generation
    private const string Model = "claude-haiku-4-5-20251001";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<string> GenerateLinkNameAsync(string url, CancellationToken ct = default)
    {
        var requestBody = new MessagesRequest
        {
            Model = Model,
            // 60 tokens is more than enough for a short title
            MaxTokens = 60,
            Messages = [new RequestMessage { Role = "user", Content = BuildPrompt(url) }]
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        // Anthropic uses x-api-key header instead of Authorization: Bearer
        httpRequest.Headers.Add("x-api-key", apiKey);
        // Anthropic requires this versioning header on every request
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");
        httpRequest.Content = JsonContent.Create(requestBody, options: JsonOptions);

        var client = httpClientFactory.CreateClient();
        using var response = await client.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<MessagesResponse>(JsonOptions, ct);
        var text = body?.Content.FirstOrDefault(b => b.Type == "text")?.Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            logger.LogWarning("Empty name returned by Anthropic for URL {Url} — falling back to URL", url);
            return url;
        }

        return text.Trim();
    }

    private static string BuildPrompt(string url) =>
        $"Generate a short, descriptive title (max 60 characters) for this URL. Reply with only the title text — no quotes, no trailing punctuation, no explanation.\n\nURL: {url}";
}

// ── JSON models matching the Anthropic Messages API shape ────────────────────

file sealed class MessagesRequest
{
    [JsonPropertyName("model")] public string Model { get; init; } = null!;
    [JsonPropertyName("max_tokens")] public int MaxTokens { get; init; }
    [JsonPropertyName("messages")] public RequestMessage[] Messages { get; init; } = [];
}

file sealed class RequestMessage
{
    [JsonPropertyName("role")] public string Role { get; init; } = null!;
    [JsonPropertyName("content")] public string Content { get; init; } = null!;
}

file sealed class MessagesResponse
{
    [JsonPropertyName("content")] public ContentBlock[] Content { get; init; } = [];
}

file sealed class ContentBlock
{
    [JsonPropertyName("type")] public string Type { get; init; } = null!;
    [JsonPropertyName("text")] public string Text { get; init; } = null!;
}
