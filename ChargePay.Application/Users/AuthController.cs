using ChargePay.Application.Common;
using ChargePay.Domain.Entities;
using ChargePay.Domain.Enums;
using ChargePay.Domain.Events;
using ChargePay.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace ChargePay.Application.Users;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IConfiguration _configuration;

    public AuthController(IUserRepository userRepository, IEventPublisher eventPublisher, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var errors = new List<ErrorDetail>();

        if (!ModelState.IsValid)
        {
            foreach (var kvp in ModelState)
            {
                foreach (var error in kvp.Value.Errors)
                {
                    errors.Add(new ErrorDetail
                    {
                        Field = kvp.Key,
                        Code = "USR_002",
                        Type = ErrorType.Validation,
                        Message = string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Valor inválido." : error.ErrorMessage
                    });
                }
            }

            return BadRequest(ApiResponse<object>.Failure(
                ResponseCode.VALIDATION_ERROR,
                "Existem erros de validação.",
                errors,
                ApiHelper.CreateMetadata(HttpContext)));
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (user is null)
        {
            return Unauthorized(CreateAuthenticationFailureResponse());
        }

        if (user.Status == UserStatus.Locked || user.Status == UserStatus.Suspended || user.Status == UserStatus.Inactive)
        {
            return Unauthorized(CreateAuthenticationFailureResponse());
        }

        var isPasswordValid = BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            user.RecordFailedLogin();
            await _userRepository.UpdateAsync(user);

            return Unauthorized(CreateAuthenticationFailureResponse());
        }

        user.RecordSuccessfulLogin();
        await _userRepository.UpdateAsync(user);

        var accessToken = GenerateAccessToken(user);
        var refreshToken = Guid.NewGuid().ToString("N");
        var expiresIn = GetExpirationSeconds();

        await _eventPublisher.PublishAsync(new UserAuthenticatedDomainEvent
        {
            AggregateId = user.UserId,
            UserId = user.UserId,
            Email = user.Email.Value,
            AuthenticatedAt = DateTime.UtcNow
        });

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn
        };

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(
            response,
            "Login realizado com sucesso.",
            ApiHelper.CreateMetadata(HttpContext)));
    }

    private ApiResponse<object> CreateAuthenticationFailureResponse()
    {
        var errors = new List<ErrorDetail>
        {
            new()
            {
                Field = "email",
                Code = "USR_005",
                Type = ErrorType.Authentication,
                Message = "E-mail ou senha incorretos."
            }
        };

        return ApiResponse<object>.Failure(
            ResponseCode.UNAUTHORIZED,
            "E-mail ou senha incorretos.",
            errors,
            ApiHelper.CreateMetadata(HttpContext));
    }

    private string GenerateAccessToken(User user)
    {
        var secret = _configuration["Jwt:Secret"] ?? "seu_super_secreto_chave_de_desenvolvimento_aqui_123456";
        var issuer = _configuration["Jwt:Issuer"] ?? "ChargePay";
        var audience = _configuration["Jwt:Audience"] ?? "ChargePayApi";
        var expirationMinutes = GetExpirationMinutes();

        var key = Encoding.ASCII.GetBytes(secret);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email.Value)
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }

    private int GetExpirationMinutes()
    {
        return Math.Max(1, _configuration.GetValue<int?>("Jwt:ExpirationMinutes") ?? 60);
    }

    private int GetExpirationSeconds()
    {
        return GetExpirationMinutes() * 60;
    }
}
