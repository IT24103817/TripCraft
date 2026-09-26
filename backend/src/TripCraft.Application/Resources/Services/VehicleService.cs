using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Services;

public interface IVehicleService
{
    Task<PagedResult<VehicleDto>> ListAsync(VehicleListQuery query, CancellationToken ct);
    Task<VehicleDto> GetAsync(Guid id, CancellationToken ct);
    Task<VehicleDto> CreateAsync(CurrentUser user, SaveVehicleRequest request, CancellationToken ct);
    Task<VehicleDto> UpdateAsync(CurrentUser user, Guid id, SaveVehicleRequest request, CancellationToken ct);
    Task DeleteAsync(CurrentUser user, Guid id, CancellationToken ct);
}

/// <summary>Vehicle CRUD (Component B). Registration numbers are unique (409), delete is soft.</summary>
public class VehicleService(IResourceRepository resources, IAuditLogger audit, IUnitOfWork unitOfWork) : IVehicleService
{
    public static readonly IReadOnlyList<string> Types = ["Car", "Van", "Coach"];

    public static readonly IReadOnlyDictionary<string, Expression<Func<Vehicle, object>>> SortableFields =
        new Dictionary<string, Expression<Func<Vehicle, object>>>
        {
            ["registrationNo"] = v => v.RegistrationNo,
            ["seats"] = v => v.Seats,
            ["ratePerKmLkr"] = v => v.RatePerKmLkr,
            ["type"] = v => v.Type
        };

    public async Task<PagedResult<VehicleDto>> ListAsync(VehicleListQuery query, CancellationToken ct)
    {
        var q = resources.Vehicles();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(v => v.RegistrationNo.ToLower().Contains(term) || v.Type.ToLower().Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(query.Type))
            q = q.Where(v => v.Type == query.Type);
        if (query.MinSeats.HasValue)
            q = q.Where(v => v.Seats >= query.MinSeats.Value);

        return await q.ApplySort(query.Sort, SortableFields, "registrationNo")
            .ToPagedResultAsync(query.Page, query.PageSize, VehicleDto.FromEntity, ct);
    }

    public async Task<VehicleDto> GetAsync(Guid id, CancellationToken ct) => VehicleDto.FromEntity(await LoadAsync(id, ct));

    public async Task<VehicleDto> CreateAsync(CurrentUser user, SaveVehicleRequest request, CancellationToken ct)
    {
        await EnsureUniqueRegistrationAsync(request.RegistrationNo, null, ct);
        var vehicle = new Vehicle();
        Apply(vehicle, request);
        resources.Add(vehicle);
        audit.Record(user.Id, "VehicleCreated", nameof(Vehicle), vehicle.Id, null, VehicleDto.FromEntity(vehicle));
        await unitOfWork.SaveChangesAsync(ct);
        return VehicleDto.FromEntity(vehicle);
    }

    public async Task<VehicleDto> UpdateAsync(CurrentUser user, Guid id, SaveVehicleRequest request, CancellationToken ct)
    {
        var vehicle = await LoadAsync(id, ct);
        await EnsureUniqueRegistrationAsync(request.RegistrationNo, id, ct);
        var before = VehicleDto.FromEntity(vehicle);
        Apply(vehicle, request);
        audit.Record(user.Id, "VehicleUpdated", nameof(Vehicle), vehicle.Id, before, VehicleDto.FromEntity(vehicle));
        await unitOfWork.SaveChangesAsync(ct);
        return VehicleDto.FromEntity(vehicle);
    }

    public async Task DeleteAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var vehicle = await LoadAsync(id, ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (await resources.Holds().AnyAsync(h => h.ResourceType == ResourceType.Vehicle && h.ResourceId == id
                                                  && h.Status == HoldStatus.Held && h.ToDate >= today, ct))
            throw new ConflictException("This vehicle is held for an upcoming trip; release the hold first.");

        vehicle.IsDeleted = true;
        vehicle.IsActive = false;
        audit.Record(user.Id, "VehicleDeleted", nameof(Vehicle), vehicle.Id, VehicleDto.FromEntity(vehicle), null);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Vehicle> LoadAsync(Guid id, CancellationToken ct) =>
        await resources.FindVehicleAsync(id, ct) ?? throw new NotFoundException("Vehicle not found.");

    private async Task EnsureUniqueRegistrationAsync(string registrationNo, Guid? excludeId, CancellationToken ct)
    {
        var normalised = registrationNo.Trim().ToUpper();
        if (await resources.Vehicles().AnyAsync(v => v.RegistrationNo == normalised && v.Id != excludeId, ct))
            throw new ConflictException($"A vehicle with registration {normalised} already exists.");
    }

    private static void Apply(Vehicle vehicle, SaveVehicleRequest request)
    {
        vehicle.RegistrationNo = request.RegistrationNo.Trim().ToUpper();
        vehicle.Type = request.Type;
        vehicle.Seats = request.Seats;
        vehicle.RatePerKmLkr = request.RatePerKmLkr;
        vehicle.IsActive = request.IsActive;
    }
}
