using System.Security.Claims;
using ChargePay.Application.Common;
using ChargePay.Domain.Entities;
using ChargePay.Domain.Enums;
using ChargePay.Domain.Repositories;
using ChargePay.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargePay.Application.Users;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
  private readonly IUserRepository _userRepository;
  private readonly IWalletRepository _walletRepository;

  public WalletController(IUserRepository userRepository, IWalletRepository walletRepository)
  {
    _userRepository = userRepository;
    _walletRepository = walletRepository;
  }

  [HttpGet]
  public async Task<IActionResult> GetWallet()
  {
    var userId = GetCurrentUserId();
    if (userId is null)
      return Unauthorized();

    var wallet = await _walletRepository.GetByUserIdAsync(userId.Value);
    if (wallet is null)
    {
      var user = await _userRepository.GetByIdAsync(userId.Value);
      if (user is null)
        return NotFound();

      wallet = Wallet.Create(userId.Value);
      await _walletRepository.AddAsync(wallet);
    }

    var transactions = await _walletRepository.GetTransactionsAsync(wallet.WalletId, 50, 1);

    var response = new WalletSummaryResponse
    {
      WalletId = wallet.WalletId,
      UserId = wallet.UserId,
      Balance = wallet.Balance.ToDecimal(),
      AvailableValues = new[] { 50m, 100m, 200m, 300m },
      Transactions = transactions
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new WalletTransactionResponse
            {
              TransactionId = t.TransactionId,
              Type = t.Type.ToString(),
              Description = t.Description,
              Amount = t.Amount.ToDecimal(),
              CreatedAt = t.CreatedAt
            })
            .ToList()
    };

    return Ok(ApiResponse<WalletSummaryResponse>.SuccessResponse(
        response,
        "Carteira carregada com sucesso.",
        ApiHelper.CreateMetadata(HttpContext)));
  }

  [HttpPost("recharges")]
  public async Task<IActionResult> CreateRecharge(
    [FromBody] CreateRechargeRequest request)
  {
    var userId = GetCurrentUserId();

    if (userId is null)
      return Unauthorized();

    if (request.Amount <= 0)
    {
      return BadRequest(ApiResponse<object>.Failure(
          ResponseCode.VALIDATION_ERROR,
          "Valor inválido.",
          new List<ErrorDetail>
          {
                new()
                {
                    Field = "amount",
                    Code = "WAL_001",
                    Type = ErrorType.Validation,
                    Message = "Informe um valor válido."
                }
          },
          ApiHelper.CreateMetadata(HttpContext)));
    }

    var validValues = new[] { 50m, 100m, 200m, 300m };

    if (!validValues.Contains(request.Amount))
    {
      return BadRequest(ApiResponse<object>.Failure(
          ResponseCode.VALIDATION_ERROR,
          "Valor de recarga não permitido.",
          new List<ErrorDetail>
          {
                new()
                {
                    Field = "amount",
                    Code = "WAL_002",
                    Type = ErrorType.Validation,
                    Message = "Escolha um valor pré-definido: R$ 50, R$ 100, R$ 200 ou R$ 300."
                }
          },
          ApiHelper.CreateMetadata(HttpContext)));
    }

    var wallet = await _walletRepository.GetByUserIdAsync(userId.Value);

    var isNewWallet = wallet is null;

    if (isNewWallet)
    {
      wallet = Wallet.Create(userId.Value);
    }

    var rechargeResult = wallet!.CreateRecharge(
        Money.FromCents((long)(request.Amount * 100m))
    );

    if (!rechargeResult.IsSuccess)
    {
      return BadRequest(ApiResponse<object>.Failure(
          ResponseCode.BUSINESS_ERROR,
          rechargeResult.ErrorMessage!,
          new List<ErrorDetail>
          {
                new()
                {
                    Field = "amount",
                    Code = "WAL_003",
                    Type = ErrorType.Business,
                    Message = rechargeResult.ErrorMessage!
                }
          },
          ApiHelper.CreateMetadata(HttpContext)));
    }

    var recharge = rechargeResult.Data!;

    if (isNewWallet)
    {
      // Carteira ainda não existe no banco.
      // Como a recarga pertence à carteira,
      // o EF poderá persistir o agregado completo.
      await _walletRepository.AddAsync(wallet);
    }
    else
    {
      // Carteira já existe.
      // Persistimos somente a nova recarga.
      await _walletRepository.AddRechargeAsync(recharge);
    }

    return Ok(ApiResponse<WalletRechargeResponse>.SuccessResponse(
        new WalletRechargeResponse
        {
          RechargeId = recharge.RechargeId,
          WalletId = recharge.WalletId,
          Amount = recharge.Amount.ToDecimal(),
          Status = recharge.Status.ToString(),
          QrCode = recharge.QrCode,
          CreatedAt = recharge.CreatedAt
        },
        "Cobrança pendente gerada com sucesso.",
        ApiHelper.CreateMetadata(HttpContext)));
  }

  [HttpPost("recharges/{rechargeId:guid}/confirm")]
  public async Task<IActionResult> ConfirmRecharge(Guid rechargeId)
  {
    var userId = GetCurrentUserId();
    if (userId is null)
      return Unauthorized();

    var wallet = await _walletRepository.GetByUserIdAsync(userId.Value);
    if (wallet is null)
      return NotFound(ApiResponse<object>.Failure(
          ResponseCode.NOT_FOUND,
          "Carteira não encontrada.",
          new List<ErrorDetail>
          {
                    new() { Field = "wallet", Code = "WAL_004", Type = ErrorType.NotFound, Message = "Carteira não encontrada." }
          },
          ApiHelper.CreateMetadata(HttpContext)));

    var recharge = wallet.Recharges.FirstOrDefault(r => r.RechargeId == rechargeId);
    if (recharge is null)
      return NotFound(ApiResponse<object>.Failure(
          ResponseCode.NOT_FOUND,
          "Cobrança não encontrada.",
          new List<ErrorDetail>
          {
                    new() { Field = "rechargeId", Code = "WAL_005", Type = ErrorType.NotFound, Message = "Cobrança não encontrada." }
          },
          ApiHelper.CreateMetadata(HttpContext)));

    var confirmation = recharge.ConfirmPayment();
    if (!confirmation.IsSuccess)
      return BadRequest(ApiResponse<object>.Failure(
          ResponseCode.BUSINESS_ERROR,
          confirmation.ErrorMessage!,
          new List<ErrorDetail>
          {
                    new() { Field = "rechargeId", Code = "WAL_006", Type = ErrorType.Business, Message = confirmation.ErrorMessage! }
          },
          ApiHelper.CreateMetadata(HttpContext)));

    var creditResult = wallet.AddCredit(recharge.Amount);
    if (!creditResult.IsSuccess)
      return BadRequest(ApiResponse<object>.Failure(
          ResponseCode.BUSINESS_ERROR,
          creditResult.ErrorMessage!,
          new List<ErrorDetail>
          {
                    new() { Field = "wallet", Code = "WAL_007", Type = ErrorType.Business, Message = creditResult.ErrorMessage! }
          },
          ApiHelper.CreateMetadata(HttpContext)));

    await _walletRepository.SaveChangesAsync();

    return Ok(ApiResponse<WalletRechargeResponse>.SuccessResponse(
        new WalletRechargeResponse
        {
          RechargeId = recharge.RechargeId,
          WalletId = recharge.WalletId,
          Amount = recharge.Amount.ToDecimal(),
          Status = recharge.Status.ToString(),
          QrCode = recharge.QrCode,
          CreatedAt = recharge.CreatedAt,
          PaidAt = recharge.PaidAt
        },
        "Pagamento confirmado com sucesso.",
        ApiHelper.CreateMetadata(HttpContext)));
  }

  private Guid? GetCurrentUserId()
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
  }
}

public class CreateRechargeRequest
{
  public decimal Amount { get; set; }
}

public class WalletSummaryResponse
{
  public Guid WalletId { get; set; }
  public Guid UserId { get; set; }
  public decimal Balance { get; set; }
  public decimal[] AvailableValues { get; set; } = Array.Empty<decimal>();
  public List<WalletTransactionResponse> Transactions { get; set; } = new();
}

public class WalletTransactionResponse
{
  public Guid TransactionId { get; set; }
  public string Type { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public decimal Amount { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class WalletRechargeResponse
{
  public Guid RechargeId { get; set; }
  public Guid WalletId { get; set; }
  public decimal Amount { get; set; }
  public string Status { get; set; } = string.Empty;
  public string QrCode { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public DateTime? PaidAt { get; set; }
}
