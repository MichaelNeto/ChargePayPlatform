using ChargePay.Domain.Entities;
using ChargePay.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChargePay.Infrastructure.Data;

/// <summary>
/// Contexto do banco de dados para a plataforma ChargePay
/// </summary>
public class ChargePayDbContext : DbContext
{
    public ChargePayDbContext(DbContextOptions<ChargePayDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ChargingStation> ChargingStations => Set<ChargingStation>();
    public DbSet<ChargingSession> ChargingSessions => Set<ChargingSession>();
    public DbSet<SessionTelemetry> SessionTelemetries => Set<SessionTelemetry>();
    public DbSet<SessionCharge> SessionCharges => Set<SessionCharge>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar tabelas
        ConfigureUserEntity(modelBuilder);
        ConfigureWalletEntity(modelBuilder);
        ConfigureWalletTransactionEntity(modelBuilder);
        ConfigureRefreshTokenEntity(modelBuilder);
        ConfigureChargingStationEntity(modelBuilder);
        ConfigureChargingSessionEntity(modelBuilder);
        ConfigureSessionTelemetryEntity(modelBuilder);
        ConfigureSessionChargeEntity(modelBuilder);
        ConfigureReceiptEntity(modelBuilder);
        ConfigureAuditLogEntity(modelBuilder);
        ConfigureNotificationEntity(modelBuilder);

        // Criar índices
        CreateIndexes(modelBuilder);
    }

    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<User>();

