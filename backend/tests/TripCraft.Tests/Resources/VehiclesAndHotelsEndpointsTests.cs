using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Infrastructure.Resources;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Resources;

public class VehiclesAndHotelsEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private const string Manager = "manager1@tripcraft.test";

    [Fact]
    public async Task Vehicle_crud_with_unique_registration()
    {
        var client = await factory.CreateClientAsAsync(Manager);
        var request = new SaveVehicleRequest("wp-kx-1010", "Van", 8, 130, true);

        var created = await client.PostAsJsonAsync("/api/vehicles", request);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var vehicle = (await created.Content.ReadFromJsonAsync<VehicleDto>(TestJson.Options))!;
        vehicle.RegistrationNo.Should().Be("WP-KX-1010");

        (await client.PostAsJsonAsync("/api/vehicles", request)).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await client.PutAsJsonAsync($"/api/vehicles/{vehicle.Id}", request with { Seats = 9 })).StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await client.GetFromJsonAsync<PagedResult<VehicleDto>>("/api/vehicles?minSeats=8&sort=-seats", TestJson.Options);
        page!.Items.Should().Contain(v => v.Id == vehicle.Id).And.OnlyContain(v => v.Seats >= 8);

        (await client.DeleteAsync($"/api/vehicles/{vehicle.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.PostAsJsonAsync("/api/vehicles", request with { Type = "Tuk-tuk" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Hotel_crud_with_room_types()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var created = await client.PostAsJsonAsync("/api/hotels", new SaveHotelRequest("Sigiriya Village", "Sigiriya", 4, 7.95, 80.75, true,
            [new HotelRoomTypeRow(null, "Garden Double", 2, 14000, 4)]));
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var hotel = (await created.Content.ReadFromJsonAsync<HotelDto>(TestJson.Options))!;
        hotel.RoomTypes.Should().ContainSingle(r => r.Name == "Garden Double");

        var room = await client.PostAsJsonAsync($"/api/hotels/{hotel.Id}/room-types", new SaveRoomTypeRequest("Cabana", 2, 16000, 6));
        room.StatusCode.Should().Be(HttpStatusCode.Created);
        var roomType = (await room.Content.ReadFromJsonAsync<RoomTypeDto>(TestJson.Options))!;
        (await client.PostAsJsonAsync($"/api/hotels/{hotel.Id}/room-types", new SaveRoomTypeRequest("cabana", 2, 16000, 6)))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await client.PutAsJsonAsync($"/api/hotels/{hotel.Id}/room-types/{roomType.Id}", new SaveRoomTypeRequest("Cabana", 3, 17000, 6)))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await client.GetFromJsonAsync<HotelDto>($"/api/hotels/{hotel.Id}", TestJson.Options);
        fetched!.RoomTypes.Should().ContainSingle(r => r.Capacity == 3);
        (await client.PostAsJsonAsync("/api/hotels", new SaveHotelRequest("Nowhere Inn", "Paris", 3, 48.85, 2.35, true,
            [new HotelRoomTypeRow(null, "Double", 2, 9000, 2)]))).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await client.DeleteAsync($"/api/hotels/{hotel.Id}/room-types/{roomType.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.DeleteAsync($"/api/hotels/{hotel.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Saving_the_hotel_form_updates_adds_and_removes_room_types_in_one_request()
    {
        var client = await factory.CreateClientAsAsync(Manager);
        var hotel = (await (await client.PostAsJsonAsync("/api/hotels", new SaveHotelRequest("Lake View", "Kandy", 3, 7.29, 80.64, true,
            [new HotelRoomTypeRow(null, "Double", 2, 10000, 4), new HotelRoomTypeRow(null, "Single", 1, 7000, 2)])))
            .Content.ReadFromJsonAsync<HotelDto>(TestJson.Options))!;
        var keep = hotel.RoomTypes.Single(r => r.Name == "Double");

        var saved = await client.PutAsJsonAsync($"/api/hotels/{hotel.Id}", new SaveHotelRequest("Lake View", "Kandy", 3, 7.29, 80.64, true,
            [new HotelRoomTypeRow(keep.Id, "Double", 2, 11000, 5), new HotelRoomTypeRow(null, "Family", 4, 18000, 2)]));

        saved.StatusCode.Should().Be(HttpStatusCode.OK);
        var after = (await saved.Content.ReadFromJsonAsync<HotelDto>(TestJson.Options))!;
        after.RoomTypes.Select(r => r.Name).Should().BeEquivalentTo("Double", "Family");
        after.RoomTypes.Single(r => r.Name == "Double").Should().Match<RoomTypeDto>(r => r.Id == keep.Id && r.RatePerNightLkr == 11000);
        (await client.PutAsJsonAsync($"/api/hotels/{hotel.Id}", new SaveHotelRequest("Lake View", "Kandy", 3, 7.29, 80.64, true, [])))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_room_type_with_booking_history_cannot_be_removed_through_the_form()
    {
        var client = await factory.CreateClientAsAsync(Manager);
        // The seeded Kandy Standard Double is held by the completed sample trip.
        var kandy = await client.GetFromJsonAsync<HotelDto>($"/api/hotels/{ResourcesSeeder.KandyHotel}", TestJson.Options);
        var others = kandy!.RoomTypes.Where(r => r.Id != ResourcesSeeder.KandyStandard)
            .Select(r => new HotelRoomTypeRow(r.Id, r.Name, r.Capacity, r.RatePerNightLkr, r.TotalRooms)).ToList();

        var response = await client.PutAsJsonAsync($"/api/hotels/{kandy.Id}", new SaveHotelRequest(kandy.Name, kandy.City,
            kandy.StarRating, kandy.Latitude, kandy.Longitude, kandy.IsActive, others));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Hotels_filter_by_city_and_a_guide_can_look_one_up()
    {
        var manager = await factory.CreateClientAsAsync(Manager);
        var page = await manager.GetFromJsonAsync<PagedResult<HotelDto>>("/api/hotels?city=Kandy", TestJson.Options);
        page!.Items.Should().ContainSingle(h => h.Id == ResourcesSeeder.KandyHotel);

        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");
        (await guide.GetAsync($"/api/hotels/{ResourcesSeeder.KandyHotel}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await guide.GetAsync("/api/hotels")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Room_types_of_a_hotel_are_listed_and_an_unknown_hotel_is_404()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var rooms = await client.GetFromJsonAsync<List<RoomTypeDto>>(
            $"/api/hotels/{ResourcesSeeder.KandyHotel}/room-types", TestJson.Options);

        rooms!.Select(r => r.Name).Should().BeEquivalentTo("Standard Double", "Family Room");
        rooms.Should().OnlyContain(r => r.HotelId == ResourcesSeeder.KandyHotel);
        (await client.GetAsync($"/api/hotels/{Guid.NewGuid()}/room-types")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("tourist1@tripcraft.test")]
    [InlineData("admin1@tripcraft.test")]
    [InlineData("guide1@tripcraft.test")]
    public async Task Only_the_operations_manager_creates_vehicles_and_hotels(string email)
    {
        var client = await factory.CreateClientAsAsync(email);

        (await client.PostAsJsonAsync("/api/vehicles", new SaveVehicleRequest("WP-0001", "Van", 6, 120, true)))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PostAsJsonAsync("/api/hotels", new SaveHotelRequest("Any Inn", "Kandy", 3, 7.29, 80.63, true,
            [new HotelRoomTypeRow(null, "Double", 2, 9000, 2)])))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
