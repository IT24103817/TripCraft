using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips;
using TripCraft.Application.Vouchers;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.Resources;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Resources;

/// <summary>
/// A Confirmed 2-day Ella trip starting today, written straight to the database: two stops on day 1, Nimal (guide1's
/// login) and the van held, and a signed trip voucher plus one hotel-night voucher, as Confirm would have made.
/// </summary>
public record ConfirmedTrip(Guid TripId, Guid[] StopIds, string TripVoucherCode, string HotelVoucherCode)
{
    public static async Task<ConfirmedTrip> CreateAsync(TestWebApplicationFactory factory, int startInDays = 0)
    {
        var signer = new VoucherSigner(TestWebApplicationFactory.VoucherSigningKey);
        return await factory.QueryDbAsync(async db =>
        {
            var tourist = await db.Tourists.FirstAsync(t => db.Users.Any(u => u.Id == t.UserId && u.Email == "tourist1@tripcraft.test"));
            var bridge = await db.Attractions.SingleAsync(a => a.Name == "Nine Arches Bridge");
            var peak = await db.Attractions.SingleAsync(a => a.Name == "Little Adam's Peak");
            var start = TripSettings.Default.Today().AddDays(startInDays); // the operator's date, as the API uses
            var trip = new TripRequest
            {
                TouristId = tourist.Id, Objective = "2 days in Ella", StartDate = start, EndDate = start.AddDays(1), Pax = 2,
                BudgetUsd = 800, Preferences = """{"language":"en"}""", Cities = TripRequest.JoinCities(["Ella"]),
                Status = TripRequestStatus.Confirmed
            };
            var day = new ItineraryDay { DayNumber = 1, City = "Ella", HotelId = ResourcesSeeder.EllaHotel };
            day.Stops.Add(new ItineraryStop { ItineraryDayId = day.Id, AttractionId = bridge.Id, Sequence = 1 });
            day.Stops.Add(new ItineraryStop { ItineraryDayId = day.Id, AttractionId = peak.Id, Sequence = 2 });
            db.AddRange(trip, new Itinerary { TripRequestId = trip.Id, Days = [day] });
            db.ResourceHolds.Add(new ResourceHold { ResourceType = ResourceType.Guide, ResourceId = ResourcesSeeder.NimalGuide,
                TripRequestId = trip.Id, FromDate = trip.StartDate, ToDate = trip.EndDate });
            db.ResourceHolds.Add(new ResourceHold { ResourceType = ResourceType.Vehicle, ResourceId = ResourcesSeeder.VanSixSeats,
                TripRequestId = trip.Id, FromDate = trip.StartDate, ToDate = trip.EndDate });

            var tripVoucher = new Voucher { TripRequestId = trip.Id, Type = VoucherType.Trip };
            tripVoucher.Code = signer.Sign(tripVoucher.Id, trip.Id, VoucherType.Trip, null);
            var hotelVoucher = new Voucher { TripRequestId = trip.Id, Type = VoucherType.HotelNight, HotelId = ResourcesSeeder.EllaHotel,
                Night = start, Rooms = 1 };
            hotelVoucher.Code = signer.Sign(hotelVoucher.Id, trip.Id, VoucherType.HotelNight, start);
            db.Vouchers.AddRange(tripVoucher, hotelVoucher);
            await db.SaveChangesAsync();
            return new ConfirmedTrip(trip.Id, day.Stops.OrderBy(s => s.Sequence).Select(s => s.Id).ToArray(),
                tripVoucher.QrPayload, hotelVoucher.QrPayload);
        });
    }
}
