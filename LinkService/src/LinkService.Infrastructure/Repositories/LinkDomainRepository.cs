using Dapper;
using LinkService.Application.Interfaces;
using LinkService.Domain.Entities;
using Npgsql;

namespace LinkService.Infrastructure.Repositories;

public class LinkDomainRepository(NpgsqlDataSource dataSource) : ILinkDomainRepository
{
    public async Task AddAsync(LinkDomain linkDomain, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await conn.ExecuteAsync(
            """
            INSERT INTO link_domains (id, domain, tags)
            VALUES (@Id, @Domain, @Tags)
            """,
            new { linkDomain.Id, linkDomain.Domain, Tags = linkDomain.Tags.ToArray() });
    }

    public async Task<LinkDomain?> FindByDomainAsync(string domain, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        var row = await conn.QuerySingleOrDefaultAsync<LinkDomainRow>(new CommandDefinition(
            "SELECT id, domain, tags FROM link_domains WHERE domain = @Domain",
            new { Domain = domain },
            cancellationToken: ct));

        return row is null ? null : LinkDomain.Reconstitute(row.Id, row.Domain, row.Tags);
    }

    private class LinkDomainRow
    {
        public Guid Id { get; set; }
        public string Domain { get; set; } = null!;
        public string[] Tags { get; set; } = [];
    }
}
