using ChargePay.Domain.Entities;
using ChargePay.Domain.Repositories;
using ChargePay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChargePay.Infrastructure.Repositories;

public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
{
    private readonly ChargePayDbContext _context;

    public WalletRepository(ChargePayDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Wallets
            .Include(w => w.Transactions)
            .Include(w => w.Recharges)
            .FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task<List<WalletTransaction>> GetTransactionsAsync(
        Guid walletId,
        int pageSize = 50,
        int pageNumber = 1)
    {
        return await _context.WalletTransactions
            .Where(wt => wt.WalletId == walletId)
            .OrderByDescending(wt => wt.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task AddRechargeAsync(WalletRecharge recharge)
    {
        await _context.WalletRecharges.AddAsync(recharge);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        foreach (var entry in _context.ChangeTracker.Entries<WalletTransaction>())
        {
            if (entry.State == EntityState.Modified)
            {
                var transactionId = entry.Entity.TransactionId;

                var exists = await _context.WalletTransactions
                    .AsNoTracking()
                    .AnyAsync(t => t.TransactionId == transactionId);

                if (!exists)
                {
                    entry.State = EntityState.Added;
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    public override async Task UpdateAsync(Wallet entity)
    {
        var trackedEntity = _context.ChangeTracker
            .Entries<Wallet>()
            .FirstOrDefault(e => e.Entity.WalletId == entity.WalletId);

        if (trackedEntity is not null)
        {
            await _context.SaveChangesAsync();
            return;
        }

        var existingWallet = await _context.Wallets
            .SingleOrDefaultAsync(w => w.WalletId == entity.WalletId);

        if (existingWallet is null)
            throw new InvalidOperationException(
                $"Carteira {entity.WalletId} não encontrada para atualização.");

        _context.Entry(existingWallet).CurrentValues.SetValues(entity);

        await _context.SaveChangesAsync();
    }
}
