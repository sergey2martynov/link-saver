namespace UserService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string? Email { get; private set; }
    public string Name { get; private set; } = null!;
    public long? TelegramId { get; private set; }
    public string? PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private User() { }

    public static User Reconstitute(Guid id, string? email, string name, long? telegramId, string? passwordHash, DateTime createdAt, DateTime? updatedAt) =>
        new() { Id = id, Email = email, Name = name, TelegramId = telegramId, PasswordHash = passwordHash, CreatedAt = createdAt, UpdatedAt = updatedAt };

    public static User Create(string email, string name)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateWithPassword(string email, string name, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            Name = name.Trim(),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateFromTelegram(long telegramId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return new User
        {
            Id = Guid.NewGuid(),
            TelegramId = telegramId,
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string email, string name)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Email = email.Trim().ToLowerInvariant();
        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
