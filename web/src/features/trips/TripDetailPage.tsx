import { useCallback, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useAuthStore } from '@/auth/authStore';
import { getErrorMessage } from '@/shared/api/errors';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { ReasonDialog } from '@/shared/components/ReasonDialog';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { useToast } from '@/shared/components/Toast';
import { formatDate, formatDateTime, formatUsd } from '@/shared/utils/format';
import { useCancelTrip, useItinerary, useTrip, useTripWorkflowId } from './api';
import { ItineraryDayEditor } from './ItineraryDayEditor';
import { StatusTimeline } from './StatusTimeline';
import { TripHistory } from './TripHistory';
import { CANCELLABLE, HAS_VOUCHERS, IN_REVIEW, STATUS_NOTES } from './tripLifecycle';
import type { ItineraryDayDto } from './types';
import { VouchersButton } from './VouchersButton';

export default function TripDetailPage() {
  const { id = '' } = useParams();
  const trip = useTrip(id);
  const itinerary = useItinerary(id);
  const cancel = useCancelTrip(id);
  const toast = useToast();
  const [confirmCancel, setConfirmCancel] = useState(false);
  const isManager = useAuthStore((s) => s.user?.role === 'OperationsManager');
  const [editingDay, setEditingDay] = useState<ItineraryDayDto | null>(null);
  // Stable, so the open dialog does not move focus again when this page re-renders.
  const closeEditor = useCallback(() => setEditingDay(null), []);
  // The API only accepts itinerary edits from an Operations Manager on a Confirmed trip.
  const canEditItinerary = isManager && trip.data?.status === 'Confirmed';
  const status = trip.data?.status;
  // The review page is keyed by the workflow id, so it is looked up only for trips that are in review.
  const inReview = status !== undefined && IN_REVIEW.includes(status);
  const workflowId = useTripWorkflowId(id, inReview);

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
                {inReview && workflowId.data && (
                  <Link to={`/approvals/${workflowId.data}`} className="btn-primary">
                    Open review
                  </Link>
                )}
                {CANCELLABLE.includes(trip.data.status) && (
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
            {/* e.g. Submitted: only the tourist may start planning (API rule), from the mobile app. */}
            <p className="mt-3 text-sm text-slate-600">{STATUS_NOTES[trip.data.status]}</p>
            {HAS_VOUCHERS.includes(trip.data.status) && (
              <div className="mt-3">
                <VouchersButton tripId={id} />
              </div>
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
              <Detail label="Cities" value={trip.data.cities.join(', ') || '—'} />
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

          {editingDay && <ItineraryDayEditor tripId={id} day={editingDay} onClose={closeEditor} />}

          <div className="card">
            <h2 className="mb-3 font-semibold text-slate-900">History</h2>
            <TripHistory tripId={id} />
          </div>

          {confirmCancel && (
            <ReasonDialog
              title="Cancel trip request"
              message="Any held guide, vehicle and rooms are released, and the tourist is told. This cannot be undone."
              fieldLabel="Reason"
              required
              requiredMessage="Give a reason; the tourist sees it."
              maxLength={500}
              confirmLabel="Cancel request"
              tone="danger"
              isPending={cancel.isPending}
              onCancel={() => setConfirmCancel(false)}
              onConfirm={(reason) =>
                cancel.mutate(
                  { reason: reason ?? '' },
                  {
                    onSuccess: () => {
                      toast.success('Trip request cancelled.');
                      setConfirmCancel(false);
                    },
                    onError: (error) => toast.error(getErrorMessage(error)),
                  },
                )
              }
            />
          )}
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
