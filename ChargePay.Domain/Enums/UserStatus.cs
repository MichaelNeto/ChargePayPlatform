namespace ChargePay.Domain.Enums;

/// <summary>
/// Status do usuário na plataforma
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// Usuário ativo e autorizado a realizar operações
    /// </summary>
    Active = 1,

    /// <summary>
    /// Usuário suspenso temporariamente
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Usuário desativado permanentemente
    /// </summary>
    Inactive = 3,

    /// <summary>
    /// Usuário com múltiplas falhas de autenticação
    /// </summary>
    Locked = 4
}
