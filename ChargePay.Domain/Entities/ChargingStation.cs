using ChargePay.Domain.Enums;
using ChargePay.Domain.ValueObjects;

namespace ChargePay.Domain.Entities;

/// <summary>
/// Entidade que representa uma estação de recarga
/// </summary>
public class ChargingStation
{
    public Guid StationId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public Money PricePerKwh { get; private set; } = null!;
    public StationStatus Status { get; private set; }
    public int TotalConnectors { get; private set; }
    public int AvailableConnectors { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Relações
    public virtual List<ChargingSession> ChargingSessions { get; private set; } = new();

    private ChargingStation() { }

    /// <summary>
    /// Criar uma nova estação de recarga
    /// </summary>
    public static Result<ChargingStation> Create(
        string name,
        string address,
        decimal latitude,
        decimal longitude,
        Money pricePerKwh,
        int totalConnectors)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<ChargingStation>.Failure("Nome da estação é obrigatório");

        if (totalConnectors <= 0)
            return Result<ChargingStation>.Failure("Total de conectores deve ser maior que zero");

        if (latitude < -90 || latitude > 90)
            return Result<ChargingStation>.Failure("Latitude inválida");

        if (longitude < -180 || longitude > 180)
            return Result<ChargingStation>.Failure("Longitude inválida");

        var station = new ChargingStation
        {
            StationId = Guid.NewGuid(),
            Name = name.Trim(),
            Address = address.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            PricePerKwh = pricePerKwh,
            Status = StationStatus.Active,
            TotalConnectors = totalConnectors,
            AvailableConnectors = totalConnectors,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return Result<ChargingStation>.Success(station);
    }

    /// <summary>
    /// Verificar se há conectores disponíveis
    /// </summary>
    public bool HasAvailableConnectors() => AvailableConnectors > 0;

    /// <summary>
    /// Alocar um conector
    /// </summary>
    public Result<bool> AllocateConnector()
    {
        if (!HasAvailableConnectors())
            return Result<bool>.Failure("Nenhum conector disponível");

        AvailableConnectors--;
        UpdatedAt = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Liberar um conector
    /// </summary>
    public void ReleaseConnector()
    {
        if (AvailableConnectors < TotalConnectors)
        {
            AvailableConnectors++;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Definir status da estação
    /// </summary>
    public void SetStatus(StationStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
