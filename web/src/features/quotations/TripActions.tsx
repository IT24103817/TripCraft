import { PageState } from '@/shared/components/PageState';
import { AcceptedActions } from './AcceptedActions';
import { DeclinedActions } from './DeclinedActions';
import { NeedsOperatorActions } from './NeedsOperatorActions';
import { useQuotationVersions } from './quotationsApi';
import { ACTION_STATUSES, NEXT_STEP_BANNERS } from './reviewRules';
import { useTripWorkflow } from './tripActionsApi';
import type { TripSummary } from './types';

interface Props {
  trip: TripSummary;
  /** Opens the trip page's "Cancel trip request" dialog. */
  onCancel: () => void;
}

/**
 * The manager's next step on the trip page, by status: Accepted → Confirm or Edit & resend; Declined → Replan
 * with note or Cancel; Needs operator → Retry planning, Edit & send manually or Cancel. Other statuses need
 * nothing from the manager, so nothing is drawn.
 */
export function TripActions({ trip, onCancel }: Props) {
  const needsManager = ACTION_STATUSES.includes(trip.status);
  const workflow = useTripWorkflow(trip.id, needsManager);
  const versions = useQuotationVersions(needsManager ? trip.id : undefined);
  // The list has what the rules need (version, status, acceptedAt), so a new version shows without a reload.
  const newest = versions.data?.at(-1);
  const banner = NEXT_STEP_BANNERS[trip.status];
  if (!needsManager || !banner) return null;

  const workflowData = workflow.data ?? null;
  return (
    <section aria-labelledby="next-step" className="card space-y-3">
      <div className="space-y-1 rounded-md border border-brand-200 bg-brand-50 p-3">
        <h2 id="next-step" className="font-semibold text-slate-900">
          Next step: {banner.title}
        </h2>
        <p className="text-sm text-slate-700">{banner.body}</p>
      </div>
      <PageState
        isLoading={workflow.isLoading || versions.isLoading}
        isError={workflow.isError || versions.isError}
        error={workflow.error ?? versions.error}
        onRetry={() => {
          void workflow.refetch();
          void versions.refetch();
        }}
      >
        {trip.status === 'ClientAccepted' && (
          <AcceptedActions trip={trip} workflow={workflowData} newest={newest} />
        )}
        {trip.status === 'ClientDeclined' && (
          <DeclinedActions tripId={trip.id} newest={newest} onCancel={onCancel} />
        )}
        {trip.status === 'NeedsOperator' && (
          <NeedsOperatorActions trip={trip} workflow={workflowData} newest={newest} onCancel={onCancel} />
        )}
      </PageState>
    </section>
  );
}
