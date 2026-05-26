using LinkService.Application.DTOs;
using LinkService.Application.Events;
using LinkService.Application.Exceptions;
using LinkService.Application.Interfaces;
using LinkService.Application.Ports;
using LinkService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LinkService.Application;

public class LinksService(
    ILinkRepository repository,
    ILinkDomainRepository domainRepository,
    ILinkEventPublisher eventPublisher,
    ILinkNotifier notifier,
    ILogger<LinksService> logger)
{
    private const int PageSize = 10;

    public async Task<PagedResult<LinkDto>> GetPagedAsync(Guid userId, int page, CancellationToken ct = default)
    {
        var (items, totalCount) = await repository.GetPagedAsync(userId, page, PageSize, ct);
        return new PagedResult<LinkDto>(items.Select(ToDto).ToList(), page, PageSize, totalCount);
    }

    public async Task<LinkDto> CreateAsync(Guid userId, CreateLinkDto dto, CancellationToken ct = default)
    {
        var link = Link.Create(userId, dto.Url, dto.Title, dto.Description);

        if (dto.Tags is { Count: > 0 })
            link.SetTags(dto.Tags);

        var domain = new Uri(dto.Url).Host;
        var linkDomain = await domainRepository.FindByDomainAsync(domain, ct);
        if (linkDomain is not null && linkDomain.Tags.Count > 0)
            link.SetSuggestedTags(linkDomain.Tags);

        await repository.AddAsync(link, ct);

        // Publish domain event — the background publisher returns immediately (fire-and-forget).
        await eventPublisher.PublishLinkCreatedAsync(
            new LinkCreatedEvent(Guid.NewGuid(), DateTime.UtcNow, link.Id, userId, link.Url, link.Title, link.Tags.ToList()), ct);

        return ToDto(link);
    }

    public async Task<LinkDto> UpdateAsync(Guid userId, Guid id, UpdateLinkDto dto, CancellationToken ct = default)
    {
        var link = await repository.GetByIdAsync(id, ct)
            ?? throw new LinkNotFoundException(id);

        if (link.UserId != userId)
            throw new LinkNotFoundException(id);

        link.Update(dto.Url, dto.Title, dto.Description);

        if (dto.Tags is not null)
            link.SetTags(dto.Tags);

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

        // Publish domain event after successful deletion — fire-and-forget.
        await eventPublisher.PublishLinkDeletedAsync(
            new LinkDeletedEvent(Guid.NewGuid(), DateTime.UtcNow, link.Id, link.UserId), ct);
    }

    // Called by the Kafka consumer worker when NamingService publishes 'links.named'.
    // Applies the AI-generated title and pushes a real-time update to the user's browser.
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

    // Called by the Kafka consumer worker when ClassifierService publishes 'links.classified'.
    // Stores tags as suggestions — the user must confirm before they become final.
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

    public async Task<LinkDto> ConfirmTagsAsync(Guid userId, Guid linkId, CancellationToken ct = default)
    {
        var link = await repository.GetByIdAsync(linkId, ct)
            ?? throw new LinkNotFoundException(linkId);

        if (link.UserId != userId)
            throw new LinkNotFoundException(linkId);

        link.ConfirmTags();
        await repository.UpdateAsync(link, ct);
        return ToDto(link);
    }

    private static LinkDto ToDto(Link link) =>
        new(link.Id, link.Url, link.Title, link.Description, link.Tags, link.SuggestedTags, link.CreatedAt, link.UpdatedAt);
}
