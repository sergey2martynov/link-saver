using LinkService.Domain.Entities;

namespace LinkService.Application.Interfaces;

public interface ILinkDomainRepository
{
    Task AddAsync(LinkDomain linkDomain, CancellationToken ct = default);
    Task<LinkDomain?> FindByDomainAsync(string domain, CancellationToken ct = default);
}
