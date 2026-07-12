using ChargePay.Domain.Entities;
using ChargePay.Domain.Repositories;
using ChargePay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChargePay.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de notificações
/// </summary>
public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    private readonly ChargePayDbContext _context;

    public NotificationRepository(ChargePayDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int pageSize = 20, int pageNumber = 1)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
}
