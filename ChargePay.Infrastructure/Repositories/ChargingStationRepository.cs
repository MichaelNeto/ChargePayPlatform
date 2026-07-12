using ChargePay.Domain.Entities;
using ChargePay.Domain.Repositories;
using ChargePay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChargePay.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de estações
/// </summary>
public class ChargingStationRepository : GenericRepository<ChargingStation>, IChargingStationRepository
{
    private readonly ChargePayDbContext _context;
    private const double EarthRadiusKm = 6371;

    public ChargingStationRepository(ChargePayDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<ChargingStation>> GetAllActiveAsync()
    {
        return await _context.ChargingStations
            .Where(s => s.Status == Domain.Enums.StationStatus.Active)
            .ToListAsync();
    }

    public async Task<List<ChargingStation>> GetByLocationAsync(decimal latitude, decimal longitude, double radiusKm = 5)
    {
        var stations = await _context.ChargingStations
            .Where(s => s.Status == Domain.Enums.StationStatus.Active)
            .ToListAsync();

        return stations.Where(s => 
        {
            var distance = CalculateDistance((double)latitude, (double)longitude, (double)s.Latitude, (double)s.Longitude);
            return distance <= radiusKm;
        }).ToList();
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    public async Task<bool> ExistsAsync(Guid stationId)
    {
        return await _context.ChargingStations.AnyAsync(s => s.StationId == stationId);
    }
}
