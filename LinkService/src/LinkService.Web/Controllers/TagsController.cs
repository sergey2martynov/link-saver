using LinkService.Application.DTOs;
using LinkService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinkService.Web.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController(TagsService tagsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return Ok(await tagsService.GetAllAsync(userId, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTagDto dto, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var result = await tagsService.CreateAsync(userId, dto, ct);
        return Created($"/api/tags/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTagDto dto, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var result = await tagsService.UpdateAsync(userId, id, dto, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        await tagsService.DeleteAsync(userId, id, ct);
        return NoContent();
    }

    private bool TryGetUserId(out Guid userId)
    {
        var header = Request.Headers["X-User-Id"].FirstOrDefault();
        return Guid.TryParse(header, out userId);
    }
}
