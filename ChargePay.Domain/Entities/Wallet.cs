using ChargePay.Domain.ValueObjects;

namespace ChargePay.Domain.Entities;

/// <summary>
/// Entidade que representa a carteira digital do usuário
/// </summary>
public class Wallet
{
    public Guid WalletId { get; private set; }
    public Guid UserId { get; private set; }
    public Money Balance { get; private set; } = null!;
    public Money? MaxDailyLimit { get; private set; }
    public Money? MaxMonthlyLimit { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Relações
    public virtual User? User { get; private set; }
    public virtual List<WalletTransaction> Transactions { get; private set; } = new();

    private Wallet() { }

    /// <summary>
    /// Criar uma nova carteira para um usuário
    /// </summary>
    public static Wallet Create(Guid userId)
    {
        return new Wallet
        {
            WalletId = Guid.NewGuid(),
            UserId = userId,
            Balance = Money.FromCents(0),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adicionar crédito à carteira
    /// </summary>
    public Result<bool> AddCredit(Money amount)
    {
        if (!amount.IsGreaterThan(Money.FromCents(0)))
            return Result<bool>.Failure("Valor deve ser maior que zero");

        Balance = Balance.Add(amount);
        UpdatedAt = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Débitar da carteira
    /// </summary>
    public Result<bool> Debit(Money amount)
    {
        if (!amount.IsGreaterThan(Money.FromCents(0)))
            return Result<bool>.Failure("Valor deve ser maior que zero");

        if (Balance.IsLessThan(amount))
            return Result<bool>.Failure("Saldo insuficiente");

        Balance = Balance.Subtract(amount);
        UpdatedAt = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Verificar se há saldo suficiente
    /// </summary>
    public bool HasSufficientBalance(Money amount) => Balance.IsGreaterThanOrEqual(amount);

    /// <summary>
    /// Definir limite diário
    /// </summary>
    public void SetDailyLimit(Money limit)
    {
        MaxDailyLimit = limit;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Definir limite mensal
    /// </summary>
    public void SetMonthlyLimit(Money limit)
    {
        MaxMonthlyLimit = limit;
        UpdatedAt = DateTime.UtcNow;
    }
}
