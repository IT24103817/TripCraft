using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Identity;
using TripCraft.Infrastructure.Persistence.Auditing;

namespace TripCraft.Tests.Shared.Database;

/// <summary>The audit_logs ⟕ users join, filters and ordering translate to real PostgreSQL SQL.</summary>
[Collection(PostgresCollection.Name)]
public class AuditLogReaderPostgresTests(PostgresFixture postgres) : IAsyncLifetime
{
    private string _connectionString = string.Empty;

    public async Task InitializeAsync() => _connectionString = await postgres.CreateMigratedDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Rows_come_back_with_the_actor_or_as_system_and_filter_by_entity_and_text()
    {
        var entityId = Guid.NewGuid();
        await using (var db = PostgresFixture.CreateContext(_connectionString))
        {
            var user = new User { Email = "audit@tripcraft.test", FullName = "Audit", PasswordHash = "hash", Role = UserRole.OperationsManager };
            db.Users.Add(user);
            db.AuditLogs.AddRange(
                new AuditLog { ActorId = user.Id, Action = "TripRequestStatusChanged", Entity = "TripRequest", EntityId = entityId,
                               Before = """{"status":"Submitted"}""", After = """{"status":"Cancelled"}""", At = DateTime.UtcNow.AddMinutes(-1) },
                new AuditLog { ActorId = null, Action = "AgentProposalReceived", Entity = "AgentWorkflow", EntityId = entityId,
                               After = """{"status":"FailedSafely"}""", At = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        await using var read = PostgresFixture.CreateContext(_connectionString);
        var rows = await new AuditLogReader(read).Query()
            .Where(a => a.EntityId == entityId)
            .OrderBy(a => a.At)
            .ToListAsync();

        rows.Should().HaveCount(2);
        rows[0].ActorEmail.Should().Be("audit@tripcraft.test");
        rows[0].ActorRole.Should().Be(UserRole.OperationsManager);
        rows[1].ActorEmail.Should().BeNull(); // system row: the left join keeps it
        rows[1].ActorRole.Should().BeNull();

        var searched = await new AuditLogReader(read).Query()
            .Where(a => a.EntityId == entityId && a.ActorEmail != null && a.ActorEmail.ToLower().Contains("audit@"))
            .ToListAsync();
        searched.Should().ContainSingle().Which.Action.Should().Be("TripRequestStatusChanged");
    }
}
