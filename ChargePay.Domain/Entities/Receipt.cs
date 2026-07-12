using ChargePay.Domain.ValueObjects;

namespace ChargePay.Domain.Entities;

/// <summary>
/// Entidade que representa um recibo/comprovante de recarga
/// </summary>
public class Receipt
{
    public Guid ReceiptId { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid WalletId { get; private set; }
    public Money AmountDebited { get; private set; } = null!;
    public decimal EnergyDispensedKwh { get; private set; }
    public string StationName { get; private set; } = null!;
    public DateTime IssuedAt { get; private set; }
    public string ReceiptNumber { get; private set; } = null!;

    // Relação
    public virtual ChargingSession? Session { get; private set; }

    private Receipt() { }

    /// <summary>
    /// Criar um novo recibo
    /// </summary>
    public static Receipt Create(
        Guid sessionId,
        Guid userId,
        Guid walletId,
        Money amountDebited,
        decimal energyDispensedKwh,
        string stationName)
    {
        var receiptNumber = GenerateReceiptNumber();

        return new Receipt
        {
            ReceiptId = Guid.NewGuid(),
            SessionId = sessionId,
            UserId = userId,
            WalletId = walletId,
            AmountDebited = amountDebited,
            EnergyDispensedKwh = energyDispensedKwh,
            StationName = stationName,
            IssuedAt = DateTime.UtcNow,
            ReceiptNumber = receiptNumber
        };
    }

    private static string GenerateReceiptNumber()
    {
        // Formato: REC-YYYYMMDD-HHMMSS-XXXX
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"REC-{timestamp}-{random}";
    }
}
