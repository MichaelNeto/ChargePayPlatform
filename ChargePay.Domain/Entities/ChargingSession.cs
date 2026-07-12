using ChargePay.Domain.Enums;
using ChargePay.Domain.ValueObjects;

namespace ChargePay.Domain.Entities;

/// <summary>
/// Entidade que representa uma sessão de recarga
/// </summary>
public class ChargingSession
{
    public Guid SessionId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid StationId { get; private set; }
    public SessionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public decimal EnergyConsumedKwh { get; private set; }
    public Money TotalAmount { get; private set; } = null!;
    public string? Notes { get; private set; }

    // Relações
    public virtual User? User { get; private set; }
    public virtual ChargingStation? Station { get; private set; }
    public virtual List<SessionTelemetry> Telemetries { get; private set; } = new();
    public virtual List<SessionCharge> Charges { get; private set; } = new();
    public virtual Receipt? Receipt { get; private set; }

    private ChargingSession() { }

    /// <summary>
    /// Criar uma nova sessão de recarga
    /// </summary>
    public static Result<ChargingSession> Create(Guid userId, Guid stationId)
    {
        var session = new ChargingSession
        {
            SessionId = Guid.NewGuid(),
            UserId = userId,
            StationId = stationId,
            Status = SessionStatus.Created,
            StartedAt = DateTime.UtcNow,
            EnergyConsumedKwh = 0,
            TotalAmount = Money.FromCents(0)
        };

        return Result<ChargingSession>.Success(session);
    }

    /// <summary>
    /// Autorizar a sessão
    /// </summary>
    public Result<bool> Authorize()
    {
        if (Status != SessionStatus.Created)
            return Result<bool>.Failure("Sessão deve estar no status Created");

        Status = SessionStatus.Authorized;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Iniciar a recarga
    /// </summary>
    public Result<bool> Start()
    {
        if (Status != SessionStatus.Authorized)
            return Result<bool>.Failure("Sessão deve estar autorizada");

        Status = SessionStatus.InProgress;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Registrar consumo de energia
    /// </summary>
    public void RecordEnergyConsumption(decimal kwhConsumed)
    {
        EnergyConsumedKwh += kwhConsumed;
    }

    /// <summary>
    /// Atualizar valor total
    /// </summary>
    public void UpdateTotalAmount(Money amount)
    {
        TotalAmount = amount;
    }

    /// <summary>
    /// Finalizar a sessão
    /// </summary>
    public Result<bool> Complete()
    {
        if (Status != SessionStatus.InProgress && Status != SessionStatus.Authorized)
            return Result<bool>.Failure("Sessão não pode ser finalizada neste estado");

        Status = SessionStatus.Completed;
        EndedAt = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Cancelar a sessão
    /// </summary>
    public Result<bool> Cancel(string reason = "")
    {
        if (Status == SessionStatus.Completed || Status == SessionStatus.Cancelled || Status == SessionStatus.Failed)
            return Result<bool>.Failure("Sessão não pode ser cancelada");

        Status = SessionStatus.Cancelled;
        EndedAt = DateTime.UtcNow;
        Notes = reason;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Marcar como falha
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        Status = SessionStatus.Failed;
        EndedAt = DateTime.UtcNow;
        Notes = errorMessage;
    }

    /// <summary>
    /// Calcular duração da sessão em minutos
    /// </summary>
    public int GetDurationMinutes()
    {
        var end = EndedAt ?? DateTime.UtcNow;
        return (int)(end - StartedAt).TotalMinutes;
    }
}
