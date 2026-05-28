using Dapper;
using LinkService.Application.DTOs;
using LinkService.Application.Ports;
using LinkService.Domain.Entities;
using Npgsql;

namespace LinkService.Infrastructure.Repositories;

public class LinkRepository(NpgsqlDataSource dataSource) : ILinkRepository
{
    private const string SelectLinkColumns =
        "l.id, l.user_id, l.url, l.title, l.suggested_tags, l.created_at, l.updated_at";

    public async Task<(IReadOnlyList<Link> Items, int TotalCount)> GetPagedAsync(
        Guid userId, int page, int pageSize, LinkFilterDto? filter = null, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);

        var conditions = new List<string> { "l.user_id = @UserId" };
        if (filter?.TagIds is { Length: > 0 })
            conditions.Add("EXISTS (SELECT 1 FROM link_tags lt WHERE lt.link_id = l.id AND lt.tag_id = ANY(@TagIds))");
        if (filter?.DateFrom.HasValue == true)
            conditions.Add("l.created_at >= @DateFrom");
        if (filter?.DateTo.HasValue == true)
            conditions.Add("l.created_at < @DateTo");

        var where = string.Join(" AND ", conditions);
        var sql = $"""
            SELECT COUNT(*) FROM links l WHERE {where};
            SELECT {SelectLinkColumns} FROM links l WHERE {where} ORDER BY l.created_at DESC LIMIT @PageSize OFFSET @Offset
            """;

        // DateTo is treated as exclusive end-of-day: add 1 day so "to 2026-05-28" includes the whole day
        var dateToExclusive = filter?.DateTo?.Date.AddDays(1);

        using var multi = await conn.QueryMultipleAsync(sql,
            new
            {
                UserId = userId,
                TagIds = filter?.TagIds,
                DateFrom = filter?.DateFrom,
                DateTo = dateToExclusive,
                PageSize = pageSize,
                Offset = (page - 1) * pageSize,
            });

        var totalCount = await multi.ReadSingleAsync<int>();
        var rows = (await multi.ReadAsync<LinkRow>()).ToList();

        if (rows.Count == 0)
            return ([], totalCount);

        var tagsByLink = await LoadTagsForLinksAsync(conn, rows.Select(r => r.Id).ToArray());
        return (rows.Select(r => Hydrate(r, tagsByLink)).ToList(), totalCount);
    }

    public async Task<Link?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);

        var row = await conn.QuerySingleOrDefaultAsync<LinkRow>(
            $"SELECT {SelectLinkColumns} FROM links l WHERE l.id = @Id",
            new { Id = id });

        if (row is null) return null;

        var tagsByLink = await LoadTagsForLinksAsync(conn, [row.Id]);
        return Hydrate(row, tagsByLink);
    }

    public async Task AddAsync(Link link, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        await conn.ExecuteAsync(
            """
            INSERT INTO links (id, user_id, url, title, suggested_tags, created_at, updated_at)
            VALUES (@Id, @UserId, @Url, @Title, @SuggestedTags, @CreatedAt, @UpdatedAt)
            """,
            ToParams(link), tx);

        await UpsertLinkTagsAsync(conn, tx, link.Id, link.TagIds);

        await tx.CommitAsync(ct);
    }

    public async Task UpdateAsync(Link link, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        await conn.ExecuteAsync(
            """
            UPDATE links
            SET url = @Url, title = @Title, suggested_tags = @SuggestedTags, updated_at = @UpdatedAt
            WHERE id = @Id
            """,
            ToParams(link), tx);

        await UpsertLinkTagsAsync(conn, tx, link.Id, link.TagIds);

        await tx.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        // link_tags rows deleted via ON DELETE CASCADE
        await conn.ExecuteAsync("DELETE FROM links WHERE id = @Id", new { Id = id });
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static async Task UpsertLinkTagsAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx, Guid linkId, IReadOnlyCollection<Guid> tagIds)
    {
        await conn.ExecuteAsync(
            "DELETE FROM link_tags WHERE link_id = @LinkId",
            new { LinkId = linkId }, tx);

        if (tagIds.Count == 0) return;

        await conn.ExecuteAsync(
            "INSERT INTO link_tags (link_id, tag_id) VALUES (@LinkId, @TagId)",
            tagIds.Select(tagId => new { LinkId = linkId, TagId = tagId }),
            tx);
    }

    private static async Task<Dictionary<Guid, List<Tag>>> LoadTagsForLinksAsync(
        NpgsqlConnection conn, Guid[] linkIds)
    {
        var rows = await conn.QueryAsync<LinkTagRow>(
            """
            SELECT lt.link_id, t.id, t.user_id, t.name, t.color, t.created_at, t.updated_at
            FROM link_tags lt
            JOIN tags t ON t.id = lt.tag_id
            WHERE lt.link_id = ANY(@LinkIds)
            """,
            new { LinkIds = linkIds });

        var result = new Dictionary<Guid, List<Tag>>();
        foreach (var row in rows)
        {
            if (!result.TryGetValue(row.LinkId, out var list))
                result[row.LinkId] = list = [];
            list.Add(Tag.Reconstitute(row.Id, row.UserId, row.Name, row.Color, row.CreatedAt, row.UpdatedAt));
        }
        return result;
    }

    private static Link Hydrate(LinkRow row, Dictionary<Guid, List<Tag>> tagsByLink)
    {
        var tags = tagsByLink.GetValueOrDefault(row.Id, []);
        var link = Link.Reconstitute(
            row.Id, row.UserId, row.Url, row.Title,
            tags.Select(t => t.Id),
            row.SuggestedTags,
            row.CreatedAt, row.UpdatedAt);
        link.SetResolvedTags(tags);
        return link;
    }

    private static object ToParams(Link link) => new
    {
        link.Id, link.UserId, link.Url, link.Title,
        SuggestedTags = link.SuggestedTags.ToArray(),
        link.CreatedAt, link.UpdatedAt
    };

    private class LinkRow
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Url { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string[] SuggestedTags { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    private class LinkTagRow
    {
        public Guid LinkId { get; set; }
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Color { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
