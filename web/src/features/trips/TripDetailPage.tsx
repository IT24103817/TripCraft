import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { getErrorMessage } from '@/shared/api/errors';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { useToast } from '@/shared/components/Toast';
import { formatDate, formatDateTime, formatUsd } from '@/shared/utils/format';
import { useCancelTrip, useItinerary, useTrip } from './api';
import { StatusTimeline } from './StatusTimeline';
import { TripHistory } from './TripHistory';

export default function TripDetailPage() {
  const { id = '' } = useParams();
  const trip = useTrip(id);
  const itinerary = useItinerary(id);
  const cancel = useCancelTrip(id);
  const toast = useToast();
  const [confirmCancel, setConfirmCancel] = useState(false);

  return (
    <PageState
      isLoading={trip.isLoading}
      isError={trip.isError}
      error={trip.error}
      onRetry={() => trip.refetch()}
    >
      {trip.data && (
        <section className="space-y-4">
          <PageHeader
            title="Trip request"
            description={trip.data.objective}
            actions={
              <>
                <Link to="/trips" className="btn-secondary">
                  Back to trips
                </Link>
                {trip.data.status === 'Submitted' && (
                  <button type="button" className="btn-danger" onClick={() => setConfirmCancel(true)}>
                    Cancel request
                  </button>
                )}
              </>
            }
          />

          <div className="card">
            <h2 className="mb-3 font-semibold text-slate-900">Status</h2>
            <StatusTimeline status={trip.data.status} />
            {trip.data.status === 'Submitted' && (
              // Only the tourist may start planning (API rule), from the mobile app.
              <p className="mt-3 text-sm text-slate-600">
                Waiting for the tourist to start planning in the mobile app.
              </p>
            )}
          </div>

          <div className="card">
            <h2 className="mb-3 font-semibold text-slate-900">Request details</h2>
            <dl className="grid grid-cols-1 gap-3 text-sm sm:grid-cols-2 lg:grid-cols-4">
              <Detail label="Status" value={<StatusBadge status={trip.data.status} />} />
              <Detail
                label="Dates"
                value={`${formatDate(trip.data.startDate)} – ${formatDate(trip.data.endDate)}`}
              />
              <Detail label="Travellers" value={trip.data.pax} />
              <Detail label="Budget" value={formatUsd(trip.data.budgetUsd)} />
              <Detail
                label="Preferences"
                value={
                  Object.entries(trip.data.preferences)
                    .map(([k, v]) => `${k}: ${String(v)}`)
                    .join(', ') || '—'
                }
              />
              <Detail label="Submitted" value={formatDateTime(trip.data.createdAt)} />
              <Detail label="Last updated" value={formatDateTime(trip.data.updatedAt)} />
            </dl>
          </div>

          <div className="card">
            <h2 className="mb-3 font-semibold text-slate-900">Itinerary</h2>
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
                    <h3 className="font-medium text-slate-900">
                      Day {day.dayNumber} — {day.city}
                    </h3>
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

          <div className="card">
            <h2 className="mb-3 font-semibold text-slate-900">History</h2>
            <TripHistory tripId={id} />
          </div>

          <ConfirmDialog
            open={confirmCancel}
            title="Cancel trip request"
            message="Cancel this trip request? It can no longer be planned."
            confirmLabel="Cancel request"
            tone="danger"
            isPending={cancel.isPending}
            onCancel={() => setConfirmCancel(false)}
            onConfirm={() =>
              cancel.mutate(undefined, {
                onSuccess: () => {
                  toast.success('Trip request cancelled.');
                  setConfirmCancel(false);
                },
                onError: (error) => toast.error(getErrorMessage(error)),
              })
            }
          />
        </section>
      )}
    </PageState>
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
