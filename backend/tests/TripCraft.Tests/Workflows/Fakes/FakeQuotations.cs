using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Tests.Workflows.Fakes;

/// <summary>Stand-in for the Quotations component (Student C) until it is merged.</summary>
public class FakeQuotationsState
{
    public List<FakeQuotation> Quotations { get; } = [];
    public List<(Guid QuotationId, Guid DecidedBy, QuotationDecision Decision, string? Comment)> Decisions { get; } = [];
}

public class FakeQuotation
{
    public Guid Id { get; init; }
    public int Version { get; init; }
    public required QuotationDraft Draft { get; init; }
    public string Status { get; set; } = "Pending";
}

/// <summary>Every change is staged and applied only when the real DbContext saves (same unit of work).</summary>
public class FakeQuotationStore : IQuotationStore
{
    private readonly FakeQuotationsState _state;
    private readonly List<Action> _pending = [];

    public FakeQuotationStore(FakeQuotationsState state, AppDbContext db)
    {
        _state = state;
        db.SavedChanges += (_, _) =>
        {
            foreach (var apply in _pending)
                apply();
            _pending.Clear();
        };
    }

    public Task<Guid> AddVersionAsync(QuotationDraft draft, CancellationToken ct)
    {
        var quotation = new FakeQuotation
        {
            Id = Guid.NewGuid(),
            Version = _state.Quotations.Count(q => q.Draft.TripRequestId == draft.TripRequestId) + 1,
            Draft = draft
        };
        _pending.Add(() => _state.Quotations.Add(quotation));
        return Task.FromResult(quotation.Id);
    }

    public Task<QuotationSummary?> GetAsync(Guid quotationId, CancellationToken ct)
    {
        var q = _state.Quotations.FirstOrDefault(x => x.Id == quotationId);
        return Task.FromResult(q is null
            ? null
            : new QuotationSummary(q.Id, q.Draft.TripRequestId, q.Version, q.Status == "Pending", q.Draft.TotalLkr,
                q.Draft.TotalUsd));
    }

    public Task SetStatusAsync(Guid quotationId, QuotationDecision status, CancellationToken ct)
    {
        _pending.Add(() => _state.Quotations.Single(q => q.Id == quotationId).Status = status.ToString());
        return Task.CompletedTask;
    }

    public void RecordDecision(Guid quotationId, Guid decidedBy, QuotationDecision decision, string? comment) =>
        _pending.Add(() => _state.Decisions.Add((quotationId, decidedBy, decision, comment)));
}
