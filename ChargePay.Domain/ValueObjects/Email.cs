using System.Text.RegularExpressions;

namespace ChargePay.Domain.ValueObjects;

/// <summary>
/// Value Object para representar um endereço de e-mail
/// </summary>
public class Email
{
    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Criar um e-mail validado
    /// </summary>
    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Failure("E-mail não pode ser vazio");

        email = email.Trim().ToLower();

        if (!ValidateEmail(email))
            return Result<Email>.Failure("E-mail inválido");

        return Result<Email>.Success(new Email(email));
    }

    private static bool ValidateEmail(string email)
    {
        try
        {
            const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }
        catch
        {
            return false;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Email other)
            return false;

        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}
