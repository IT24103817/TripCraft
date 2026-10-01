using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Identity.Dtos;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Identity;

public class AuditLogActorFilterTests
{
    [Fact]
    public async Task Audit_log_filters_by_actor_email_and_by_system_rows()
    {
        await using var factory = new TestWebApplicationFactory();
        await factory.RunToProposalAsync(); // tourist rows, and system rows for the proposal
        var admin = await factory.CreateClientAsAsync("admin1@tripcraft.test");

        var tourist = await admin.GetFromJsonAsync<PagedResult<AuditLogDto>>("/api/admin/audit-logs?actor=TOURIST1&pageSize=100", TestJson.Options);
        var system = await admin.GetFromJsonAsync<PagedResult<AuditLogDto>>("/api/admin/audit-logs?actor=system&pageSize=100", TestJson.Options);

        tourist!.Items.Should().NotBeEmpty().And.OnlyContain(a => a.ActorEmail == "tourist1@tripcraft.test");
        system!.Items.Should().NotBeEmpty().And.OnlyContain(a => a.ActorEmail == null);
        system.Items.Should().Contain(a => a.Action == "AgentProposalReceived");
    }
}
