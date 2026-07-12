namespace ChargePay.Domain.Events;

/// <summary>
/// Classe base para eventos de domínio
/// </summary>
public abstract class DomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public Guid AggregateId { get; set; }
    public string CorrelationId { get; protected set; } = Guid.NewGuid().ToString();

    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
    }

    public abstract string GetEventType();
}

/// <summary>
/// Evento: Usuário criado
/// </summary>
public class UserCreatedDomainEvent : DomainEvent
{
    public Guid CustomerId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Document { get; set; } = null!;
    public string UserType { get; set; } = null!;

    public override string GetEventType() => "usuario-criado";
}

/// <summary>
/// Evento: Usuário autenticado
/// </summary>
public class UserAuthenticatedDomainEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public DateTime AuthenticatedAt { get; set; }

    public override string GetEventType() => "usuario-autenticado";
}

/// <summary>
/// Evento: Sessão renovada
/// </summary>
public class SessionRenewedDomainEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public DateTime RenewedAt { get; set; }

    public override string GetEventType() => "sessao-renovada";
}

/// <summary>
/// Evento: Carteira criada
/// </summary>
public class WalletCreatedDomainEvent : DomainEvent
{
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public decimal InitialBalance { get; set; }

    public override string GetEventType() => "carteira-criada";
}

/// <summary>
/// Evento: Saldo creditado
/// </summary>
public class BalanceCreditedDomainEvent : DomainEvent
{
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;

    public override string GetEventType() => "saldo-creditado";
}

/// <summary>
/// Evento: Saldo debitado
/// </summary>
public class BalanceDebitedDomainEvent : DomainEvent
{
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public Guid? ChargingSessionId { get; set; }

    public override string GetEventType() => "saldo-debitado";
}

/// <summary>
/// Evento: Estação criada
/// </summary>
public class StationCreatedDomainEvent : DomainEvent
{
    public Guid StationId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal PricePerKwh { get; set; }

    public override string GetEventType() => "estacao-criada";
}

/// <summary>
/// Evento: Sessão de recarga criada
/// </summary>
public class ChargingSessionCreatedDomainEvent : DomainEvent
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public Guid StationId { get; set; }

    public override string GetEventType() => "sessao-criada";
}

/// <summary>
/// Evento: Sessão autorizada
/// </summary>
public class ChargingSessionAuthorizedDomainEvent : DomainEvent
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public DateTime AuthorizedAt { get; set; }

    public override string GetEventType() => "sessao-autorizada";
}

/// <summary>
/// Evento: Telemetria recebida
/// </summary>
public class TelemetryReceivedDomainEvent : DomainEvent
{
    public Guid SessionId { get; set; }
    public decimal CurrentKwh { get; set; }
    public decimal Voltage { get; set; }
    public decimal Current { get; set; }

    public override string GetEventType() => "telemetria-recebida";
}

/// <summary>
/// Evento: Consumo calculado
/// </summary>
public class EnergyConsumptionCalculatedDomainEvent : DomainEvent
{
    public Guid SessionId { get; set; }
    public decimal EnergyConsumedKwh { get; set; }

    public override string GetEventType() => "consumo-calculado";
}

/// <summary>
/// Evento: Cobrança gerada
/// </summary>
public class ChargeGeneratedDomainEvent : DomainEvent
{
    public Guid ChargeId { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public decimal EnergyKwh { get; set; }
    public decimal Amount { get; set; }

    public override string GetEventType() => "cobranca-gerada";
}

/// <summary>
/// Evento: Sessão finalizada
/// </summary>
public class ChargingSessionCompletedDomainEvent : DomainEvent
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public decimal EnergyConsumedKwh { get; set; }
    public decimal TotalAmount { get; set; }

    public override string GetEventType() => "sessao-finalizada";
}

/// <summary>
/// Evento: Recibo gerado
/// </summary>
public class ReceiptGeneratedDomainEvent : DomainEvent
{
    public Guid ReceiptId { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }

    public override string GetEventType() => "recibo-gerado";
}
