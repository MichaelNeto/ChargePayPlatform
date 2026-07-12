namespace ChargePay.Domain.Enums;

/// <summary>
/// Status da sessão de recarga
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// Sessão inicializada mas não autorizada
    /// </summary>
    Created = 1,

    /// <summary>
    /// Sessão autorizada para iniciar a recarga
    /// </summary>
    Authorized = 2,

    /// <summary>
    /// Recarga em andamento
    /// </summary>
    InProgress = 3,

    /// <summary>
    /// Recarga concluída
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Recarga cancelada
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Erro durante a recarga
    /// </summary>
    Failed = 6
}
