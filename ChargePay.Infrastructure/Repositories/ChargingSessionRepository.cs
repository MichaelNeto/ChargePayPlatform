using ChargePay.Domain.Entities;
using ChargePay.Domain.Repositories;
using ChargePay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChargePay.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de sessões de recarga
/// </summary>
public class ChargingSessionRepository : GenericRepository<ChargingSession>, IChargingSessionRepository
{
    private readonly ChargePayDbContext _context;

    public ChargingSessionRepository(ChargePayDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<ChargingSession>> GetUserSessionsAsync(Guid userId, int pageSize = 20, int pageNumber = 1)
    {
        return await _context.ChargingSessions
            .Where(cs => cs.UserId == userId)
            .Include(cs => cs.Charges)
            .Include(cs => cs.Receipt)
            .OrderByDescending(cs => cs.StartedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<ChargingSession>> GetUserSessionsByPeriodAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await _context.ChargingSessions
            .Where(cs => cs.UserId == userId && cs.StartedAt >= startDate && cs.StartedAt <= endDate)
            .Include(cs => cs.Charges)
            .Include(cs => cs.Receipt)
            .OrderByDescending(cs => cs.StartedAt)
            .ToListAsync();
    }

    public async Task<List<ChargingSession>> GetActiveSessionsAsync()
    {
        return await _context.ChargingSessions
            .Where(cs => cs.Status == Domain.Enums.SessionStatus.InProgress)
            .ToListAsync();
    }
}
