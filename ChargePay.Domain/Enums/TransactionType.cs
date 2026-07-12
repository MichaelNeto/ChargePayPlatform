namespace ChargePay.Domain.Enums;

/// <summary>
/// Tipo de transação financeira
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Crédito adicionado à carteira
    /// </summary>
    Credit = 1,

    /// <summary>
    /// Débito pela recarga
    /// </summary>
    Debit = 2,

    /// <summary>
    /// Reembolso
    /// </summary>
    Refund = 3,

    /// <summary>
    /// Ajuste administrativo
    /// </summary>
    Adjustment = 4
}
