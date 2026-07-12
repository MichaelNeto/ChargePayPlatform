using ChargePay.Domain.ValueObjects;

namespace ChargePay.Domain.Entities;

/// <summary>
/// Entidade que representa uma cobrança de consumo
/// </summary>
public class SessionCharge
{
    public Guid ChargeId { get; private set; }
    public Guid SessionId { get; private set; }
    public decimal EnergyKwh { get; private set; }
    public Money TariffPerKwh { get; private set; } = null!;
    public Money ChargeAmount { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    // Relação
    public virtual ChargingSession? Session { get; private set; }

    private SessionCharge() { }

    /// <summary>
    /// Criar uma nova cobrança
    /// </summary>
    public static SessionCharge Create(
        Guid sessionId,
        decimal energyKwh,
        Money tariffPerKwh)
    {
        // Calcular valor: energia * tarifa
        var chargeAmount = tariffPerKwh.Multiply(energyKwh);

        return new SessionCharge
        {
            ChargeId = Guid.NewGuid(),
            SessionId = sessionId,
            EnergyKwh = energyKwh,
            TariffPerKwh = tariffPerKwh,
            ChargeAmount = chargeAmount,
            CreatedAt = DateTime.UtcNow
        };
    }
}
