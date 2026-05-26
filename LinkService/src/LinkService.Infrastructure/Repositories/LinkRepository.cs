using Dapper;
using LinkService.Application.Ports;
using LinkService.Domain.Entities;
using Npgsql;

namespace LinkService.Infrastructure.Repositories;

public class LinkRepository(NpgsqlDataSource dataSource) : ILinkRepository
{
    private const string SelectColumns =
        "id, user_id, url, title, description, tags, suggested_tags, created_at, updated_at";

    public async Task<(IReadOnlyList<Link> Items, int TotalCount)> GetPagedAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        using var multi = await conn.QueryMultipleAsync(
            $"""
            SELECT COUNT(*) FROM links WHERE user_id = @UserId;
            SELECT {SelectColumns} FROM links WHERE user_id = @UserId ORDER BY created_at DESC LIMIT @PageSize OFFSET @Offset
            """,
            new { UserId = userId, PageSize = pageSize, Offset = (page - 1) * pageSize });

        var totalCount = await multi.ReadSingleAsync<int>();
        var rows = await multi.ReadAsync<LinkRow>();
        return (rows.Select(ToEntity).ToList(), totalCount);
    }

    public async Task<Link?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        var row = await conn.QuerySingleOrDefaultAsync<LinkRow>(
            $"SELECT {SelectColumns} FROM links WHERE id = @Id",
            new { Id = id });
        return row is null ? null : ToEntity(row);
    }

    public async Task AddAsync(Link link, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO links (id, user_id, url, title, description, tags, suggested_tags, created_at, updated_at)
            VALUES (@Id, @UserId, @Url, @Title, @Description, @Tags, @SuggestedTags, @CreatedAt, @UpdatedAt)
            """,
            ToParams(link));
    }

    public async Task UpdateAsync(Link link, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            UPDATE links
            SET url = @Url, title = @Title, description = @Description,
                tags = @Tags, suggested_tags = @SuggestedTags, updated_at = @UpdatedAt
            WHERE id = @Id
            """,
            ToParams(link));
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync("DELETE FROM links WHERE id = @Id", new { Id = id });
    }

    private static object ToParams(Link link) => new
    {
        link.Id, link.UserId, link.Url, link.Title, link.Description,
        Tags = link.Tags.ToArray(),
        SuggestedTags = link.SuggestedTags.ToArray(),
        link.CreatedAt, link.UpdatedAt
    };

    private static Link ToEntity(LinkRow row) =>
        Link.Reconstitute(row.Id, row.UserId, row.Url, row.Title, row.Description, row.Tags, row.SuggestedTags, row.CreatedAt, row.UpdatedAt);

    private class LinkRow
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Url { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string[] Tags { get; set; } = [];
        public string[] SuggestedTags { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
