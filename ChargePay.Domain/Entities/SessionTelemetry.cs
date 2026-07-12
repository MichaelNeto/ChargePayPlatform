namespace ChargePay.Domain.Entities;

/// <summary>
/// Entidade que representa leitura de telemetria de uma sessão
/// </summary>
public class SessionTelemetry
{
    public Guid TelemetryId { get; private set; }
    public Guid SessionId { get; private set; }
    public decimal CurrentKwh { get; private set; }
    public decimal PreviousKwh { get; private set; }
    public decimal KwhIncrement { get; private set; }
    public decimal Voltage { get; private set; }
    public decimal Current { get; private set; }
    public decimal Power { get; private set; }
    public int Temperature { get; private set; }
    public DateTime ReceivedAt { get; private set; }

    // Relação
    public virtual ChargingSession? Session { get; private set; }

    private SessionTelemetry() { }

    /// <summary>
    /// Criar um novo registro de telemetria
    /// </summary>
    public static SessionTelemetry Create(
        Guid sessionId,
        decimal currentKwh,
        decimal previousKwh,
        decimal voltage,
        decimal current,
        decimal power,
        int temperature)
    {
        var increment = currentKwh - previousKwh;
        if (increment < 0) increment = 0;

        return new SessionTelemetry
        {
            TelemetryId = Guid.NewGuid(),
            SessionId = sessionId,
            CurrentKwh = currentKwh,
            PreviousKwh = previousKwh,
            KwhIncrement = increment,
            Voltage = voltage,
            Current = current,
            Power = power,
            Temperature = temperature,
            ReceivedAt = DateTime.UtcNow
        };
    }
}
