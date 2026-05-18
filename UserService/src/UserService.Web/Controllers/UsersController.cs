using Microsoft.AspNetCore.Mvc;
using UserService.Application;
using UserService.Application.DTOs;

namespace UserService.Web.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(UsersService usersService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, CancellationToken ct = default)
    {
        if (page < 1) return BadRequest("Page must be >= 1.");
        return Ok(await usersService.GetPagedAsync(page, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto, CancellationToken ct)
    {
        var result = await usersService.CreateAsync(dto, ct);
        return Created($"/api/users/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserDto dto, CancellationToken ct)
    {
        var result = await usersService.UpdateAsync(id, dto, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await usersService.DeleteAsync(id, ct);
        return NoContent();
    }
}
