import { useState, type ReactNode } from 'react';
import { Link, useParams, useSearchParams } from 'react-router-dom';
import { getErrorMessage } from '@/shared/api/errors';
import { Button } from '@/shared/components/Button';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { ReasonDialog } from '@/shared/components/ReasonDialog';
import { TabList } from '@/shared/components/TabList';
import { useToast } from '@/shared/components/Toast';
import { useCancelTrip, useTrip, useTripWorkflowId } from './api';
import { PdfDownloadButton } from './PdfDownloadButton';
import { CANCEL_IN_ACTION_PANEL, CANCELLABLE, HAS_PROPOSAL, QUOTATION_SENT_OR_LATER } from './tripLifecycle';
import { TripOverview } from './TripOverview';
import type { TripRequestDto } from './types';

const TABS = [
  { id: 'overview', label: 'Overview' },
  { id: 'quotation', label: 'Quotation' },
];

interface Props {
  /** The Quotation tab's content; the app passes it in (features never import each other). */
  quotationTab?: (trip: TripRequestDto) => ReactNode;
  /**
   * The manager's next step for the trip's status (Confirm, Edit & resend, Replan…), shown above the tabs.
   * `openCancel` opens this page's "Cancel trip request" dialog.
   */
  statusActions?: (trip: TripRequestDto, openCancel: () => void) => ReactNode;
}

/** One trip request: an Overview tab and a Quotation tab (?tab=quotation), with the trip's actions on top. */
export default function TripDetailPage({ quotationTab, statusActions }: Props) {
  const { id = '' } = useParams();
  const trip = useTrip(id);
  const cancel = useCancelTrip(id);
  const toast = useToast();
  const [params, setParams] = useSearchParams();
  const [confirmCancel, setConfirmCancel] = useState(false);
  const tab = params.get('tab') === 'quotation' && quotationTab ? 'quotation' : 'overview';
  const status = trip.data?.status;
  // The review page is keyed by the workflow id, so it is looked up only for trips that have a proposal.
  const hasProposal = status !== undefined && HAS_PROPOSAL.includes(status);
  const workflowId = useTripWorkflowId(id, hasProposal);
  // When the action panel offers "Cancel with reason", the header does not repeat it.
  const cancelInHeader =
    status !== undefined &&
    CANCELLABLE.includes(status) &&
    !(statusActions && CANCEL_IN_ACTION_PANEL.includes(status));

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
                {hasProposal && workflowId.data && (
                  <Link to={`/approvals/${workflowId.data}`} className="btn-secondary">
                    Open review
                  </Link>
                )}
                {QUOTATION_SENT_OR_LATER.includes(trip.data.status) && (
                  <PdfDownloadButton
                    path={`/api/trips/${id}/itinerary.pdf`}
                    fileName={`tripcraft-itinerary-${id}.pdf`}
                    label="Download itinerary PDF"
                    errorMessage="Could not download the itinerary."
                  />
                )}
                {cancelInHeader && (
                  <Button variant="danger" onClick={() => setConfirmCancel(true)}>
                    Cancel request
                  </Button>
                )}
              </>
            }
          />

          {statusActions?.(trip.data, () => setConfirmCancel(true))}

          {quotationTab && (
            <TabList
              label="Trip sections"
              tabs={TABS}
              selected={tab}
              onSelect={(next) => setParams(next === 'overview' ? {} : { tab: next }, { replace: true })}
            />
          )}
          <div
            role={quotationTab ? 'tabpanel' : undefined}
            id={`panel-${tab}`}
            aria-labelledby={`tab-${tab}`}
          >
            {tab === 'quotation' && quotationTab ? (
              quotationTab(trip.data)
            ) : (
              <TripOverview trip={trip.data} />
            )}
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
