using ChargePay.Domain.Entities;
using ChargePay.Domain.Repositories;
using ChargePay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChargePay.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de recibos
/// </summary>
public class ReceiptRepository : GenericRepository<Receipt>, IReceiptRepository
{
    private readonly ChargePayDbContext _context;

    public ReceiptRepository(ChargePayDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Receipt>> GetUserReceiptsAsync(Guid userId, int pageSize = 20, int pageNumber = 1)
    {
        return await _context.Receipts
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.IssuedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Receipt?> GetBySessionIdAsync(Guid sessionId)
    {
        return await _context.Receipts.FirstOrDefaultAsync(r => r.SessionId == sessionId);
    }
}
