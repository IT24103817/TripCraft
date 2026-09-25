using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Infrastructure.Workflows;

// Placeholders until Resource Management (Student B) and Quotations (Student C) are merged.
// Each call throws ComponentNotAvailableException (503). B and C replace the registrations in
// WorkflowsSetup with their real services; nothing else changes.

public class PendingResourceCatalog : IResourceCatalog
{
    private const string Component = "Resource Management (Student B)";

    public Task<IReadOnlyList<GuideOption>> FindAvailableGuidesAsync(DateOnly from, DateOnly to, string language, int pax,
        CancellationToken ct) => throw new ComponentNotAvailableException(Component);

    public Task<IReadOnlyList<VehicleOption>> FindAvailableVehiclesAsync(DateOnly from, DateOnly to, int seats,
        CancellationToken ct) => throw new ComponentNotAvailableException(Component);

    public Task<IReadOnlyList<RoomOption>> FindAvailableRoomsAsync(string city, DateOnly night, int rooms,
        CancellationToken ct) => throw new ComponentNotAvailableException(Component);

    public Task<RateCard> GetRateCardAsync(CancellationToken ct) => throw new ComponentNotAvailableException(Component);

    public Task<GuideOption?> GetGuideAsync(Guid id, CancellationToken ct) =>
        throw new ComponentNotAvailableException(Component);

    public Task<VehicleOption?> GetVehicleAsync(Guid id, CancellationToken ct) =>
        throw new ComponentNotAvailableException(Component);

    public Task<RoomOption?> GetRoomTypeAsync(Guid roomTypeId, CancellationToken ct) =>
        throw new ComponentNotAvailableException(Component);

    public Task<bool> HasOverlappingHoldAsync(ResourceType type, Guid resourceId, DateOnly from, DateOnly to,
        CancellationToken ct) => throw new ComponentNotAvailableException(Component);
}

public class PendingResourceHoldService : IResourceHoldService
{
    public Task CreateHoldAsync(ResourceHoldRequest hold, CancellationToken ct) =>
        throw new ComponentNotAvailableException("Resource Management (Student B)");
}

public class PendingQuotationStore : IQuotationStore
{
    private const string Component = "Quotations (Student C)";

    public Task<Guid> AddVersionAsync(QuotationDraft draft, CancellationToken ct) =>
        throw new ComponentNotAvailableException(Component);

    public Task<QuotationSummary?> GetAsync(Guid quotationId, CancellationToken ct) =>
        throw new ComponentNotAvailableException(Component);

    public Task SetStatusAsync(Guid quotationId, QuotationDecision status, CancellationToken ct) =>
        throw new ComponentNotAvailableException(Component);

    public void RecordDecision(Guid quotationId, Guid decidedBy, QuotationDecision decision, string? comment) =>
        throw new ComponentNotAvailableException(Component);
}
