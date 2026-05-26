namespace LinkService.Domain.Entities;

public class LinkDomain
{
    public Guid Id { get; private set; }
    public string Domain { get; private set; } = null!;
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    private readonly List<string> _tags = [];

    private LinkDomain() { }

    public static LinkDomain Create(string domain, IEnumerable<string> tags)
    {
        var ld = new LinkDomain { Id = Guid.NewGuid(), Domain = domain };
        ld._tags.AddRange(tags);
        return ld;
    }

    public static LinkDomain Reconstitute(Guid id, string domain, IEnumerable<string> tags)
    {
        var ld = new LinkDomain { Id = id, Domain = domain };
        ld._tags.AddRange(tags);
        return ld;
    }
}
