import { useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { Button } from '@/shared/components/Button';
import { ReasonDialog } from '@/shared/components/ReasonDialog';
import { useToast } from '@/shared/components/Toast';
import { useQuotation } from './quotationsApi';
import { declineReason } from './reviewRules';
import { useReplanTrip } from './tripActionsApi';
import type { QuotationDto } from './types';

interface Props {
  tripId: string;
  /** The newest version from the versions list (its decisions are loaded here). */
  newest: QuotationDto | undefined;
  onCancel: () => void;
}

/** ClientDeclined: the client's reason, then "Replan with note" (a note is required) or "Cancel with reason". */
export function DeclinedActions({ tripId, newest, onCancel }: Props) {
  const toast = useToast();
  const replan = useReplanTrip(tripId);
  const [asking, setAsking] = useState(false);
  // Only GET /api/quotations/{id} has the decisions, which hold the client's reason.
  const detail = useQuotation(newest?.id);
  const reason = declineReason(detail.data);

  return (
    <div className="space-y-3">
      {reason && (
        <p className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
          The client declined version {detail.data?.version}: “{reason}”
        </p>
      )}
      <div className="flex flex-wrap gap-2">
        <Button onClick={() => setAsking(true)}>Replan with note</Button>
        <Button variant="danger" onClick={onCancel}>
          Cancel with reason
        </Button>
      </div>

      {asking && (
        <ReasonDialog
          title="Replan with note"
          message="The Planner agent plans the trip again with your note and the client's reason. A new version that passes the checks goes to the client automatically."
          fieldLabel="Note for the Planner agent"
          required
          requiredMessage="Write a note for the Planner agent."
          maxLength={1000}
          confirmLabel="Replan"
          isPending={replan.isPending}
          onCancel={() => setAsking(false)}
          onConfirm={(note) =>
            replan.mutate(note ?? '', {
              onSuccess: () => {
                setAsking(false);
                toast.success('Re-planning started. The new version goes to the client automatically.');
              },
              onError: (error) => {
                setAsking(false);
                toast.error(getErrorMessage(error));
              },
            })
          }
        />
      )}
    </div>
  );
}
