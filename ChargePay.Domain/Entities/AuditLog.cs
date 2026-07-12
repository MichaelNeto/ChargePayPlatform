namespace ChargePay.Domain.Entities;

/// <summary>
/// Entidade que registra todos os eventos da plataforma para auditoria
/// </summary>
public class AuditLog
{
    public Guid LogId { get; private set; }
    public string EventType { get; private set; } = null!;
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string? CorrelationId { get; private set; }
    public string? TraceId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }

    private AuditLog() { }

    /// <summary>
    /// Criar um novo registro de auditoria
    /// </summary>
    public static AuditLog Create(
        string eventType,
        string entityType,
        Guid entityId,
        string action,
        string description,
        Guid? userId = null,
        string? correlationId = null,
        string? traceId = null,
        string? oldValues = null,
        string? newValues = null)
    {
        return new AuditLog
        {
            LogId = Guid.NewGuid(),
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            Action = action,
            Description = description,
            CorrelationId = correlationId,
            TraceId = traceId,
            CreatedAt = DateTime.UtcNow,
            OldValues = oldValues,
            NewValues = newValues
        };
    }
}
