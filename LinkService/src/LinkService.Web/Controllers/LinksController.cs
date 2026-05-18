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
        if (page < 1) return BadRequest("Page must be >= 1.");
        return Ok(await linkService.GetPagedAsync(page, ct));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLinkDto dto, CancellationToken ct)
    {
        var result = await linkService.CreateAsync(dto, ct);
        return Created($"/api/links/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateLinkDto dto, CancellationToken ct)
    {
        var result = await linkService.UpdateAsync(id, dto, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await linkService.DeleteAsync(id, ct);
        return NoContent();
    }
}
