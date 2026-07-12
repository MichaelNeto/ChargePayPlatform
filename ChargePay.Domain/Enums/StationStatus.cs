namespace ChargePay.Domain.Enums;

/// <summary>
/// Status da estação de recarga
/// </summary>
public enum StationStatus
{
    /// <summary>
    /// Estação ativa e operacional
    /// </summary>
    Active = 1,

    /// <summary>
    /// Estação sob manutenção
    /// </summary>
    Maintenance = 2,

    /// <summary>
    /// Estação inativa
    /// </summary>
    Inactive = 3,

    /// <summary>
    /// Estação com problemas críticos
    /// </summary>
    Unavailable = 4
}
