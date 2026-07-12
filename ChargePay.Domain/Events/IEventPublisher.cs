namespace ChargePay.Domain.Events;

public interface IEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent);
}
