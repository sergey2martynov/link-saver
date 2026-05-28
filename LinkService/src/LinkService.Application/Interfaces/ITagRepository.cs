using LinkService.Domain.Entities;

namespace LinkService.Application.Interfaces;

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetAllByUserAsync(Guid userId, CancellationToken ct = default);
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Tag tag, CancellationToken ct = default);
    Task UpdateAsync(Tag tag, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
