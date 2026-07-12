namespace ChargePay.Domain.Enums;

/// <summary>
/// Tipo de usuário - Pessoa Física ou Jurídica
/// </summary>
public enum UserType
{
    /// <summary>
    /// Pessoa Física (CPF)
    /// </summary>
    Individual = 1,

    /// <summary>
    /// Pessoa Jurídica (CNPJ)
    /// </summary>
    Company = 2
}
