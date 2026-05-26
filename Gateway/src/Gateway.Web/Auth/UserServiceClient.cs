using System.Net;

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
        var result = await response.Content.ReadFromJsonAsync<UserResult>(ct);
        return result!.Id;
    }

    public async Task<Guid> RegisterAsync(string email, string name, string password, CancellationToken ct)
    {
        var response = await httpClient.PostAsJsonAsync(
            "internal/users/register",
            new { Email = email, Name = name, Password = password },
            ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UserResult>(ct);
        return result!.Id;
    }

    public async Task<Guid?> ValidateCredentialsAsync(string email, string password, CancellationToken ct)
    {
        var response = await httpClient.PostAsJsonAsync(
            "internal/users/validate-credentials",
            new { Email = email, Password = password },
            ct);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return null;
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UserResult>(ct);
        return result!.Id;
    }

    private record UserResult(Guid Id);
}
