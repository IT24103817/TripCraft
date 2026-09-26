using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Identity.Dtos;
using TripCraft.Application.Identity.Services;

namespace TripCraft.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = Policies.AdminOnly)]
public class AdminUsersController(IUserAdminService userAdminService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> List(CancellationToken ct)
    {
        return Ok(await userAdminService.ListAsync(ct));
    }

    /// <summary>Create a user with any role (Guide, OperationsManager, Admin, Tourist).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken ct)
    {
        var user = await userAdminService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, user);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await userAdminService.DeactivateAsync(id, ct);
        return NoContent();
    }
}
