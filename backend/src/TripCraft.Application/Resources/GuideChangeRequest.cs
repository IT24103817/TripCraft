using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Resources;

/// <summary>
/// A guide asks to be replaced on a confirmed trip (v1.1). The manager resolves it by picking an available guide
/// with the same language; the guide holds are swapped in one transaction.
/// </summary>
public class GuideChangeRequest : BaseEntity
{
    public Guid TripRequestId { get; set; }
    public Guid GuideId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public GuideChangeRequestStatus Status { get; set; } = GuideChangeRequestStatus.Open;
    public Guid? ReplacementGuideId { get; set; }
    public Guid? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public enum GuideChangeRequestStatus
{
    Open,
    Resolved
}
