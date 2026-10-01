import { useCallback, useState } from 'react';
import { useAuthStore } from '@/auth/authStore';
import { PageState } from '@/shared/components/PageState';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { formatDate, formatDateTime, formatUsd } from '@/shared/utils/format';
import { useItinerary } from './api';
import { ItineraryDayEditor } from './ItineraryDayEditor';
import { PdfDownloadButton } from './PdfDownloadButton';
import { StatusTimeline } from './StatusTimeline';
import { TripHistory } from './TripHistory';
import { HAS_VOUCHERS, STATUS_NOTES } from './tripLifecycle';
import type { ItineraryDayDto, TripRequestDto } from './types';

/** The Overview tab of a trip: status, request details, the itinerary (editable once confirmed) and history. */
export function TripOverview({ trip }: { trip: TripRequestDto }) {
  const itinerary = useItinerary(trip.id);
  const isManager = useAuthStore((s) => s.user?.role === 'OperationsManager');
  const [editingDay, setEditingDay] = useState<ItineraryDayDto | null>(null);
  // Stable, so the open dialog does not move focus again when this page re-renders.
  const closeEditor = useCallback(() => setEditingDay(null), []);
  // The API only accepts itinerary edits from an Operations Manager on a Confirmed trip.
  const canEditItinerary = isManager && trip.status === 'Confirmed';

  return (
    <div className="space-y-4">
      <div className="card">
        <h2 className="mb-3 font-semibold text-slate-900">Status</h2>
        <StatusTimeline status={trip.status} />
        {/* e.g. Submitted: only the tourist may start planning (API rule), from the mobile app. */}
        <p className="mt-3 text-sm text-slate-600">{STATUS_NOTES[trip.status]}</p>
        {HAS_VOUCHERS.includes(trip.status) && (
          <div className="mt-3">
            <PdfDownloadButton
              path={`/api/trips/${trip.id}/vouchers.pdf`}
              fileName={`tripcraft-vouchers-${trip.id}.pdf`}
              label="Download vouchers (PDF)"
              errorMessage="Could not download the vouchers."
            />
          </div>
        )}
      </div>

      <div className="card">
        <h2 className="mb-3 font-semibold text-slate-900">Request details</h2>
        <dl className="grid grid-cols-1 gap-3 text-sm sm:grid-cols-2 lg:grid-cols-4">
          <Detail label="Status" value={<StatusBadge status={trip.status} />} />
          <Detail label="Dates" value={`${formatDate(trip.startDate)} – ${formatDate(trip.endDate)}`} />
          <Detail label="Cities" value={trip.cities.join(', ') || '—'} />
          <Detail label="Travellers" value={trip.pax} />
          <Detail label="Budget" value={formatUsd(trip.budgetUsd)} />
          <Detail
            label="Preferences"
            value={
              Object.entries(trip.preferences)
                .map(([k, v]) => `${k}: ${String(v)}`)
                .join(', ') || '—'
            }
          />
          <Detail label="Submitted" value={formatDateTime(trip.createdAt)} />
          <Detail label="Last updated" value={formatDateTime(trip.updatedAt)} />
        </dl>
      </div>

      <div className="card">
        <div className="mb-3 flex flex-wrap items-baseline justify-between gap-2">
          <h2 className="font-semibold text-slate-900">Itinerary</h2>
          {itinerary.data && (
            <p className="text-xs text-slate-500">
              Version {itinerary.data.version} · {itinerary.data.generatedBy}
            </p>
          )}
        </div>
        <PageState
          isLoading={itinerary.isLoading}
          isError={itinerary.isError}
          error={itinerary.error}
          onRetry={() => itinerary.refetch()}
          isEmpty={!itinerary.data}
          emptyTitle="No itinerary yet"
          emptyDescription="The itinerary appears here once planning has produced one."
        >
          <ol className="space-y-3">
            {itinerary.data?.days.map((day) => (
              <li key={day.dayNumber} className="rounded border border-slate-200 p-3">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <h3 className="font-medium text-slate-900">
                    Day {day.dayNumber} — {day.city}
                  </h3>
                  {canEditItinerary && (
                    <button
                      type="button"
                      className="btn-secondary"
                      aria-label={`Edit day ${day.dayNumber}`}
                      onClick={() => setEditingDay(day)}
                    >
                      Edit day
                    </button>
                  )}
                </div>
                {day.notes && <p className="text-sm text-slate-600">{day.notes}</p>}
                <ul className="mt-2 list-inside list-disc text-sm text-slate-700">
                  {day.stops.map((stop) => (
                    <li key={stop.sequence}>
                      {stop.attractionName} ({stop.durationMinutes} min)
                    </li>
                  ))}
                </ul>
              </li>
            ))}
          </ol>
        </PageState>
      </div>

      {editingDay && <ItineraryDayEditor tripId={trip.id} day={editingDay} onClose={closeEditor} />}

      <div className="card">
        <h2 className="mb-3 font-semibold text-slate-900">History</h2>
        <TripHistory tripId={trip.id} />
      </div>
    </div>
  );
}

function Detail({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div>
      <dt className="text-slate-500">{label}</dt>
      <dd className="font-medium text-slate-900">{value}</dd>
    </div>
  );
}
