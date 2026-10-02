import { useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { Button } from '@/shared/components/Button';
import { ReasonDialog } from '@/shared/components/ReasonDialog';
import { useToast } from '@/shared/components/Toast';
import { ProposalEditor } from './ProposalEditor';
import { RepriceButton } from './RepriceButton';
import { sendBlockedReason } from './reviewRules';
import { useSendQuotation } from './tripActionsApi';
import type { QuotationDto, TripSummary, WorkflowDto } from './types';

interface Props {
  trip: TripSummary;
  workflow: WorkflowDto | null;
  /** The newest quotation version (what "Send to client" sends), once loaded. */
  newest: QuotationDto | undefined;
  /** The version Re-price starts from; null prices the trip's proposal (no usable version yet). */
  repriceFrom: string | null;
  /** What the tourist will see, e.g. "The tourist will be asked to accept the updated quote." */
  notice: string;
}

/**
 * "Edit & resend" / "Edit & send manually": edit a day or swap resources, Re-price (a new version, not sent
 * yet), then "Send to client" (POST /api/quotations/{id}/send). Send stays disabled, with the reason, until
 * the newest version is a priced one that was not sent and nothing was edited after that price.
 */
export function EditAndSend({ trip, workflow, newest, repriceFrom, notice }: Props) {
  const toast = useToast();
  const send = useSendQuotation();
  const [sending, setSending] = useState(false);
  const proposal = workflow?.finalOutcome?.proposal;
  const edited = workflow?.finalOutcome?.editedSinceQuotation === true;
  const blocked = sendBlockedReason({ newest, editedSinceQuotation: edited });

  if (!proposal?.days?.length) {
    return (
      <p className="text-sm text-slate-600">
        The agents produced no proposal to edit. Retry planning instead.
      </p>
    );
  }

  return (
    <div className="space-y-3 rounded-md border border-slate-200 bg-surface p-4">
      <p className="rounded-md border border-amber-300 bg-amber-50 p-3 text-sm text-amber-900">{notice}</p>
      <ProposalEditor
        trip={trip}
        days={proposal.days}
        resources={proposal.resources}
        names={workflow?.resourceNames ?? {}}
      />
      <div className="flex flex-wrap gap-2">
        <RepriceButton tripId={trip.id} quotationId={repriceFrom} />
        <Button
          disabled={blocked !== null}
          aria-describedby={blocked ? 'send-blocked' : undefined}
          onClick={() => setSending(true)}
        >
          Send to client
        </Button>
      </div>
      {blocked && (
        <p id="send-blocked" className="text-sm text-slate-600">
          {blocked}
        </p>
      )}

      {sending && newest && (
        <ReasonDialog
          title="Send to client"
          message={`Version ${newest.version} goes to the tourist in the app. ${notice}`}
          fieldLabel="Comment for the tourist (optional)"
          required={false}
          maxLength={1000}
          confirmLabel="Send to client"
          isPending={send.isPending}
          onCancel={() => setSending(false)}
          onConfirm={(comment) =>
            send.mutate(
              { quotationId: newest.id, comment },
              {
                onSuccess: () => {
                  setSending(false);
                  toast.success(
                    `Version ${newest.version} sent to the client. Waiting for them to accept it.`,
                  );
                },
                onError: (error) => {
                  setSending(false);
                  toast.error(getErrorMessage(error));
                },
              },
            )
          }
        />
      )}
    </div>
  );
}
