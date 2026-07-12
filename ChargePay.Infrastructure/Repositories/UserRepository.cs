using ChargePay.Domain.Entities;
using ChargePay.Domain.Repositories;
using ChargePay.Domain.ValueObjects;
using ChargePay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChargePay.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de usuários
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly ChargePayDbContext _context;

    public UserRepository(ChargePayDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var emailResult = Email.Create(email);
        if (!emailResult.IsSuccess)
            return null;

        return await _context.Users.FirstOrDefaultAsync(u => u.Email == emailResult.Data);
    }

    public async Task<User?> GetByDocumentAsync(string document)
    {
        var documentResult = Document.Create(document);
        if (!documentResult.IsSuccess)
            return null;

        return await _context.Users.FirstOrDefaultAsync(u => u.Document == documentResult.Data);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var emailResult = Email.Create(email);
        if (!emailResult.IsSuccess)
            return false;

        return await _context.Users.AnyAsync(u => u.Email == emailResult.Data);
    }

    public async Task<bool> ExistsByDocumentAsync(string document)
    {
        var documentResult = Document.Create(document);
        if (!documentResult.IsSuccess)
            return false;

        return await _context.Users.AnyAsync(u => u.Document == documentResult.Data);
    }
}