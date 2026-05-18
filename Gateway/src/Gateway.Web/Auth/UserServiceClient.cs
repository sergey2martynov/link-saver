namespace Gateway.Web.Auth;

public class UserServiceClient(HttpClient httpClient)
{
    public async Task<Guid> UpsertTelegramUserAsync(long telegramId, string name, CancellationToken ct)
    {
        var response = await httpClient.PostAsJsonAsync(
            "internal/users/upsert-telegram",
            new { TelegramId = telegramId, Name = name },
            ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UpsertResult>(ct);
        return result!.Id;
    }

    private record UpsertResult(Guid Id);
}
