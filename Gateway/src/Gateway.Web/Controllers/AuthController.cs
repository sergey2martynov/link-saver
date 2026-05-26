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
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var userId = await userServiceClient.RegisterAsync(request.Email, request.Name, request.Password, ct);
        return Ok(new { Token = jwt.Issue(userId) });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var userId = await userServiceClient.ValidateCredentialsAsync(request.Email, request.Password, ct);
        if (userId is null)
            return Unauthorized();
        return Ok(new { Token = jwt.Issue(userId.Value) });
    }

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

public record RegisterRequest(string Email, string Name, string Password);
public record LoginRequest(string Email, string Password);
public record TelegramAuthRequest(string InitData);
