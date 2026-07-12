using ChargePay.Domain.Enums;
using ChargePay.Domain.ValueObjects;

namespace ChargePay.Domain.Entities;

/// <summary>
/// Entidade que representa uma transação na carteira
/// </summary>
public class WalletTransaction
{
    public Guid TransactionId { get; private set; }
    public Guid WalletId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public TransactionType Type { get; private set; }
    public string Description { get; private set; } = null!;
    public Guid? ChargingSessionId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Relação
    public virtual Wallet? Wallet { get; private set; }

    private WalletTransaction() { }

    /// <summary>
    /// Criar uma nova transação
    /// </summary>
    public static WalletTransaction Create(
        Guid walletId,
        Money amount,
        TransactionType type,
        string description,
        Guid? chargingSessionId = null)
    {
        return new WalletTransaction
        {
            TransactionId = Guid.NewGuid(),
            WalletId = walletId,
            Amount = amount,
            Type = type,
            Description = description,
            ChargingSessionId = chargingSessionId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
