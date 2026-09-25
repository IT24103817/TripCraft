namespace TripCraft.Application.Workflows.External;

/// <summary>USD to LKR rate from open.er-api.com (PLAN.md section 9). Never throws.</summary>
public interface IExchangeRateService
{
    /// <summary>A fresh rate, or the last known rate with Stale = true when the provider fails.</summary>
    Task<ExchangeRate> GetUsdToLkrAsync(CancellationToken ct);
}

/// <summary>Rate = how many LKR one USD buys.</summary>
public record ExchangeRate(string Base, string Quote, decimal Rate, DateTime AsOf, bool Stale);
