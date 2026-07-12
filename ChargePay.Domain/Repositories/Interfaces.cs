using ChargePay.Domain.Entities;

namespace ChargePay.Domain.Repositories;

/// <summary>
/// Interface para repositório de usuários
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByDocumentAsync(string document);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid userId);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByDocumentAsync(string document);
}

/// <summary>
/// Interface para repositório de carteiras
/// </summary>
public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(Guid walletId);
    Task<Wallet?> GetByUserIdAsync(Guid userId);
    Task AddAsync(Wallet wallet);
    Task UpdateAsync(Wallet wallet);
    Task<List<WalletTransaction>> GetTransactionsAsync(Guid walletId, int pageSize = 50, int pageNumber = 1);
}

/// <summary>
/// Interface para repositório de estações
/// </summary>
public interface IChargingStationRepository
{
    Task<ChargingStation?> GetByIdAsync(Guid stationId);
    Task<List<ChargingStation>> GetAllActiveAsync();
    Task<List<ChargingStation>> GetByLocationAsync(decimal latitude, decimal longitude, double radiusKm = 5);
    Task AddAsync(ChargingStation station);
    Task UpdateAsync(ChargingStation station);
    Task<bool> ExistsAsync(Guid stationId);
}

/// <summary>
/// Interface para repositório de sessões de recarga
/// </summary>
public interface IChargingSessionRepository
{
    Task<ChargingSession?> GetByIdAsync(Guid sessionId);
    Task<List<ChargingSession>> GetUserSessionsAsync(Guid userId, int pageSize = 20, int pageNumber = 1);
    Task<List<ChargingSession>> GetUserSessionsByPeriodAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task AddAsync(ChargingSession session);
    Task UpdateAsync(ChargingSession session);
    Task<List<ChargingSession>> GetActiveSessionsAsync();
}

/// <summary>
/// Interface para repositório de recibos
/// </summary>
public interface IReceiptRepository
{
    Task<Receipt?> GetByIdAsync(Guid receiptId);
    Task<List<Receipt>> GetUserReceiptsAsync(Guid userId, int pageSize = 20, int pageNumber = 1);
    Task AddAsync(Receipt receipt);
    Task<Receipt?> GetBySessionIdAsync(Guid sessionId);
}

/// <summary>
/// Interface para repositório de auditoria
/// </summary>
public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<List<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, int pageSize = 50);
    Task<List<AuditLog>> GetByUserAsync(Guid userId, int pageSize = 50);
    Task<List<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}

/// <summary>
/// Interface para repositório de notificações
/// </summary>
public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid notificationId);
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int pageSize = 20, int pageNumber = 1);
    Task<List<Notification>> GetUnreadNotificationsAsync(Guid userId);
    Task AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
}

/// <summary>
/// Interface para repositório genérico
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}
