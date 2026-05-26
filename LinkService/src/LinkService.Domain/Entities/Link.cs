namespace LinkService.Domain.Entities;

public class Link
{
    public Guid Id { get; private set; }
    public Guid UserId { get; set; }
    public string Url { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();
    public IReadOnlyCollection<string> SuggestedTags => _suggestedTags.AsReadOnly();
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<string> _tags = [];
    private readonly List<string> _suggestedTags = [];

    private Link() { }

    public static Link Reconstitute(Guid id, Guid userId, string url, string title, string? description,
        IEnumerable<string> tags, IEnumerable<string> suggestedTags, DateTime createdAt, DateTime? updatedAt)
    {
        var link = new Link
        {
            Id = id,
            UserId = userId,
            Url = url,
            Title = title,
            Description = description,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
        link.SetTags(tags);
        link.SetSuggestedTags(suggestedTags);
        return link;
    }

    public static Link Create(Guid userId, string url, string? title = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty.", nameof(url));

        return new Link
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Url = url,
            Title = title ?? string.Empty,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void SetTitle(string title)
    {
        Title = title;
        UpdatedAt = DateTime.UtcNow;
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

    public void SetSuggestedTags(IEnumerable<string> tags)
    {
        _suggestedTags.Clear();
        _suggestedTags.AddRange(tags.Select(t => t.Trim().ToLowerInvariant()));
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmTags()
    {
        SetTags(_suggestedTags);
        _suggestedTags.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
