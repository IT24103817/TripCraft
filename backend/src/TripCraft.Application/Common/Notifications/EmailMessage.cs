using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Common.Notifications;

/// <summary>
/// Transactional outbox for email: the row is written in the same transaction as the business change
/// (e.g. Confirm), then sent after the commit. SentAt stays null and Error is set when sending fails, so a
/// failed email never undoes a confirmed booking and can be retried.
/// </summary>
public class EmailMessage : BaseEntity
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Guid? TripRequestId { get; set; }
    public DateTime? SentAt { get; set; }
    public string? Error { get; set; }
}
