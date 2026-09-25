using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Trips;

/// <summary>POST /api/trip-requests/{id}/passport-photo (PLAN.md sections 6 and 10).</summary>
public class PassportPhotoEndpointTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 5, 6, 7, 8];

    private async Task<(HttpClient Client, TripRequestDto Trip)> TouristWithTripAsync(string email = "tourist1@tripcraft.test")
    {
        var client = await factory.CreateClientAsAsync(email);
        var created = await client.PostAsJsonAsync("/api/trip-requests", TripRequestsEndpointsTests.NewTrip());
        created.EnsureSuccessStatusCode();
        return (client, (await created.Content.ReadFromJsonAsync<TripRequestDto>(TestJson.Options))!);
    }

    private static MultipartFormDataContent Photo(byte[] bytes, string contentType = "image/jpeg", string name = "passport.jpg")
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "file", name } };
    }

    [Fact]
    public async Task Owner_uploads_a_jpeg_stored_privately_under_a_random_name()
    {
        var (client, trip) = await TouristWithTripAsync();

        var response = await client.PostAsync($"/api/trip-requests/{trip.Id}/passport-photo", Photo(Jpeg));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<PassportPhotoResponse>(TestJson.Options))!;
        body.ContentType.Should().Be("image/jpeg");
        body.SizeBytes.Should().Be(Jpeg.Length);

        var key = await factory.QueryDbAsync(async db =>
            (await db.Tourists.SingleAsync(t => t.Id == trip.TouristId)).PassportPhotoUrl);
        key.Should().MatchRegex(@"^passport-photos/[0-9a-f]{32}\.jpg$").And.NotContain("passport.jpg");
        File.ReadAllBytes(Path.Combine(factory.UploadsDir, key!)).Should().Equal(Jpeg);
        (await factory.QueryDbAsync(db => db.AuditLogs.AnyAsync(a => a.Action == "PassportPhotoUploaded"))).Should().BeTrue();
    }

    [Fact]
    public async Task Png_is_accepted_by_its_content()
    {
        var (client, trip) = await TouristWithTripAsync();

        var response = await client.PostAsync($"/api/trip-requests/{trip.Id}/passport-photo", Photo(Png, "image/png", "p.png"));

        (await response.Content.ReadFromJsonAsync<PassportPhotoResponse>(TestJson.Options))!.ContentType.Should().Be("image/png");
    }

    [Fact]
    public async Task A_non_image_is_rejected_even_when_it_claims_to_be_a_jpeg()
    {
        var (client, trip) = await TouristWithTripAsync();

        var response = await client.PostAsync($"/api/trip-requests/{trip.Id}/passport-photo",
            Photo("%PDF-1.7 not an image"u8.ToArray(), "image/jpeg"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("JPEG or PNG");
    }

    [Fact]
    public async Task A_photo_over_5_mb_is_rejected()
    {
        var (client, trip) = await TouristWithTripAsync();
        var big = new byte[5 * 1024 * 1024 + 1];
        Jpeg.CopyTo(big, 0);

        var response = await client.PostAsync($"/api/trip-requests/{trip.Id}/passport-photo", Photo(big));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Another_tourist_gets_403_and_a_manager_gets_403()
    {
        var (_, trip) = await TouristWithTripAsync();
        var other = await factory.CreateClientAsAsync("tourist2@tripcraft.test");
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");

        (await other.PostAsync($"/api/trip-requests/{trip.Id}/passport-photo", Photo(Jpeg))).StatusCode
            .Should().Be(HttpStatusCode.Forbidden);
        (await manager.PostAsync($"/api/trip-requests/{trip.Id}/passport-photo", Photo(Jpeg))).StatusCode
            .Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Missing_file_returns_400()
    {
        var (client, trip) = await TouristWithTripAsync();

        var response = await client.PostAsync($"/api/trip-requests/{trip.Id}/passport-photo", new MultipartFormDataContent());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
