using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Services;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.External;

namespace TripCraft.Application.Trips.Templates;

public interface ITripTemplateService
{
    Task<IReadOnlyList<TripTemplateDto>> ListAsync(CancellationToken ct);
    Task<TripTemplateDto> GetAsync(Guid id, CancellationToken ct);
    Task<BookTemplateResponse> BookAsync(CurrentUser user, Guid id, BookTemplateRequest request, CancellationToken ct);
}

public record BookTemplateResponse(TripRequestDto Trip, StartPlanningResponse Planning);

/// <summary>
/// Mood packages on the tourist's home screen (v1.1). List and detail come with a from-price for two people, built
/// from today's rates. "Book as is" creates an ordinary trip request from the package and starts planning, so the
/// agents, the manager's review and the client's acceptance all still happen.
/// </summary>
public class TripTemplateService(
    ITripTemplateRepository templates,
    IAttractionRepository attractions,
    IResourceRepository resources,
    IDistanceService distances,
    IExchangeRateService exchangeRates,
    ITripRequestService tripRequests,
    ITripPlanningService planning) : ITripTemplateService
{
    /// <summary>Package prices on the cards are for two travellers.</summary>
    public const int PricedForPax = 2;

    public async Task<IReadOnlyList<TripTemplateDto>> ListAsync(CancellationToken ct)
    {
        var result = new List<TripTemplateDto>();
        foreach (var template in await templates.ListActiveAsync(ct))
            result.Add(await ToDtoAsync(template, withItinerary: false, ct));
        return result;
    }

    public async Task<TripTemplateDto> GetAsync(Guid id, CancellationToken ct) =>
        await ToDtoAsync(await LoadAsync(id, ct), withItinerary: true, ct);

    public async Task<BookTemplateResponse> BookAsync(CurrentUser user, Guid id, BookTemplateRequest request, CancellationToken ct)
    {
        var template = await LoadAsync(id, ct);
        var trip = await tripRequests.CreateAsync(user, new CreateTripRequestRequest(
            template.Objective, request.StartDate, request.StartDate.AddDays(template.Days - 1), request.Pax,
            request.BudgetUsd, JsonDocument.Parse(template.Preferences).RootElement.Clone(), request.Nationality,
            request.PassportNumber, template.CityList), ct);
        var started = await planning.StartPlanningAsync(user, trip.Id, ct);
        return new BookTemplateResponse(await tripRequests.GetAsync(user, trip.Id, ct), started);
    }

    private async Task<TripTemplate> LoadAsync(Guid id, CancellationToken ct) =>
        await templates.GetActiveAsync(id, ct) ?? throw new NotFoundException("Package not found.");

    private async Task<TripTemplateDto> ToDtoAsync(TripTemplate t, bool withItinerary, CancellationToken ct)
    {
        var days = WorkflowJson.Deserialize<List<TemplateDay>>(t.Plan) ?? [];
        var names = days.SelectMany(d => d.Stops).Distinct().ToList();
        var found = await attractions.QueryActive().Where(a => names.Contains(a.Name)).ToListAsync(ct);

        var price = TemplatePricing.Calculate(days, PricedForPax, await PriceInputsAsync(t, days, found, ct));
        var itinerary = withItinerary
            ? days.OrderBy(d => d.Day).Select(d => new TemplateDayDto(d.Day, d.City, d.Stops.Select(name =>
                found.FirstOrDefault(a => a.Name == name) is { } a
                    ? new TemplateStopDto(a.Id, a.Name, a.EntryFeeLkr, a.Latitude, a.Longitude)
                    : new TemplateStopDto(null, name, 0, null, null)).ToList())).ToList()
            : null;
        return new TripTemplateDto(t.Id, t.Slug, t.Name, t.MoodTag, t.Summary, t.Objective, t.Days, t.CityList,
            JsonDocument.Parse(t.Preferences).RootElement.Clone(), PricedForPax, price.TotalLkr, price.TotalUsd, itinerary);
    }

    /// <summary>Cheapest English guide and vehicle for the party, room rates per city, transfers and the margin.</summary>
    private async Task<TemplatePriceInputs> PriceInputsAsync(TripTemplate t, List<TemplateDay> days,
        List<Attraction> found, CancellationToken ct)
    {
        var guideRate = await resources.Guides()
            .Where(g => g.IsActive && g.MaxPax >= PricedForPax && g.Languages.Any(l => l.LanguageCode == "en"))
            .Select(g => (decimal?)g.DayRateLkr).MinAsync(ct) ?? 0;
        var kmRate = await resources.Vehicles().Where(v => v.IsActive && v.Seats >= PricedForPax)
            .Select(v => (decimal?)v.RatePerKmLkr).MinAsync(ct) ?? 0;
        var cities = t.CityList.ToList();
        var rooms = (await resources.RoomTypes().Where(r => r.Hotel!.IsActive && cities.Contains(r.Hotel.City)).ToListAsync(ct))
            .GroupBy(r => r.Hotel!.City)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<(int, decimal)>)g.Select(r => (r.Capacity, r.RatePerNightLkr)).ToList());

        var transfers = new List<decimal>();
        var ordered = days.OrderBy(d => d.Day).ToList();
        for (var i = 1; i < ordered.Count; i++)
        {
            if (ordered[i].City == ordered[i - 1].City)
                continue;
            var leg = await distances.GetDistanceAsync(ordered[i - 1].City, ordered[i].City, ct);
            transfers.Add(leg?.DistanceKm ?? 0);
        }

        var fx = await exchangeRates.GetUsdToLkrAsync(ct);
        return new TemplatePriceInputs(guideRate, kmRate, rooms,
            found.ToDictionary(a => a.Name, a => a.EntryFeeLkr), transfers,
            await resources.CurrentMarginPctAsync(DateOnly.FromDateTime(DateTime.UtcNow), ct), fx.Rate);
    }
}
