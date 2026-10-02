using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Identity.Dtos;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Trips;

/// <summary>GET /api/tourists/me and the profile passport photo the package Book sheet asks for (v1.1).</summary>
public class TouristProfileEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 5, 6, 7, 8];

    private static MultipartFormDataContent Photo(byte[] bytes) =>
        new() { { new ByteArrayContent(bytes) { Headers = { ContentType = new MediaTypeHeaderValue("image/jpeg") } }, "file", "p.jpg" } };

    [Fact]
    public async Task A_new_tourist_has_no_photo_until_one_is_uploaded_to_the_profile()
    {
        var anonymous = factory.CreateClient();
        (await anonymous.PostAsJsonAsync("/api/auth/register", new RegisterRequest("newbie@tripcraft.test", "Passw0rd!", "New Bie")))
            .EnsureSuccessStatusCode();
        var tourist = await factory.CreateClientAsAsync("newbie@tripcraft.test");

        var before = await tourist.GetFromJsonAsync<TouristProfileDto>("/api/tourists/me", TestJson.Options);
        var upload = await tourist.PostAsync("/api/tourists/me/passport-photo", Photo(Jpeg));
        var after = await tourist.GetFromJsonAsync<TouristProfileDto>("/api/tourists/me", TestJson.Options);

        before!.HasPassportPhoto.Should().BeFalse();
        upload.StatusCode.Should().Be(HttpStatusCode.OK);
        (await upload.Content.ReadFromJsonAsync<TouristProfileDto>(TestJson.Options))!.HasPassportPhoto.Should().BeTrue();
        after!.HasPassportPhoto.Should().BeTrue();
    }

    [Fact]
    public async Task A_file_that_is_not_a_jpeg_or_png_is_400_and_staff_get_403()
    {
        var tourist = await factory.CreateClientAsAsync("tourist2@tripcraft.test");
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");

        var bad = await tourist.PostAsync("/api/tourists/me/passport-photo", Photo([0x25, 0x50, 0x44, 0x46]));

        bad.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await bad.Content.ReadAsStringAsync()).Should().Contain("JPEG or PNG");
        (await manager.GetAsync("/api/tourists/me")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await manager.PostAsync("/api/tourists/me/passport-photo", Photo(Jpeg))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
