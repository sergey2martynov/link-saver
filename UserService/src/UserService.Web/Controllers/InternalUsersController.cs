using Microsoft.AspNetCore.Mvc;
using UserService.Application;
using UserService.Application.DTOs;

namespace UserService.Web.Controllers;

[ApiController]
[Route("internal/users")]
public class InternalUsersController(UsersService usersService) : ControllerBase
{
    [HttpPost("upsert-telegram")]
    public async Task<IActionResult> UpsertTelegram(UpsertTelegramUserDto dto, CancellationToken ct)
    {
        var result = await usersService.UpsertTelegramAsync(dto, ct);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterWithPasswordDto dto, CancellationToken ct)
    {
        var result = await usersService.RegisterWithPasswordAsync(dto, ct);
        return Ok(result);
    }

    [HttpPost("validate-credentials")]
    public async Task<IActionResult> ValidateCredentials(ValidateCredentialsDto dto, CancellationToken ct)
    {
        var result = await usersService.ValidateCredentialsAsync(dto, ct);
        if (result is null) return Unauthorized();
        return Ok(result);
    }
}
