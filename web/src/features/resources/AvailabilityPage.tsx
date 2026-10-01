import { useCallback, useState } from 'react';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { useListParams } from '@/shared/hooks/useListParams';
import { toIsoDate } from '@/shared/utils/format';
import { useAvailabilityGrid } from './api';
import { AvailabilityCellPanel, type CellSelection } from './AvailabilityCellPanel';
import { shiftView, viewLabel, viewRange, viewStart, type GridView } from './availabilityDates';
import { AvailabilityFilters, type GridFilters } from './AvailabilityFilters';
import { AvailabilityGrid } from './AvailabilityGrid';

/**
 * Component B business view: every guide, vehicle and hotel room type by day for a week or a month, with what
 * holds it. Click a cell to block a free day, edit or release a manual block, or open the trip of a hold.
 * The view, the dates and the filters live in the URL.
 */
export default function AvailabilityPage() {
  const list = useListParams();
  const today = toIsoDate(new Date());
  const view: GridView = list.get('view') === 'month' ? 'month' : 'week';
  const start = viewStart(view, list.get('start') || today);
  const { from, to } = viewRange(view, start);
  const filters: GridFilters = {
    type: list.get('type'),
    language: list.get('language'),
    seats: list.get('seats'),
    city: list.get('city'),
  };
  const grid = useAvailabilityGrid({
    from,
    to,
    type: filters.type,
    // Only a full two-letter code is sent, so typing "e" does not empty the list.
    language: filters.language.length === 2 ? filters.language : '',
    seats: filters.seats,
    city: filters.city,
  });
  const [selection, setSelection] = useState<CellSelection | null>(null);
  // Stable, so the open panel does not move focus again when this page re-renders.
  const closePanel = useCallback(() => setSelection(null), []);

  return (
    <section className="space-y-4">
      <PageHeader
        title="Availability"
        description="Who and what is free, held for a trip, confirmed or blocked, day by day."
      />
      <div className="flex flex-wrap items-center gap-2">
        <div role="group" aria-label="View" className="flex gap-1">
          {(['week', 'month'] as const).map((option) => (
            <button
              key={option}
              type="button"
              className={view === option ? 'btn-primary' : 'btn-secondary'}
              aria-pressed={view === option}
              onClick={() => list.set({ view: option, start: viewStart(option, start) })}
            >
              {option === 'week' ? 'Week' : 'Month'}
            </button>
          ))}
        </div>
        <button
          type="button"
          className="btn-secondary"
          aria-label={`Previous ${view}`}
          onClick={() => list.set({ start: shiftView(view, start, -1) })}
        >
          ‹ Prev
        </button>
        <button type="button" className="btn-secondary" onClick={() => list.set({ start: '' })}>
          Today
        </button>
        <button
          type="button"
          className="btn-secondary"
          aria-label={`Next ${view}`}
          onClick={() => list.set({ start: shiftView(view, start, 1) })}
        >
          Next ›
        </button>
        <h2 aria-live="polite" className="ml-2 text-lg font-semibold text-slate-900">
          {viewLabel(view, start)}
        </h2>
      </div>
      <AvailabilityFilters filters={filters} onChange={(patch) => list.set(patch)} />
      <PageState
        isLoading={grid.isLoading}
        isError={grid.isError}
        error={grid.error}
        onRetry={() => grid.refetch()}
        isEmpty={grid.data?.rows.length === 0}
        emptyTitle="No resources match these filters"
        emptyDescription="Clear a filter to see more guides, vehicles or room types."
      >
        {grid.data && (
          <AvailabilityGrid grid={grid.data} onOpen={(row, cell) => setSelection({ row, cell })} />
        )}
      </PageState>
      {selection && <AvailabilityCellPanel selection={selection} onClose={closePanel} />}
    </section>
  );
}
