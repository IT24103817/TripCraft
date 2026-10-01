using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Common.Notifications;

namespace TripCraft.Api.Controllers.Notifications;

/// <summary>In-app notifications (v1.1). Every signed-in user reads and marks only their own.</summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(INotificationService notifications) : ControllerBase
{
    /// <summary>The newest 50 and the unread count. Flutter polls this; React shows the count on the bell.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<NotificationListDto>> Mine(CancellationToken ct) =>
        Ok(await notifications.ListMineAsync(User.GetCurrentUser(), ct));

    /// <summary>404 for an unknown id or someone else's notification.</summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        await notifications.MarkReadAsync(User.GetCurrentUser(), id, ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await notifications.MarkAllReadAsync(User.GetCurrentUser(), ct);
        return NoContent();
    }
}
