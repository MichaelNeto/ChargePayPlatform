using ChargePay.Domain.Entities;
using ChargePay.Domain.Repositories;
using ChargePay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChargePay.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de carteiras
/// </summary>
public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
{
    private readonly ChargePayDbContext _context;

    public WalletRepository(ChargePayDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task<List<WalletTransaction>> GetTransactionsAsync(Guid walletId, int pageSize = 50, int pageNumber = 1)
    {
        return await _context.WalletTransactions
            .Where(wt => wt.WalletId == walletId)
            .OrderByDescending(wt => wt.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
