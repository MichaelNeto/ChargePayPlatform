using ChargePay.Domain.Enums;
using ChargePay.Domain.ValueObjects;

namespace ChargePay.Domain.Entities;

public class WalletRecharge
{
  public Guid RechargeId { get; private set; }
  public Guid WalletId { get; private set; }
  public Money Amount { get; private set; } = null!;
  public RechargeStatus Status { get; private set; }
  public string QrCode { get; private set; } = null!;
  public DateTime CreatedAt { get; private set; }
  public DateTime? PaidAt { get; private set; }

  public virtual Wallet? Wallet { get; private set; }

  private WalletRecharge() { }

  public static WalletRecharge Create(Guid walletId, Money amount)
  {
    return new WalletRecharge
    {
      RechargeId = Guid.NewGuid(),
      WalletId = walletId,
      Amount = amount,
      Status = RechargeStatus.Pending,
      QrCode = $"pix-simulado:{Guid.NewGuid():N}:{amount.Amount}",
      CreatedAt = DateTime.UtcNow
    };
  }

  public Result<WalletRecharge> ConfirmPayment()
  {
    if (Status != RechargeStatus.Pending)
      return Result<WalletRecharge>.Failure("Cobrança não está pendente.");

    Status = RechargeStatus.Paid;
    PaidAt = DateTime.UtcNow;

    return Result<WalletRecharge>.Success(this);
  }
}
