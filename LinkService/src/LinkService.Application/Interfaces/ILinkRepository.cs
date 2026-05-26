using LinkService.Domain.Entities;

namespace LinkService.Application.Ports;

public interface ILinkRepository
{
    Task<Link?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Link> Items, int TotalCount)> GetPagedAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Link link, CancellationToken ct = default);
    Task UpdateAsync(Link link, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
