namespace TripCraft.Application.Quotations.Dashboard;

/// <summary>
/// "Needs your action" counts on the manager dashboard (v1.1): quotations reach the client without the manager, so
/// these are the trips where the operator decides — accepted (confirm), declined, or needs the operator.
/// </summary>
public record DashboardActionsDto(int AcceptedToConfirm, int DeclinedNeedsDecision, int NeedsOperator,
    int GuideChangeRequests, int RecentCancellations);

/// <summary>
/// One trip waiting for the operator (GET /api/dashboard/attention). Detail is the client's decline reason, the
/// error summary (NeedsOperator) or "Version N accepted".
/// </summary>
public record AttentionItemDto(Guid TripRequestId, string Objective, string Status, DateOnly StartDate, DateOnly EndDate,
    int Pax, string TouristName, string Detail, DateTime Since, decimal? TotalUsd);

/// <summary>A trip running today or tomorrow, with its guide and vehicle. Day is "today" or "tomorrow".</summary>
public record UpcomingTripDto(Guid TripRequestId, string Objective, DateOnly StartDate, DateOnly EndDate, int Pax,
    string Status, string TouristName, string? GuideName, string? VehicleRegistrationNo, string? VehicleType, string Day,
    int DayNumber);
