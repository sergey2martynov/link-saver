using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Gateway.Web.Auth;

public class TelegramAuthService(IConfiguration configuration)
{
    private readonly byte[] _secretKey = ComputeSecretKey(configuration["Telegram:BotToken"]!);

    public TelegramUser? Verify(string initData)
    {
        var fields = HttpUtility.ParseQueryString(initData);
        var hash = fields["hash"];
        if (hash is null) return null;

        fields.Remove("hash");

        var dataCheckString = string.Join("\n",
            fields.AllKeys
                .Order()
                .Select(k => $"{k}={fields[k]}"));

        var expected = HMACSHA256.HashData(_secretKey, Encoding.UTF8.GetBytes(dataCheckString));
        if (!Convert.ToHexString(expected).Equals(hash, StringComparison.OrdinalIgnoreCase))
            return null;

        var userJson = fields["user"];
        return userJson is null ? null : JsonSerializer.Deserialize<TelegramUser>(userJson);
    }

    private static byte[] ComputeSecretKey(string botToken) =>
        HMACSHA256.HashData(Encoding.UTF8.GetBytes("WebAppData"), Encoding.UTF8.GetBytes(botToken));
}

public record TelegramUser(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("first_name")] string FirstName,
    [property: JsonPropertyName("last_name")] string? LastName,
    [property: JsonPropertyName("username")] string? Username);
