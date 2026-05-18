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
}
