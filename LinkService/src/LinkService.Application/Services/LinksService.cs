using LinkService.Application.DTOs;
using LinkService.Application.Exceptions;
using LinkService.Application.Ports;
using LinkService.Domain.Entities;

namespace LinkService.Application;

public class LinksService(ILinkRepository repository)
{
    private const int PageSize = 10;

    public async Task<PagedResult<LinkDto>> GetPagedAsync(int page, CancellationToken ct = default)
    {
        var (items, totalCount) = await repository.GetPagedAsync(page, PageSize, ct);
        return new PagedResult<LinkDto>(items.Select(ToDto).ToList(), page, PageSize, totalCount);
    }

    public async Task<LinkDto> CreateAsync(CreateLinkDto dto, CancellationToken ct = default)
    {
        var link = Link.Create(dto.Url, dto.Title, dto.Description);

        if (dto.Tags is { Count: > 0 })
            link.SetTags(dto.Tags);

        await repository.AddAsync(link, ct);

        return ToDto(link);
    }

    public async Task<LinkDto> UpdateAsync(Guid id, UpdateLinkDto dto, CancellationToken ct = default)
    {
        var link = await repository.GetByIdAsync(id, ct)
            ?? throw new LinkNotFoundException(id);

        link.Update(dto.Url, dto.Title, dto.Description);

        if (dto.Tags is not null)
            link.SetTags(dto.Tags);

        await repository.UpdateAsync(link, ct);

        return ToDto(link);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var link = await repository.GetByIdAsync(id, ct)
            ?? throw new LinkNotFoundException(id);

        await repository.DeleteAsync(link.Id, ct);
    }

    private static LinkDto ToDto(Link link) =>
        new(link.Id, link.Url, link.Title, link.Description, link.Tags, link.CreatedAt, link.UpdatedAt);
}
