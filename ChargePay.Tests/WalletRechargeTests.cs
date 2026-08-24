using ChargePay.Domain.Entities;
using ChargePay.Domain.Enums;
using ChargePay.Domain.ValueObjects;
using Xunit;

namespace ChargePay.Tests;

public class WalletRechargeTests
{
  [Fact]
  public void ShouldCreatePendingRechargeAndDisplayQrCode()
  {
    var userId = Guid.NewGuid();
    var wallet = Wallet.Create(userId);
    var amount = Money.FromCents(5000);

    var recharge = wallet.CreateRecharge(amount);

    Assert.True(recharge.IsSuccess);
    Assert.Equal(5000, recharge.Data!.Amount.Amount);
    Assert.Equal(RechargeStatus.Pending, recharge.Data.Status);
    Assert.NotNull(recharge.Data.QrCode);
    Assert.NotEmpty(recharge.Data.QrCode);
  }

  [Fact]
  public void ShouldConfirmPaymentCreditBalanceAndRegisterTransaction()
  {
    var userId = Guid.NewGuid();
    var wallet = Wallet.Create(userId);
    var amount = Money.FromCents(5000);

    var recharge = wallet.CreateRecharge(amount);

    var confirmation = wallet.ConfirmRecharge(
        recharge.Data!.RechargeId
    );

    Assert.True(confirmation.IsSuccess);
    Assert.Equal(RechargeStatus.Paid, recharge.Data.Status);
    Assert.Equal(5000, wallet.Balance.Amount);
    Assert.Contains(wallet.Transactions, t =>
        t.Type == TransactionType.Credit &&
        t.Amount.Amount == 5000 &&
        t.Description.Contains("Recarga via Pix"));
  }
}