        builder.HasKey(u => u.UserId);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(80);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.BirthDate)
            .HasColumnType("timestamp without time zone")
            .IsRequired();
        builder.Property(u => u.Phone).IsRequired().HasMaxLength(11);

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value).Data!)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Document)
            .HasConversion(
                document => document.Value,
                value => Document.Create(value).Data!)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();

        // Índices
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Document).IsUnique();

        // Relações
        builder.HasOne(u => u.Wallet)
            .WithOne(w => w.User)
            .HasForeignKey<Wallet>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ChargingSessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureWalletEntity(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Wallet>();

        builder.HasKey(w => w.WalletId);
        builder.Property(w => w.UserId).IsRequired();
        builder.Property(w => w.Balance)
        .HasConversion(money => money.Amount, value => Money.FromCents(value))
        .IsRequired();

            builder.Property(w => w.MaxDailyLimit)
                .HasConversion(
                    money => money == null ? (long?)null : money.Amount,
                    value => value == null ? null : Money.FromCents(value.Value));

            builder.Property(w => w.MaxMonthlyLimit)
                .HasConversion(
        money => money == null ? (long?)null : money.Amount,
        value => value == null ? null : Money.FromCents(value.Value));
        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.UpdatedAt).IsRequired();

        builder.HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureWalletTransactionEntity(ModelBuilder modelBuilder)
{
    var builder = modelBuilder.Entity<WalletTransaction>();

    builder.HasKey(wt => wt.TransactionId);
    builder.Property(wt => wt.WalletId).IsRequired();
    builder.Property(wt => wt.Type).IsRequired();
    builder.Property(wt => wt.Description).IsRequired().HasMaxLength(500);
    builder.Property(wt => wt.CreatedAt).IsRequired();

    builder.Property(wt => wt.Amount)
        .HasConversion(money => money.Amount, value => Money.FromCents(value))
        .IsRequired();

    builder.HasOne(wt => wt.Wallet)
        .WithMany(w => w.Transactions)
        .HasForeignKey(wt => wt.WalletId)
        .OnDelete(DeleteBehavior.Cascade);
}

    private void ConfigureRefreshTokenEntity(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<RefreshToken>();

        builder.HasKey(rt => rt.TokenId);
        builder.Property(rt => rt.UserId).IsRequired();
        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
        builder.Property(rt => rt.ExpiresAt).IsRequired();
        builder.Property(rt => rt.CreatedAt).IsRequired();
    }

    private void ConfigureChargingStationEntity(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<ChargingStation>();

        builder.HasKey(s => s.StationId);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(255);
        builder.Property(s => s.Address).IsRequired().HasMaxLength(500);
        builder.Property(s => s.Latitude).IsRequired();
        builder.Property(s => s.Longitude).IsRequired();
        builder.Property(s => s.PricePerKwh)
    .HasConversion(money => money.Amount, value => Money.FromCents(value))
    .IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasMany(s => s.ChargingSessions)
            .WithOne(cs => cs.Station)
            .HasForeignKey(cs => cs.StationId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void ConfigureChargingSessionEntity(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<ChargingSession>();

        builder.HasKey(cs => cs.SessionId);
        builder.Property(cs => cs.UserId).IsRequired();
        builder.Property(cs => cs.StationId).IsRequired();
        builder.Property(cs => cs.Status).IsRequired();
        builder.Property(cs => cs.StartedAt).IsRequired();
        builder.Property(cs => cs.EnergyConsumedKwh).IsRequired();
        builder.Property(cs => cs.TotalAmount)
    .HasConversion(money => money.Amount, value => Money.FromCents(value))
    .IsRequired();

        builder.HasMany(cs => cs.Telemetries)
            .WithOne(t => t.Session)
            .HasForeignKey(t => t.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(cs => cs.Charges)
            .WithOne(c => c.Session)
            .HasForeignKey(c => c.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Receipt)
            .WithOne(r => r.Session)
            .HasForeignKey<Receipt>(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureSessionTelemetryEntity(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<SessionTelemetry>();

        builder.HasKey(t => t.TelemetryId);
        builder.Property(t => t.SessionId).IsRequired();
        builder.Property(t => t.CurrentKwh).IsRequired();
        builder.Property(t => t.PreviousKwh).IsRequired();
        builder.Property(t => t.KwhIncrement).IsRequired();
        builder.Property(t => t.ReceivedAt).IsRequired();
    }

    private void ConfigureSessionChargeEntity(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<SessionCharge>();

        builder.HasKey(c => c.ChargeId);
        builder.Property(c => c.SessionId).IsRequired();
        builder.Property(c => c.EnergyKwh).IsRequired();
        builder.Property(c => c.TariffPerKwh)
    .HasConversion(money => money.Amount, value => Money.FromCents(value))
    .IsRequired();
builder.Property(c => c.ChargeAmount)
    .HasConversion(money => money.Amount, value => Money.FromCents(value))
    .IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
    }

    private void ConfigureReceiptEntity(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Receipt>();

        builder.HasKey(r => r.ReceiptId);
        builder.Property(r => r.SessionId).IsRequired();
        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.WalletId).IsRequired();
        builder.Property(r => r.AmountDebited)
    .HasConversion(money => money.Amount, value => Money.FromCents(value))
    .IsRequired();
        builder.Property(r => r.StationName).IsRequired().HasMaxLength(255);
        builder.Property(r => r.ReceiptNumber).IsRequired().HasMaxLength(50);
        builder.Property(r => r.IssuedAt).IsRequired();
    }

    private void ConfigureAuditLogEntity(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<AuditLog>();

        builder.HasKey(a => a.LogId);
        builder.Property(a => a.EventType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityId).IsRequired();
        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Description).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.UserId);
    }

    private void ConfigureNotificationEntity(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Notification>();

        builder.HasKey(n => n.NotificationId);
        builder.Property(n => n.UserId).IsRequired();
        builder.Property(n => n.Title).IsRequired().HasMaxLength(255);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);
        builder.Property(n => n.Type).IsRequired().HasMaxLength(50);
        builder.Property(n => n.CreatedAt).IsRequired();

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.CreatedAt);
    }

    private void CreateIndexes(ModelBuilder modelBuilder)
    {
        // Índices de performance
        modelBuilder.Entity<WalletTransaction>()
            .HasIndex(wt => wt.WalletId);

        modelBuilder.Entity<ChargingSession>()
            .HasIndex(cs => new { cs.UserId, cs.StartedAt });

        modelBuilder.Entity<SessionTelemetry>()
            .HasIndex(st => st.SessionId);

        modelBuilder.Entity<Receipt>()
            .HasIndex(r => r.UserId);
    }
}
