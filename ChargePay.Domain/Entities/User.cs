using ChargePay.Domain.Enums;
using ChargePay.Domain.Events;
using ChargePay.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace ChargePay.Domain.Entities;

/// <summary>
/// Entidade que representa um usuário na plataforma
/// Implementa conceitos de agregado raiz de DDD
/// </summary>
public class User
{
    public Guid UserId { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime BirthDate { get; private set; }
    public string Phone { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Document Document { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserType Type { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public int Age => CalculateAge(BirthDate);

    // Relações
    public virtual Wallet? Wallet { get; private set; }
    public virtual List<ChargingSession> ChargingSessions { get; private set; } = new();
    public virtual List<RefreshToken> RefreshTokens { get; private set; } = new();

    private User() { }

    /// <summary>
    /// Criar um novo usuário
    /// </summary>
    public static Result<User> Create(
        string firstName,
        string lastName,
        DateTime birthDate,
        string phone,
        Email email,
        Document document,
        UserType type,
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result<User>.Failure("Primeiro nome é obrigatório");

        if (!IsValidFirstName(firstName))
            return Result<User>.Failure("Primeiro nome deve conter entre 2 e 80 caracteres e não pode conter números ou caracteres especiais.");

        if (string.IsNullOrWhiteSpace(lastName))
            return Result<User>.Failure("Sobrenome é obrigatório");

        if (!IsValidLastName(lastName))
            return Result<User>.Failure("Sobrenome deve conter entre 2 e 100 caracteres e não pode conter números ou caracteres especiais.");

        if (birthDate.Date >= DateTime.UtcNow.Date)
            return Result<User>.Failure("Data de nascimento inválida");

        if (CalculateAge(birthDate) < 18)
            return Result<User>.Failure("É necessário possuir pelo menos 18 anos.");

        if (string.IsNullOrWhiteSpace(phone) || !ValidatePhone(phone))
            return Result<User>.Failure("Telefone inválido. Deve conter DDD + número com 11 dígitos.");

        if (passwordHash.Length < 60)
            return Result<User>.Failure("Hash de senha inválido");

        var user = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            BirthDate = birthDate.Date,
            Phone = Regex.Replace(phone, "\\D", ""),
            Email = email,
            Document = document,
            Type = type,
            PasswordHash = passwordHash,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            FailedLoginAttempts = 0
        };

        return Result<User>.Success(user);
    }

    private static bool IsValidFirstName(string firstName)
    {
        firstName = firstName.Trim();
        if (firstName.Length < 2 || firstName.Length > 80)
            return false;

        return Regex.IsMatch(firstName, "^[\\p{L}'-]+$");
    }

    private static bool IsValidLastName(string lastName)
    {
        lastName = lastName.Trim();
        if (lastName.Length < 2 || lastName.Length > 100)
            return false;

        if (!lastName.Contains(' '))
            return Regex.IsMatch(lastName, "^[\\p{L}'-]+$");

        return Regex.IsMatch(lastName, @"^[\p{L}'\s-]+$");
    }

    private static bool ValidatePhone(string phone)
    {
        phone = Regex.Replace(phone, "\\D", "");
        return phone.Length == 11 && long.TryParse(phone, out _);
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - birthDate.Date.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;

        return age;
    }

    /// <summary>
    /// Registrar uma tentativa de login bem-sucedida
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Registrar uma tentativa de login falha
    /// </summary>
    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        UpdatedAt = DateTime.UtcNow;

        // Bloquear após 5 tentativas inválidas
        if (FailedLoginAttempts >= 5)
        {
            Status = UserStatus.Locked;
        }
    }

    /// <summary>
    /// Desbloquear o usuário
    /// </summary>
    public void Unlock()
    {
        Status = UserStatus.Active;
        FailedLoginAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Suspender o usuário
    /// </summary>
    public void Suspend(string reason = "")
    {
        Status = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Desativar o usuário
    /// </summary>
    public void Deactivate()
    {
        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verificar se o usuário pode realizar transações
    /// </summary>
    public bool CanPerformTransactions() => Status == UserStatus.Active;
}
