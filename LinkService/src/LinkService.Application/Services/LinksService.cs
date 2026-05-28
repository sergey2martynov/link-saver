using LinkService.Application.DTOs;
using LinkService.Application.Events;
using LinkService.Application.Exceptions;
using LinkService.Application.Interfaces;
using LinkService.Application.Ports;
using LinkService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LinkService.Application.Services;

public class LinksService(
    ILinkRepository repository,
    ILinkDomainRepository domainRepository,
    ILinkEventPublisher eventPublisher,
    ILinkNotifier notifier,
    ILogger<LinksService> logger)
{
    private const int PageSize = 10;

    public async Task<PagedResult<LinkDto>> GetPagedAsync(Guid userId, int page, LinkFilterDto? filter = null, CancellationToken ct = default)
    {
        var (items, totalCount) = await repository.GetPagedAsync(userId, page, PageSize, filter, ct);
        return new PagedResult<LinkDto>(items.Select(ToDto).ToList(), page, PageSize, totalCount);
    }

    public async Task<LinkDto> CreateAsync(Guid userId, CreateLinkDto dto, CancellationToken ct = default)
    {
        var link = Link.Create(userId, dto.Url, dto.Title);

        if (dto.TagIds is { Count: > 0 })
            link.SetTagIds(dto.TagIds);

        var domain = new Uri(dto.Url).Host;
        var linkDomain = await domainRepository.FindByDomainAsync(domain, ct);
        if (linkDomain is not null && linkDomain.Tags.Count > 0)
            link.SetSuggestedTags(linkDomain.Tags);

        await repository.AddAsync(link, ct);

        await eventPublisher.PublishLinkCreatedAsync(
            new LinkCreatedEvent(Guid.NewGuid(), DateTime.UtcNow, link.Id, userId, link.Url, link.Title, []), ct);

        return ToDto(link);
    }

    public async Task<LinkDto> UpdateAsync(Guid userId, Guid id, UpdateLinkDto dto, CancellationToken ct = default)
    {
        var link = await repository.GetByIdAsync(id, ct)
            ?? throw new LinkNotFoundException(id);

        if (link.UserId != userId)
            throw new LinkNotFoundException(id);

        link.Update(dto.Url, dto.Title);

        if (dto.TagIds is not null)
            link.SetTagIds(dto.TagIds);

        await repository.UpdateAsync(link, ct);

        return ToDto(link);
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var link = await repository.GetByIdAsync(id, ct)
            ?? throw new LinkNotFoundException(id);

        if (link.UserId != userId)
            throw new LinkNotFoundException(id);

        await repository.DeleteAsync(link.Id, ct);

        await eventPublisher.PublishLinkDeletedAsync(
            new LinkDeletedEvent(Guid.NewGuid(), DateTime.UtcNow, link.Id, link.UserId), ct);
    }

    // Called by the Kafka consumer when NamingService publishes 'links.named'.
    public async Task ApplyNameAsync(Guid linkId, string name, CancellationToken ct = default)
    {
        var link = await repository.GetByIdAsync(linkId, ct);
        if (link is null)
        {
            logger.LogWarning("Name received for unknown link {LinkId} — ignoring.", linkId);
            return;
        }

        link.SetTitle(name);
        await repository.UpdateAsync(link, ct);
        await notifier.NotifyLinkNamedAsync(link.Id, name, link.UserId, ct);
    }

    // Called by the Kafka consumer when ClassifierService publishes 'links.classified'.
    public async Task ApplySuggestedTagsAsync(Guid linkId, string[] tags, CancellationToken ct = default)
    {
        var link = await repository.GetByIdAsync(linkId, ct);
        if (link is null)
        {
            logger.LogWarning("Suggested tags received for unknown link {LinkId} — ignoring.", linkId);
            return;
        }

        link.SetSuggestedTags(tags);
        await repository.UpdateAsync(link, ct);
    }

    public async Task<LinkDto> DismissSuggestionsAsync(Guid userId, Guid linkId, CancellationToken ct = default)
    {
        var link = await repository.GetByIdAsync(linkId, ct)
            ?? throw new LinkNotFoundException(linkId);

        if (link.UserId != userId)
            throw new LinkNotFoundException(linkId);

        link.ClearSuggestedTags();
        await repository.UpdateAsync(link, ct);
        return ToDto(link);
    }

    private static LinkDto ToDto(Link link) =>
        new(link.Id, link.Url, link.Title,
            link.Tags.Select(TagsService.ToDto).ToList(),
            link.SuggestedTags,
            link.CreatedAt, link.UpdatedAt);
}
