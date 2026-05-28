namespace LinkService.Domain.Entities;

public class Tag
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Color { get; private set; } = null!;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Tag() { }

    public static Tag Create(Guid userId, string name, string color, DateTime? startDate = null, DateTime? endDate = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(color)) throw new ArgumentException("Color cannot be empty.", nameof(color));

        return new Tag
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            Color = color.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Tag Reconstitute(Guid id, Guid userId, string name, string color,
        DateTime? startDate, DateTime? endDate, DateTime createdAt, DateTime? updatedAt) =>
        new()
        {
            Id = id, UserId = userId, Name = name, Color = color,
            StartDate = startDate, EndDate = endDate, CreatedAt = createdAt, UpdatedAt = updatedAt
        };

    public void Update(string name, string color, DateTime? startDate, DateTime? endDate)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(color)) throw new ArgumentException("Color cannot be empty.", nameof(color));

        Name = name.Trim();
        Color = color.Trim();
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }
}
