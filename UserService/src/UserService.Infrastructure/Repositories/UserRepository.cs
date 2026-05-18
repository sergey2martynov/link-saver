using Dapper;
using Npgsql;
using UserService.Application.Ports;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Repositories;

public class UserRepository(NpgsqlDataSource dataSource) : IUserRepository
{
    private const string SelectColumns = "id, email, name, telegram_id, created_at, updated_at";

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        using var multi = await conn.QueryMultipleAsync(
            $"""
            SELECT COUNT(*) FROM users;
            SELECT {SelectColumns} FROM users ORDER BY created_at DESC LIMIT @PageSize OFFSET @Offset
            """,
            new { PageSize = pageSize, Offset = (page - 1) * pageSize });

        var totalCount = await multi.ReadSingleAsync<int>();
        var rows = await multi.ReadAsync<UserRow>();
        return (rows.Select(ToEntity).ToList(), totalCount);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        var row = await conn.QuerySingleOrDefaultAsync<UserRow>(
            $"SELECT {SelectColumns} FROM users WHERE id = @Id",
            new { Id = id });
        return row is null ? null : ToEntity(row);
    }

    public async Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        var row = await conn.QuerySingleOrDefaultAsync<UserRow>(
            $"SELECT {SelectColumns} FROM users WHERE telegram_id = @TelegramId",
            new { TelegramId = telegramId });
        return row is null ? null : ToEntity(row);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            "INSERT INTO users (id, email, name, telegram_id, created_at, updated_at) VALUES (@Id, @Email, @Name, @TelegramId, @CreatedAt, @UpdatedAt)",
            ToParams(user));
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE users SET email = @Email, name = @Name, updated_at = @UpdatedAt WHERE id = @Id",
            ToParams(user));
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync("DELETE FROM users WHERE id = @Id", new { Id = id });
    }

    private static object ToParams(User user) => new
    {
        user.Id, user.Email, user.Name, user.TelegramId, user.CreatedAt, user.UpdatedAt
    };

    private static User ToEntity(UserRow row) =>
        User.Reconstitute(row.Id, row.Email, row.Name, row.TelegramId, row.CreatedAt, row.UpdatedAt);

    private class UserRow
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string Name { get; set; } = null!;
        public long? TelegramId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
