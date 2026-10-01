namespace TripCraft.Application.Quotations.Dashboard;

/// <summary>"Needs your action" counts on the manager dashboard (v1.1).</summary>
public record DashboardActionsDto(int ProposalsToReview, int ClientAcceptedToConfirm, int GuideChangeRequests,
    int RecentCancellations, int DeclinedQuotations);

/// <summary>A trip running today or tomorrow, with its guide and vehicle. Day is "today" or "tomorrow".</summary>
public record UpcomingTripDto(Guid TripRequestId, string Objective, DateOnly StartDate, DateOnly EndDate, int Pax,
    string Status, string TouristName, string? GuideName, string? VehicleRegistrationNo, string? VehicleType, string Day,
    int DayNumber);
