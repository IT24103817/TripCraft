using FluentAssertions;
using TripCraft.Application.Common.Settings;

namespace TripCraft.Tests.Common;

public class TripSettingsTests
{
    [Fact]
    public void Cancel_until_is_the_cutoff_number_of_days_before_the_start()
    {
        new TripSettings(3, "ops@x.test").CancelUntil(new DateOnly(2026, 11, 10)).Should().Be(new DateOnly(2026, 11, 7));
        new TripSettings(0, "ops@x.test").CancelUntil(new DateOnly(2026, 11, 10)).Should().Be(new DateOnly(2026, 11, 10));
    }

    [Fact]
    public void Today_is_the_operators_date_not_the_servers_utc_date()
    {
        var colombo = new TripSettings(3, "ops@x.test", "Asia/Colombo").Today();
        var expected = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5.5)); // Sri Lanka is UTC+5:30, no daylight saving

        colombo.Should().Be(expected);
    }
}
