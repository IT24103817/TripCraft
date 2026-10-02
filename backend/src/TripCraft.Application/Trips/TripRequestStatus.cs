namespace TripCraft.Application.Trips;

/// <summary>
/// Trip lifecycle v1.1 (stored as text). Main path, with no operator step before the client:
/// Submitted → Planning → QuotationSent → ClientAccepted → Confirmed → InProgress → Completed.
/// Side states: ClientDeclined (the tourist said no, with a reason), NeedsOperator (planning failed safely or a
/// Hard rule failed: nothing was sent), Cancelled. The human approval gate is Confirm (Operations Manager): only
/// then are resources held. Allowed moves: <see cref="TripStatusMachine"/>.
/// </summary>
public enum TripRequestStatus
{
    Submitted,
    Planning,
    QuotationSent,
    ClientAccepted,
    ClientDeclined,
    NeedsOperator,
    Confirmed,
    InProgress,
    Completed,
    Cancelled
}
