using LinkService.Application.DTOs;
using LinkService.Application;
using Microsoft.AspNetCore.Mvc;

namespace LinkService.Web.Controllers;

[ApiController]
[Route("api/links")]
public class LinksController(LinksService linkService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, CancellationToken ct = default)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (page < 1) return BadRequest("Page must be >= 1.");
        return Ok(await linkService.GetPagedAsync(userId, page, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLinkDto dto, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var result = await linkService.CreateAsync(userId, dto, ct);
        return Created($"/api/links/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateLinkDto dto, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var result = await linkService.UpdateAsync(userId, id, dto, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/tags/confirm")]
    public async Task<IActionResult> ConfirmTags(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var result = await linkService.ConfirmTagsAsync(userId, id, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        await linkService.DeleteAsync(userId, id, ct);
        return NoContent();
    }

    private bool TryGetUserId(out Guid userId)
    {
        var header = Request.Headers["X-User-Id"].FirstOrDefault();
        return Guid.TryParse(header, out userId);
    }
}
