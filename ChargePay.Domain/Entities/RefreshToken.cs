namespace ChargePay.Domain.Entities;

/// <summary>
/// Entidade para armazenar tokens de refresh
/// </summary>
public class RefreshToken
{
    public Guid TokenId { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private RefreshToken() { }

    /// <summary>
    /// Criar um novo refresh token
    /// </summary>
    public static RefreshToken Create(Guid userId, string token, int expirationDays)
    {
        return new RefreshToken
        {
            TokenId = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
    }

    /// <summary>
    /// Verificar se o token ainda é válido
    /// </summary>
    public bool IsValid() => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Revogar o token
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}
