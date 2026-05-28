using Dapper;
using LinkService.Application.Interfaces;
using LinkService.Domain.Entities;
using Npgsql;

namespace LinkService.Infrastructure.Repositories;

public class TagRepository(NpgsqlDataSource dataSource) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetAllByUserAsync(Guid userId, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<TagRow>(
            "SELECT id, user_id, name, color, created_at, updated_at FROM tags WHERE user_id = @UserId ORDER BY name",
            new { UserId = userId });
        return rows.Select(ToEntity).ToList();
    }

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        var row = await conn.QuerySingleOrDefaultAsync<TagRow>(
            "SELECT id, user_id, name, color, created_at, updated_at FROM tags WHERE id = @Id",
            new { Id = id });
        return row is null ? null : ToEntity(row);
    }

    public async Task AddAsync(Tag tag, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            "INSERT INTO tags (id, user_id, name, color, created_at, updated_at) VALUES (@Id, @UserId, @Name, @Color, @CreatedAt, @UpdatedAt)",
            ToParams(tag));
    }

    public async Task UpdateAsync(Tag tag, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            "UPDATE tags SET name = @Name, color = @Color, updated_at = @UpdatedAt WHERE id = @Id",
            ToParams(tag));
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync("DELETE FROM tags WHERE id = @Id", new { Id = id });
    }

    private static object ToParams(Tag tag) => new
    {
        tag.Id, tag.UserId, tag.Name, tag.Color, tag.CreatedAt, tag.UpdatedAt
    };

    private static Tag ToEntity(TagRow row) =>
        Tag.Reconstitute(row.Id, row.UserId, row.Name, row.Color, row.CreatedAt, row.UpdatedAt);

    private class TagRow
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Color { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
