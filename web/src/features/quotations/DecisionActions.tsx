import { useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { ReasonDialog } from '@/shared/components/ReasonDialog';
import { useToast } from '@/shared/components/Toast';
import { statusLabel } from '@/shared/statuses';
import { useQuotationDecision } from './api';
import { RepriceButton } from './RepriceButton';
import { NEXT_STEPS, sendBlockedReason } from './reviewRules';
import type { QuotationDecisionResponse, QuotationDto, WorkflowDto } from './types';

type Action = 'approve' | 'reject' | 'request-revision';

/** The first words of the success toast, per decision returned by the API. */
const DONE: Record<string, string> = {
  Approved: 'Sent to the client.',
  Rejected: 'Rejected.',
  RevisionRequested: 'Revision requested.',
};

interface Props {
  workflow: WorkflowDto;
  /** The newest quotation version (the one the buttons act on), once loaded. */
  quotation: QuotationDto | undefined;
}

/**
 * The review buttons for a trip in PendingReview: Send to client, Request revision (comment required), Reject
 * and Re-price. "Send to client" is disabled, with the reason, whenever the API would answer 409.
 */
export function DecisionActions({ workflow, quotation }: Props) {
  const toast = useToast();
  const decide = useQuotationDecision();
  const [action, setAction] = useState<Action | null>(null);

  const quotationId = workflow.finalOutcome?.proposal.quotationId;
  const blocked = sendBlockedReason({
    workflowStatus: workflow.status,
    editedSinceQuotation: workflow.finalOutcome?.editedSinceQuotation === true,
    quotationId,
    quotationStatus: quotation?.status,
  });

  const run = (decision: Action, comment?: string) => {
    if (!quotationId) return;
    decide.mutate(
      { quotationId, decision, comment },
      {
        onSuccess: (result: QuotationDecisionResponse) => {
          const done = DONE[result.decision] ?? `${statusLabel(result.decision)}.`;
          const tripStatus = statusLabel(result.tripStatus).toLowerCase();
          toast.success(`${done} Trip is now ${tripStatus}.`);
          setAction(null);
        },
        onError: (error) => {
          toast.error(getErrorMessage(error));
          setAction(null);
        },
      },
    );
  };

  if (!quotationId) {
    return <p className="text-sm text-slate-600">The agents have not priced a quotation yet.</p>;
  }

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap gap-2">
        <button
          type="button"
          className="btn-primary"
          disabled={blocked !== null}
          aria-describedby={blocked ? 'send-blocked' : undefined}
          onClick={() => setAction('approve')}
        >
          Send to client
        </button>
        <button type="button" className="btn-secondary" onClick={() => setAction('request-revision')}>
          Request revision
        </button>
        <button type="button" className="btn-danger" onClick={() => setAction('reject')}>
          Reject
        </button>
        <RepriceButton quotationId={quotationId} />
      </div>
      {blocked && (
        <p id="send-blocked" className="rounded-md bg-amber-50 p-2 text-sm text-amber-900">
          {blocked}
        </p>
      )}

      <div className="text-sm text-slate-600">
        <h3 className="font-medium text-slate-800">What happens next</h3>
        <ul className="mt-1 space-y-1">
          {NEXT_STEPS.map((step) => (
            <li key={step.action}>
              <span className="font-medium text-slate-800">{step.action}:</span> {step.text}
            </li>
          ))}
        </ul>
      </div>

      <ConfirmDialog
        open={action === 'approve'}
        title="Send to client"
        message="The tourist gets this quotation in the app and can accept or decline it. Nothing is booked yet."
        confirmLabel="Send to client"
        isPending={decide.isPending}
        onCancel={() => setAction(null)}
        onConfirm={() => run('approve')}
      />
      {action === 'request-revision' && (
        <ReasonDialog
          title="Request a revision"
          message="The Planner agent re-plans the trip using your comment. The new version comes back here for review."
          fieldLabel="Comment"
          required
          requiredMessage="A comment is required for a revision."
          maxLength={1000}
          confirmLabel="Send to the planner"
          isPending={decide.isPending}
          onCancel={() => setAction(null)}
          onConfirm={(comment) => run('request-revision', comment)}
        />
      )}
      {action === 'reject' && (
        <ReasonDialog
          title="Reject trip"
          message="The trip is cancelled and the tourist is told. Nothing is held."
          fieldLabel="Comment for the tourist"
          required={false}
          maxLength={1000}
          confirmLabel="Reject"
          tone="danger"
          isPending={decide.isPending}
          onCancel={() => setAction(null)}
          onConfirm={(comment) => run('reject', comment)}
        />
      )}
    </div>
  );
}
