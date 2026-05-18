using UserService.Domain.Entities;

namespace UserService.Application.Ports;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
