using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Services;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.External;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Api.Controllers.Internal;

/// <summary>
/// Read-only tool endpoints for the agent service (agents/README.md). X-Internal-Key only, no JWT.
/// Every endpoint reuses an existing service: attractions from Trips, availability and rates from
/// Resource Management, and the third-party wrappers for distance, weather and FX.
/// </summary>
[ApiController]
[Route("api/internal")]
[AllowAnonymous]
[TypeFilter(typeof(InternalKeyAuthFilter))]
public class InternalToolsController(
    IAttractionService attractions,
    IDistanceService distances,
    IWeatherService weather,
    IResourceCatalog resources,
    IExchangeRateService exchangeRates) : ControllerBase
{
    [HttpGet("attractions")]
    public async Task<ActionResult<IReadOnlyList<AttractionDto>>> Attractions([FromQuery] CityQuery query, CancellationToken ct)
    {
        var page = await attractions.ListAsync(
            new AttractionListQuery { City = query.City, PageSize = PagedQuery.MaxPageSize }, ct);
        return Ok(page.Items);
    }

    [HttpGet("distance")]
    public async Task<ActionResult<DistanceResult>> Distance([FromQuery] DistanceQuery query, CancellationToken ct) =>
        Ok(await distances.GetDistanceAsync(query.From, query.To, ct)
           ?? throw new NotFoundException($"No distance known between {query.From} and {query.To}."));

    /// <summary>404 when no forecast is available; the agent treats weather as optional.</summary>
    [HttpGet("weather")]
    public async Task<ActionResult<WeatherForecast>> Weather([FromQuery] WeatherQuery query, CancellationToken ct) =>
        Ok(await weather.GetForecastAsync(query.City, query.Date, ct)
           ?? throw new NotFoundException($"No forecast for {query.City} on {query.Date:yyyy-MM-dd}."));

    [HttpGet("availability/guides")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<GuideOption>>> Guides([FromQuery] GuideAvailabilityQuery q, CancellationToken ct) =>
        Ok(await resources.FindAvailableGuidesAsync(q.From, q.To, q.Language, q.Pax, ct));

    [HttpGet("availability/vehicles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<VehicleOption>>> Vehicles([FromQuery] VehicleAvailabilityQuery q, CancellationToken ct) =>
        Ok(await resources.FindAvailableVehiclesAsync(q.From, q.To, q.Seats, ct));

    [HttpGet("availability/rooms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IReadOnlyList<RoomOption>>> Rooms([FromQuery] RoomAvailabilityQuery q, CancellationToken ct) =>
        Ok(await resources.FindAvailableRoomsAsync(q.City, q.Night, q.Rooms, ct));

    /// <summary>"rate-card" is the path the agent's get_rate_card tool calls; "rates" is the same data.</summary>
    [HttpGet("rates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [HttpGet("rate-card")]
    public async Task<ActionResult<RateCard>> Rates(CancellationToken ct) => Ok(await resources.GetRateCardAsync(ct));

    /// <summary>Used by the agent's get_fx_rate tool. Never fails: a stale rate is flagged instead.</summary>
    [HttpGet("fx-rate")]
    public async Task<ActionResult<ExchangeRate>> FxRate(CancellationToken ct) => Ok(await exchangeRates.GetUsdToLkrAsync(ct));
}
