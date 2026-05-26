using UserService.Application.Events;

namespace UserService.Application.Ports;

// Port for publishing user domain events.
// The Application layer depends on this abstraction; Infrastructure provides the Kafka implementation.
public interface IUserEventPublisher
{
    Task PublishUserRegisteredAsync(UserRegisteredEvent evt, CancellationToken ct = default);
}
