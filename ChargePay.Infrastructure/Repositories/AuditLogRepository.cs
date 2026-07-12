using ChargePay.Domain.Entities;
using ChargePay.Domain.Repositories;
using ChargePay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChargePay.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de auditoria
/// </summary>
public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    private readonly ChargePayDbContext _context;

    public AuditLogRepository(ChargePayDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, int pageSize = 50)
    {
        return await _context.AuditLogs
            .Where(al => al.EntityType == entityType && al.EntityId == entityId)
            .OrderByDescending(al => al.CreatedAt)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetByUserAsync(Guid userId, int pageSize = 50)
    {
        return await _context.AuditLogs
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.CreatedAt)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.AuditLogs
            .Where(al => al.CreatedAt >= startDate && al.CreatedAt <= endDate)
            .OrderByDescending(al => al.CreatedAt)
            .ToListAsync();
    }
}
