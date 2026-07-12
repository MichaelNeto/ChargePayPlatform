using System.Text.RegularExpressions;

namespace ChargePay.Domain.ValueObjects;

/// <summary>
/// Value Object para representar um documento (CPF ou CNPJ)
/// </summary>
public class Document
{
    public string Value { get; private set; }
    public string Type { get; private set; } // "CPF" ou "CNPJ"

    private Document(string value, string type)
    {
        Value = value;
        Type = type;
    }

    /// <summary>
    /// Criar um documento de CPF
    /// </summary>
    public static Result<Document> CreateCpf(string cpf)
    {
        if (!ValidateCpf(cpf))
            return Result<Document>.Failure("CPF inválido");

        return Result<Document>.Success(new Document(CleanDocument(cpf), "CPF"));
    }

    /// <summary>
    /// Criar um documento de CNPJ
    /// </summary>
    public static Result<Document> CreateCnpj(string cnpj)
    {
        if (!ValidateCnpj(cnpj))
            return Result<Document>.Failure("CNPJ inválido");

        return Result<Document>.Success(new Document(CleanDocument(cnpj), "CNPJ"));
    }

    /// <summary>
    /// Criar um documento de CPF ou CNPJ automaticamente
    /// </summary>
    public static Result<Document> Create(string document)
    {
        var cleaned = CleanDocument(document);
        return cleaned.Length switch
        {
            11 => CreateCpf(cleaned),
            14 => CreateCnpj(cleaned),
            _ => Result<Document>.Failure("Documento inválido")
        };
    }

    private static bool ValidateCpf(string cpf)
    {
        cpf = CleanDocument(cpf);
        if (cpf.Length != 11 || !long.TryParse(cpf, out _))
            return false;

        // Validar dígitos verificadores
        var digits = new int[11];
        for (int i = 0; i < 11; i++)
            digits[i] = cpf[i] - '0';

        // Primeiro dígito verificador
        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += digits[i] * (10 - i);

        int remainder = sum % 11;
        int firstVerifier = remainder < 2 ? 0 : 11 - remainder;

        if (digits[9] != firstVerifier)
            return false;

        // Segundo dígito verificador
        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += digits[i] * (11 - i);

        remainder = sum % 11;
        int secondVerifier = remainder < 2 ? 0 : 11 - remainder;

        return digits[10] == secondVerifier;
    }

    private static bool ValidateCnpj(string cnpj)
    {
        cnpj = CleanDocument(cnpj);
        if (cnpj.Length != 14 || !long.TryParse(cnpj, out _))
            return false;

        // Validar dígitos verificadores
        var digits = new int[14];
        for (int i = 0; i < 14; i++)
            digits[i] = cnpj[i] - '0';

        // Primeiro dígito verificador
        int sum = 0;
        int multiplier = 5;
        for (int i = 0; i < 8; i++)
        {
            sum += digits[i] * multiplier;
            multiplier = multiplier == 2 ? 9 : multiplier - 1;
        }

        int remainder = sum % 11;
        int firstVerifier = remainder < 2 ? 0 : 11 - remainder;

        if (digits[8] != firstVerifier)
            return false;

        // Segundo dígito verificador
        sum = 0;
        multiplier = 6;
        for (int i = 0; i < 9; i++)
        {
            sum += digits[i] * multiplier;
            multiplier = multiplier == 2 ? 9 : multiplier - 1;
        }

        remainder = sum % 11;
        int secondVerifier = remainder < 2 ? 0 : 11 - remainder;

        return digits[9] == secondVerifier;
    }

    private static string CleanDocument(string document)
    {
        return Regex.Replace(document, @"\D", "");
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Document other)
            return false;

        return Value == other.Value && Type == other.Type;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, Type);
    }

    public override string ToString() => Value;
}

/// <summary>
/// Classe para representar resultado de operações
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }

    private Result(bool isSuccess, T? data, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
}
