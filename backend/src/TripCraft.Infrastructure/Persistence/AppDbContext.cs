using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Entities;
using TripCraft.Application.Identity;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.External;

namespace TripCraft.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();

    // Component A — Trip Requests & Itinerary
    public DbSet<Tourist> Tourists => Set<Tourist>();
    public DbSet<TripRequest> TripRequests => Set<TripRequest>();
    public DbSet<Attraction> Attractions => Set<Attraction>();
    public DbSet<Itinerary> Itineraries => Set<Itinerary>();
    public DbSet<ItineraryDay> ItineraryDays => Set<ItineraryDay>();
    public DbSet<ItineraryStop> ItineraryStops => Set<ItineraryStop>();

    // Owned by Component C; created early for start-planning and audit rows.
    public DbSet<AgentWorkflow> AgentWorkflows => Set<AgentWorkflow>();
    public DbSet<AgentStep> AgentSteps => Set<AgentStep>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Static fallback for the distance provider (PLAN.md section 9).
    public DbSet<CityDistance> CityDistances => Set<CityDistance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration in Persistence/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Money columns are numeric(12,2) everywhere (CLAUDE.md rule).
        configurationBuilder.Properties<decimal>().HavePrecision(12, 2);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>IUnitOfWork: one SaveChanges call is one database transaction.</summary>
    Task IUnitOfWork.SaveChangesAsync(CancellationToken ct) => SaveChangesAsync(ct);

    async Task<IUnitOfWorkTransaction> IUnitOfWork.BeginTransactionAsync(CancellationToken ct) =>
        new EfTransaction(await Database.BeginTransactionAsync(ct));

    void IUnitOfWork.DiscardChanges() => ChangeTracker.Clear();

    /// <summary>Wraps an EF Core transaction. Disposing it without CommitAsync rolls back.</summary>
    private sealed class EfTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
    {
        public Task CommitAsync(CancellationToken ct) => transaction.CommitAsync(ct);
        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    private void SetTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
