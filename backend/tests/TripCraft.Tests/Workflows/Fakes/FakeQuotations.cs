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
    public DateTime? AcceptedAt { get; set; }
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
            Draft = draft,
            Status = draft.SendNow ? "Approved" : "Pending"
        };
        _pending.Add(() => _state.Quotations.Add(quotation));
        return Task.FromResult(quotation.Id);
    }

    public Task<QuotationSummary?> GetAsync(Guid quotationId, CancellationToken ct) =>
        Task.FromResult(Summary(_state.Quotations.FirstOrDefault(x => x.Id == quotationId)));

    public Task<QuotationSummary?> GetLatestForTripAsync(Guid tripRequestId, CancellationToken ct) =>
        Task.FromResult(Summary(_state.Quotations.Where(q => q.Draft.TripRequestId == tripRequestId)
            .MaxBy(q => q.Version)));

    public Task SetStatusAsync(Guid quotationId, QuotationDecision status, CancellationToken ct)
    {
        _pending.Add(() =>
        {
            var quotation = _state.Quotations.Single(q => q.Id == quotationId);
            if (status == QuotationDecision.Accepted)
                quotation.AcceptedAt = DateTime.UtcNow;
            else if (status != QuotationDecision.Confirmed)
                quotation.Status = status.ToString();
        });
        return Task.CompletedTask;
    }

    private static QuotationSummary? Summary(FakeQuotation? q) => q is null
        ? null
        : new QuotationSummary(q.Id, q.Draft.TripRequestId, q.Version, q.Status, q.Draft.TotalLkr, q.Draft.TotalUsd,
            q.AcceptedAt);

    public Task<string?> GetDeclineReasonAsync(Guid quotationId, CancellationToken ct) =>
        Task.FromResult(_state.Decisions.LastOrDefault(d => d.QuotationId == quotationId && d.Decision == QuotationDecision.Declined).Comment);

    public void RecordDecision(Guid quotationId, Guid decidedBy, QuotationDecision decision, string? comment) =>
        _pending.Add(() => _state.Decisions.Add((quotationId, decidedBy, decision, comment)));
}
