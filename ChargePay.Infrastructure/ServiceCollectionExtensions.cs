using ChargePay.Domain.Events;
using ChargePay.Domain.Repositories;
using ChargePay.Infrastructure.Data;
using ChargePay.Infrastructure.Events;
using ChargePay.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChargePay.Infrastructure;

/// <summary>
/// Extensões para registrar serviços de infraestrutura
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adicionar configurações de dados (EF Core, Repositories)
    /// </summary>
    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Registrar DbContext
        services.AddDbContext<ChargePayDbContext>(options =>
            options.UseNpgsql(connectionString)
                .EnableSensitiveDataLogging(false)
                .EnableDetailedErrors(false)
        );

        // Registrar Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IChargingStationRepository, ChargingStationRepository>();
        services.AddScoped<IChargingSessionRepository, ChargingSessionRepository>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        return services;
    }

    /// <summary>
    /// Executar migrações automaticamente
    /// </summary>
    public static IServiceProvider MigrateDatabase(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChargePayDbContext>();

        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            // Log do erro
            Console.Error.WriteLine($"Erro ao executar migrações: {ex.Message}");
            throw;
        }

        return services;
    }
}
