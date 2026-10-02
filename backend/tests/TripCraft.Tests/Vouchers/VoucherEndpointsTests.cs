using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Vouchers;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Vouchers;

/// <summary>GET /api/trips/{id}/vouchers(.pdf) and POST /api/vouchers/verify after a confirmation.</summary>
public class VoucherEndpointsTests
{
    [Fact]
    public async Task The_owner_sees_the_vouchers_and_prints_them_and_others_cannot()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, _) = await factory.RunToConfirmedAsync();
        var owner = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);
        var other = await factory.CreateClientAsAsync(WorkflowFlow.OtherTourist);

        var vouchers = (await owner.GetFromJsonAsync<List<VoucherDto>>($"/api/trips/{trip.Id}/vouchers", TestJson.Options))!;
        var pdf = await owner.GetAsync($"/api/trips/{trip.Id}/vouchers.pdf");

        vouchers.Should().HaveCount(5);
        vouchers[0].Type.Should().Be("Trip", "the trip voucher is shown first");
        vouchers.Skip(1).Should().BeInAscendingOrder(v => v.Night);
        vouchers.Should().OnlyContain(v => v.QrPayload.StartsWith(VoucherSigner.QrPrefix));
        pdf.StatusCode.Should().Be(HttpStatusCode.OK);
        pdf.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await pdf.Content.ReadAsByteArrayAsync();
        System.Text.Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
        (await other.GetAsync($"/api/trips/{trip.Id}/vouchers")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await other.GetAsync($"/api/trips/{trip.Id}/vouchers.pdf")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task There_are_no_vouchers_before_confirmation()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, _) = await factory.RunToClientAcceptedAsync();
        var owner = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);

        (await owner.GetFromJsonAsync<List<VoucherDto>>($"/api/trips/{trip.Id}/vouchers", TestJson.Options)).Should().BeEmpty();
        (await owner.GetAsync($"/api/trips/{trip.Id}/vouchers.pdf")).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Verify_accepts_an_issued_code_and_rejects_a_forged_one()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, _) = await factory.RunToConfirmedAsync();
        var owner = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");
        var tripVoucher = (await owner.GetFromJsonAsync<List<VoucherDto>>($"/api/trips/{trip.Id}/vouchers", TestJson.Options))!
            .Single(v => v.Type == "Trip");

        var valid = await (await guide.PostAsJsonAsync("/api/vouchers/verify", new VerifyVoucherRequest(tripVoucher.QrPayload)))
            .Content.ReadFromJsonAsync<VoucherVerificationDto>(TestJson.Options);
        var forged = await (await guide.PostAsJsonAsync("/api/vouchers/verify", new VerifyVoucherRequest(Flip(tripVoucher.QrPayload))))
            .Content.ReadFromJsonAsync<VoucherVerificationDto>(TestJson.Options);

        valid!.Valid.Should().BeTrue();
        valid.TripRequestId.Should().Be(trip.Id);
        forged!.Valid.Should().BeFalse();
        (await owner.PostAsJsonAsync("/api/vouchers/verify", new VerifyVoucherRequest(tripVoucher.Code)))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden, "tourists do not scan vouchers");
    }

    /// <summary>Changes the last signature character, so the code is forged.</summary>
    private static string Flip(string code) => code[..^1] + (code[^1] == 'A' ? 'B' : 'A');
}
