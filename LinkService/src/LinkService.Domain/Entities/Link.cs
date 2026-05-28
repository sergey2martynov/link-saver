namespace LinkService.Domain.Entities;

public class Link
{
    public Guid Id { get; private set; }
    public Guid UserId { get; set; }
    public string Url { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public IReadOnlyCollection<Guid> TagIds => _tagIds.AsReadOnly();
    public IReadOnlyCollection<Tag> Tags => _resolvedTags.AsReadOnly();
    public IReadOnlyCollection<string> SuggestedTags => _suggestedTags.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<Guid> _tagIds = [];
    private readonly List<Tag> _resolvedTags = [];
    private readonly List<string> _suggestedTags = [];

    private Link() { }

    public static Link Reconstitute(Guid id, Guid userId, string url, string title,
        IEnumerable<Guid> tagIds, IEnumerable<string> suggestedTags, DateTime createdAt, DateTime? updatedAt)
    {
        var link = new Link
        {
            Id = id,
            UserId = userId,
            Url = url,
            Title = title,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
        link._tagIds.AddRange(tagIds);
        link._suggestedTags.AddRange(suggestedTags.Select(t => t.Trim().ToLowerInvariant()));
        return link;
    }

    public static Link Create(Guid userId, string url, string? title = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty.", nameof(url));

        return new Link
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Url = url,
            Title = title ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void SetTitle(string title)
    {
        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string url, string title)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty.", nameof(url));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        Url = url;
        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTagIds(IEnumerable<Guid> tagIds)
    {
        _tagIds.Clear();
        _tagIds.AddRange(tagIds);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetResolvedTags(IEnumerable<Tag> tags)
    {
        _resolvedTags.Clear();
        _resolvedTags.AddRange(tags);
    }

    public void SetSuggestedTags(IEnumerable<string> tags)
    {
        _suggestedTags.Clear();
        _suggestedTags.AddRange(tags.Select(t => t.Trim().ToLowerInvariant()));
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearSuggestedTags()
    {
        _suggestedTags.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
