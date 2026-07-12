namespace ChargePay.Domain.ValueObjects;

/// <summary>
/// Value Object para representar moeda (utiliza centavos como unidade)
/// </summary>
public class Money : IEquatable<Money>
{
    /// <summary>
    /// Valor em centavos
    /// </summary>
    public long Amount { get; private set; }

    private Money(long amount)
    {
        Amount = amount;
    }

    /// <summary>
    /// Criar uma instância de Money a partir de um valor em reais
    /// </summary>
    public static Result<Money> Create(decimal value)
    {
        if (value < 0)
            return Result<Money>.Failure("Valor não pode ser negativo");

        // Converter para centavos
        long amountInCents = (long)(value * 100);
        return Result<Money>.Success(new Money(amountInCents));
    }

    /// <summary>
    /// Criar a partir de centavos
    /// </summary>
    public static Money FromCents(long cents) => new(cents);

    /// <summary>
    /// Obter valor em reais
    /// </summary>
    public decimal ToDecimal() => Amount / 100m;

    public Money Add(Money other) => new(Amount + other.Amount);
    public Money Subtract(Money other) => new(Amount - other.Amount);
    public Money Multiply(decimal factor) => new((long)(Amount * factor));

    public bool IsGreaterThan(Money other) => Amount > other.Amount;
    public bool IsGreaterThanOrEqual(Money other) => Amount >= other.Amount;
    public bool IsLessThan(Money other) => Amount < other.Amount;
    public bool IsLessThanOrEqual(Money other) => Amount <= other.Amount;

    public bool Equals(Money? other) => other != null && Amount == other.Amount;

    public override bool Equals(object? obj)
    {
        if (obj is not Money other)
            return false;

        return Equals(other);
    }

    public override int GetHashCode() => Amount.GetHashCode();

    public override string ToString() => $"R$ {ToDecimal():N2}";
}
