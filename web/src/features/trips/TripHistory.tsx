import { PageState } from '@/shared/components/PageState';
import { statusLabel } from '@/shared/statuses';
import { formatDateTime } from '@/shared/utils/format';
import { useTripHistory } from './api';

/** Readable names for the audit actions a manager sees on a trip. Unknown actions are shown as they are. */
const ACTION_LABELS: Record<string, string> = {
  TripRequestCreated: 'Trip request submitted',
  TripRequestUpdated: 'Details edited',
  TripRequestStatusChanged: 'Status changed',
  AgentWorkflowStarted: 'Agents started planning',
  AgentWorkflowFailedSafely: 'Agents could not start',
  AgentProposalReceived: 'Agents returned a proposal',
  QuotationApproved: 'Quotation sent to the client',
  QuotationSent: 'Quotation sent to the client',
  QuotationRejected: 'Trip rejected by the operator',
  QuotationRevisionRequested: 'Revision requested',
  QuotationRepriced: 'Quotation re-priced (new version)',
  QuotationAccepted: 'Client accepted the quotation',
  QuotationDeclined: 'Client declined the quotation',
  ProposalDayEdited: 'Proposal day edited',
  ProposalResourcesSwapped: 'Guide, vehicle or hotel swapped',
  TripConfirmed: 'Trip confirmed and booked',
  TripRequestCancelled: 'Trip cancelled',
  ItineraryDayEdited: 'Itinerary day edited',
  GuideChangeRequested: 'Guide asked to be replaced',
  GuideChangeResolved: 'Guide replaced',
  StopCheckedIn: 'Stop checked in',
};

/** Spec section 5 "history": every audited change of the trip and its workflows, oldest first. */
export function TripHistory({ tripId }: { tripId: string }) {
  const history = useTripHistory(tripId);
  return (
    <PageState
      isLoading={history.isLoading}
      isError={history.isError}
      error={history.error}
      onRetry={() => history.refetch()}
      isEmpty={history.data?.length === 0}
      emptyTitle="No history yet"
    >
      <ol aria-label="Trip history" className="space-y-2 text-sm">
        {history.data?.map((entry, i) => (
          <li key={i} className="flex flex-wrap gap-x-3 border-l-2 border-slate-200 pl-3">
            <span className="text-slate-500">{formatDateTime(entry.at)}</span>
            <span className="font-medium text-slate-900">{ACTION_LABELS[entry.action] ?? entry.action}</span>
            {entry.toStatus && (
              <span className="text-slate-700">
                {entry.fromStatus ? `${statusLabel(entry.fromStatus)} → ` : ''}
                {statusLabel(entry.toStatus)}
              </span>
            )}
            <span className="text-slate-500">
              by {entry.actor === 'OperationsManager' ? 'Operations Manager' : entry.actor}
            </span>
            {entry.reason && <span className="w-full text-slate-600">Reason: {entry.reason}</span>}
          </li>
        ))}
      </ol>
    </PageState>
  );
}
