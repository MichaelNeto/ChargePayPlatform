namespace ChargePay.Domain.Entities;

/// <summary>
/// Entidade que representa uma notificação
/// </summary>
public class Notification
{
    public Guid NotificationId { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public string Type { get; private set; } = null!; // "Welcome", "Charge", "SessionStarted", "SessionEnded", "InsufficientFunds"
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public Guid? RelatedEntityId { get; private set; }

    private Notification() { }

    /// <summary>
    /// Criar uma nova notificação
    /// </summary>
    public static Notification Create(
        Guid userId,
        string title,
        string message,
        string type,
        Guid? relatedEntityId = null)
    {
        return new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            RelatedEntityId = relatedEntityId
        };
    }

    /// <summary>
    /// Marcar como lida
    /// </summary>
    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
