namespace LinkService.Domain.Entities;

public class Link
{
    public Guid Id { get; private set; }
    public Guid UserId { get; set; }
    public string Url { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<string> _tags = [];

    private Link() { }

    public static Link Reconstitute(Guid id, string url, string title, string? description,
        IEnumerable<string> tags, DateTime createdAt, DateTime? updatedAt)
    {
        var link = new Link
        {
            Id = id,
            Url = url,
            Title = title,
            Description = description,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
        link.SetTags(tags);
        return link;
    }

    public static Link Create(string url, string title, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty.", nameof(url));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        return new Link
        {
            Id = Guid.NewGuid(),
            Url = url,
            Title = title,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string url, string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty.", nameof(url));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        Url = url;
        Title = title;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTags(IEnumerable<string> tags)
    {
        _tags.Clear();
        foreach (var tag in tags)
            AddTag(tag);
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be empty.", nameof(tag));

        var normalized = tag.Trim().ToLowerInvariant();

        if (!_tags.Contains(normalized))
            _tags.Add(normalized);
    }

    public void RemoveTag(string tag)
    {
        _tags.Remove(tag.Trim().ToLowerInvariant());
    }
}
