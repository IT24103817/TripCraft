using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Infrastructure.Workflows;

// Placeholder until Quotations (Student C) is merged. Each call throws ComponentNotAvailableException (503).
// C replaces the registration in WorkflowsSetup with the real store; nothing else changes.

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
