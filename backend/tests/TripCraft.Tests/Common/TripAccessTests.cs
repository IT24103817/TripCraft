using FluentAssertions;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Identity;

namespace TripCraft.Tests.Common;

/// <summary>The shared "tourists only see their own trips" rule used by trips, workflows and quotations.</summary>
public class TripAccessTests
{
    private static readonly Guid Owner = Guid.NewGuid();

    [Fact]
    public void The_owning_tourist_and_staff_pass()
    {
        FluentActions.Invoking(() => TripAccess.EnsureCanAccess(new CurrentUser(Owner, UserRole.Tourist), Owner)).Should().NotThrow();
        FluentActions.Invoking(() => TripAccess.EnsureCanAccess(new CurrentUser(Guid.NewGuid(), UserRole.OperationsManager), Owner)).Should().NotThrow();
    }

    [Fact]
    public void Another_tourist_or_an_unknown_owner_is_forbidden()
    {
        FluentActions.Invoking(() => TripAccess.EnsureCanAccess(new CurrentUser(Guid.NewGuid(), UserRole.Tourist), Owner))
            .Should().Throw<ForbiddenException>();
        FluentActions.Invoking(() => TripAccess.EnsureCanAccess(new CurrentUser(Owner, UserRole.Tourist), null))
            .Should().Throw<ForbiddenException>();
    }
}
