import { Link, useNavigate, useParams } from 'react-router-dom';
import { getErrorMessage } from '@/shared/api/errors';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { useToast } from '@/shared/components/Toast';
import { formatDate, formatDateTime, formatUsd } from '@/shared/utils/format';
import { useItinerary, useStartPlanning, useTrip } from './api';
import { StatusTimeline } from './StatusTimeline';

export default function TripDetailPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const toast = useToast();
  const trip = useTrip(id);
  const itinerary = useItinerary(id);
  const startPlanning = useStartPlanning(id);

  const start = () =>
    startPlanning.mutate(undefined, {
      onSuccess: (result) => {
        if (result.workflowStatus === 'FailedSafely') {
          toast.error(`Planning could not start: ${result.errorSummary ?? 'agent service unavailable'}`);
          return;
        }
        toast.success('Planning started. The agents are working on it.');
        navigate(`/workflows/${result.workflowId}`);
      },
      onError: (error) => toast.error(getErrorMessage(error)),
    });

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
                  <button
                    type="button"
                    className="btn-primary"
                    disabled={startPlanning.isPending}
                    onClick={start}
                  >
                    {startPlanning.isPending ? 'Starting…' : 'Start planning'}
                  </button>
                )}
              </>
            }
          />

          <div className="card">
            <h2 className="mb-3 font-semibold text-slate-900">Status</h2>
            <StatusTimeline status={trip.data.status} />
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
