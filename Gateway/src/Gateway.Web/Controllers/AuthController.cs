using Gateway.Web.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Web.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    TelegramAuthService telegram,
    JwtService jwt,
    UserServiceClient userServiceClient) : ControllerBase
{
    [HttpPost("telegram")]
    public async Task<IActionResult> Telegram([FromBody] TelegramAuthRequest request, CancellationToken ct)
    {
        var telegramUser = telegram.Verify(request.InitData);
        if (telegramUser is null)
            return Unauthorized();

        var name = $"{telegramUser.FirstName} {telegramUser.LastName}".Trim();
        var userId = await userServiceClient.UpsertTelegramUserAsync(telegramUser.Id, name, ct);

        return Ok(new { Token = jwt.Issue(userId) });
    }
}

public record TelegramAuthRequest(string InitData);
