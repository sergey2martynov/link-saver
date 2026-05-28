using LinkService.Application.DTOs;
using LinkService.Application.Exceptions;
using LinkService.Application.Interfaces;
using LinkService.Domain.Entities;

namespace LinkService.Application.Services;

public class TagsService(ITagRepository repository)
{
    public async Task<IReadOnlyList<TagDto>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var tags = await repository.GetAllByUserAsync(userId, ct);
        return tags.Select(ToDto).ToList();
    }

    public async Task<TagDto> CreateAsync(Guid userId, CreateTagDto dto, CancellationToken ct = default)
    {
        var tag = Tag.Create(userId, dto.Name, dto.Color, dto.StartDate, dto.EndDate);
        await repository.AddAsync(tag, ct);
        return ToDto(tag);
    }

    public async Task<TagDto> UpdateAsync(Guid userId, Guid id, UpdateTagDto dto, CancellationToken ct = default)
    {
        var tag = await repository.GetByIdAsync(id, ct) ?? throw new TagNotFoundException(id);
        if (tag.UserId != userId) throw new TagNotFoundException(id);

        tag.Update(dto.Name, dto.Color, dto.StartDate, dto.EndDate);
        await repository.UpdateAsync(tag, ct);
        return ToDto(tag);
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var tag = await repository.GetByIdAsync(id, ct) ?? throw new TagNotFoundException(id);
        if (tag.UserId != userId) throw new TagNotFoundException(id);

        await repository.DeleteAsync(id, ct);
    }

    internal static TagDto ToDto(Tag tag) =>
        new(tag.Id, tag.Name, tag.Color, tag.StartDate, tag.EndDate, tag.CreatedAt, tag.UpdatedAt);
}
