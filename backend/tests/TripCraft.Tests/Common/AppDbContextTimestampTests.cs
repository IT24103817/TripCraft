using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Identity;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Tests.Common;

public class AppDbContextTimestampTests
{
    private static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task SaveChanges_sets_CreatedAt_and_UpdatedAt_on_insert()
    {
        await using var db = NewContext();
        var user = new User { Email = "a@b.c", PasswordHash = "x", FullName = "A" };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.UpdatedAt.Should().Be(user.CreatedAt);
    }

    [Fact]
    public async Task SaveChanges_only_moves_UpdatedAt_on_update()
    {
        await using var db = NewContext();
        var user = new User { Email = "a@b.c", PasswordHash = "x", FullName = "A" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var createdAt = user.CreatedAt;

        await Task.Delay(20);
        user.FullName = "B";
        await db.SaveChangesAsync();

        user.CreatedAt.Should().Be(createdAt);
        user.UpdatedAt.Should().BeAfter(createdAt);
    }
}
