using ChargePay.Application.Common;
using ChargePay.Domain.Entities;
using ChargePay.Domain.Enums;
using ChargePay.Domain.Events;
using ChargePay.Domain.Repositories;
using ChargePay.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using BCrypt.Net;

namespace ChargePay.Application.Users;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher;

    public UserController(IUserRepository userRepository, IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
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
                        Code = "CUS_001",
                        Type = ErrorType.Validation,
                        Message = string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Valor inválido." : error.ErrorMessage
                    });
                }
            }
        }

        var emailResult = Email.Create(request.Email);
        if (!emailResult.IsSuccess)
            errors.Add(new ErrorDetail { Field = "email", Code = "USR_002", Type = ErrorType.Validation, Message = emailResult.ErrorMessage! });

        var documentResult = Document.Create(request.Document);
        if (!documentResult.IsSuccess)
            errors.Add(new ErrorDetail { Field = "document", Code = "CUS_002", Type = ErrorType.Validation, Message = documentResult.ErrorMessage! });

        if (request.BirthDate.Date >= DateTime.UtcNow.Date)
            errors.Add(new ErrorDetail { Field = "birthDate", Code = "CUS_003", Type = ErrorType.Validation, Message = "Data de nascimento inválida." });

        var age = CalculateAge(request.BirthDate);
        if (age < 18)
            errors.Add(new ErrorDetail { Field = "birthDate", Code = "CUS_004", Type = ErrorType.Validation, Message = "É necessário possuir idade mínima de 18 anos." });

        if (!Regex.IsMatch(request.Password, "^\\d{6}$"))
            errors.Add(new ErrorDetail { Field = "password", Code = "USR_003", Type = ErrorType.Validation, Message = "Senha deve conter exatamente 6 dígitos numéricos." });

        if (!Regex.IsMatch(request.Phone, "^\\d{11}$"))
            errors.Add(new ErrorDetail { Field = "phone", Code = "CUS_005", Type = ErrorType.Validation, Message = "Telefone inválido. Deve conter DDD + número com 11 dígitos." });

        if (await _userRepository.ExistsByEmailAsync(request.Email.Trim().ToLower()))
            errors.Add(new ErrorDetail { Field = "email", Code = "USR_004", Type = ErrorType.Validation, Message = "E-mail já cadastrado." });

        if (documentResult.IsSuccess && await _userRepository.ExistsByDocumentAsync(documentResult.Data!.Value))
            errors.Add(new ErrorDetail { Field = "document", Code = "CUS_006", Type = ErrorType.Validation, Message = "Documento já cadastrado." });

        if (errors.Any())
        {
            return BadRequest(ApiResponse<object>.Failure(
                ResponseCode.VALIDATION_ERROR,
                "Existem erros de validação.",
                errors,
                ApiHelper.CreateMetadata(HttpContext)
            ));
        }

        var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);

        var userResult = ChargePay.Domain.Entities.User.Create(
            request.FirstName,
            request.LastName,
            request.BirthDate,
            request.Phone,
            emailResult.Data!,
            documentResult.Data!,
            documentResult.Data!.Type == "CPF" ? UserType.Individual : UserType.Company,
            passwordHash);

        if (!userResult.IsSuccess)
        {
            errors.Add(new ErrorDetail
            {
                Field = "user",
                Code = "USR_001",
                Type = ErrorType.Business,
                Message = userResult.ErrorMessage!
            });

            return BadRequest(ApiResponse<object>.Failure(
                ResponseCode.BUSINESS_ERROR,
                "Operação não permitida.",
                errors,
                ApiHelper.CreateMetadata(HttpContext)
            ));
        }

        var user = userResult.Data!;
        await _userRepository.AddAsync(user);

        var domainEvent = new UserCreatedDomainEvent
        {
            AggregateId = user.UserId,
            CustomerId = user.UserId,
            UserId = user.UserId,
            Name = user.FullName,
            Email = user.Email.Value,
            Document = user.Document.Value,
            UserType = user.Type.ToString()
        };

        await _eventPublisher.PublishAsync(domainEvent);

        var responseData = new
        {
            customerId = user.UserId,
            userId = user.UserId
        };

        return CreatedAtAction(nameof(Create), ApiResponse<object>.SuccessResponse(
            responseData,
            "Cliente cadastrado com sucesso.",
            ApiHelper.CreateMetadata(HttpContext)
        ));
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - birthDate.Date.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;

        return age;
    }
}

