using ChargePay.Domain.Entities;
using ChargePay.Domain.ValueObjects;
using ChargePay.Infrastructure.Data;
using ChargePay.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChargePay.Tests;

public class WalletRepositoryConcurrencyTests
{
  [Fact]
  public async Task ConfirmRecharge_ShouldPersistWalletChanges_WhenWalletIsTrackedInCurrentContext()
  {
    var userId = Guid.NewGuid();
    var options = new DbContextOptionsBuilder<ChargePayDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    await using (var context = new ChargePayDbContext(options))
    {
      var wallet = Wallet.Create(userId);
      var recharge = wallet.CreateRecharge(Money.FromCents(5000));

      Assert.True(recharge.IsSuccess);
      context.Wallets.Add(wallet);
      await context.SaveChangesAsync();
    }

    await using (var context = new ChargePayDbContext(options))
    {
      var repository = new WalletRepository(context);
      var wallet = await repository.GetByUserIdAsync(userId);

      Assert.NotNull(wallet);
      Assert.Single(wallet!.Recharges);

      var recharge = wallet.Recharges[0];
      var confirmation = recharge.ConfirmPayment();
      var creditResult = wallet.AddCredit(recharge.Amount);

      Assert.True(confirmation.IsSuccess);
      Assert.True(creditResult.IsSuccess);

      await repository.UpdateAsync(wallet);

      var persisted = await context.Wallets
          .Include(w => w.Recharges)
          .Include(w => w.Transactions)
          .SingleAsync(w => w.UserId == userId);

      Assert.Equal(5000, persisted.Balance.Amount);
      Assert.Equal(ChargePay.Domain.Enums.RechargeStatus.Paid, persisted.Recharges.Single().Status);
      Assert.Contains(persisted.Transactions, t => t.Type == ChargePay.Domain.Enums.TransactionType.Credit);
    }
  }
}
