using UserService.Application.DTOs;
using UserService.Application.Events;
using UserService.Application.Exceptions;
using UserService.Application.Ports;
using UserService.Domain.Entities;

namespace UserService.Application;

public class UsersService(IUserRepository repository, IPasswordHasher passwordHasher, IUserEventPublisher eventPublisher)
{
    private const int PageSize = 10;

    public async Task<PagedResult<UserDto>> GetPagedAsync(int page, CancellationToken ct = default)
    {
        var (items, totalCount) = await repository.GetPagedAsync(page, PageSize, ct);
        return new PagedResult<UserDto>(items.Select(ToDto).ToList(), page, PageSize, totalCount);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        var user = User.Create(dto.Email, dto.Name);
        await repository.AddAsync(user, ct);
        return ToDto(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var user = await repository.GetByIdAsync(id, ct)
            ?? throw new UserNotFoundException(id);

        user.Update(dto.Email, dto.Name);
        await repository.UpdateAsync(user, ct);
        return ToDto(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await repository.GetByIdAsync(id, ct)
            ?? throw new UserNotFoundException(id);

        await repository.DeleteAsync(user.Id, ct);
    }

    public async Task<UserDto> RegisterWithPasswordAsync(RegisterWithPasswordDto dto, CancellationToken ct = default)
    {
        var existing = await repository.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant(), ct);
        if (existing is not null)
            throw new EmailAlreadyTakenException(dto.Email);

        var hash = passwordHasher.Hash(dto.Password);
        var user = User.CreateWithPassword(dto.Email, dto.Name, hash);
        await repository.AddAsync(user, ct);

        // Publish domain event — the background publisher returns immediately (fire-and-forget).
        await eventPublisher.PublishUserRegisteredAsync(
            new UserRegisteredEvent(Guid.NewGuid(), DateTime.UtcNow, user.Id, user.Email!, user.Name), ct);

        return ToDto(user);
    }

    public async Task<UserDto?> ValidateCredentialsAsync(ValidateCredentialsDto dto, CancellationToken ct = default)
    {
        var user = await repository.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant(), ct);
        if (user?.PasswordHash is null)
            return null;

        return passwordHasher.Verify(dto.Password, user.PasswordHash) ? ToDto(user) : null;
    }

    public async Task<UserDto> UpsertTelegramAsync(UpsertTelegramUserDto dto, CancellationToken ct = default)
    {
        var existing = await repository.GetByTelegramIdAsync(dto.TelegramId, ct);
        if (existing is not null)
            return ToDto(existing);

        var user = User.CreateFromTelegram(dto.TelegramId, dto.Name);
        await repository.AddAsync(user, ct);
        return ToDto(user);
    }

    private static UserDto ToDto(User user) =>
        new(user.Id, user.Email, user.Name, user.CreatedAt, user.UpdatedAt);
}
