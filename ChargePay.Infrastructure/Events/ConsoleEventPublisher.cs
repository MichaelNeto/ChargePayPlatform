using ChargePay.Domain.Events;

namespace ChargePay.Infrastructure.Events;

public class ConsoleEventPublisher : IEventPublisher
{
    public Task PublishAsync(DomainEvent domainEvent)
    {
        Console.WriteLine($"Evento publicado: {domainEvent.GetEventType()} - AggregateId={domainEvent.AggregateId} - EventId={domainEvent.EventId}");
        return Task.CompletedTask;
    }
}
