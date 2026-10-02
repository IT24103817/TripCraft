import { useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { Button } from '@/shared/components/Button';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { useToast } from '@/shared/components/Toast';
import { formatUsd } from '@/shared/utils/format';
import { EditAndSend } from './EditAndSend';
import { confirmBlockedReason, isAccepted } from './reviewRules';
import { useConfirmTrip } from './tripActionsApi';
import type { QuotationDto, TripSummary, WorkflowDto } from './types';

interface Props {
  trip: TripSummary;
  workflow: WorkflowDto | null;
  newest: QuotationDto | undefined;
}

/**
 * ClientAccepted: Confirm (the approval gate: book everything in one transaction) or Edit & resend. Confirm is
 * disabled, with the reason, whenever the API would answer 409; a 409 that still happens is shown here.
 */
export function AcceptedActions({ trip, workflow, newest }: Props) {
  const toast = useToast();
  const confirmTrip = useConfirmTrip(trip.id);
  const [editing, setEditing] = useState(false);
  const [asking, setAsking] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const edited = workflow?.finalOutcome?.editedSinceQuotation === true;
  const blocked = confirmBlockedReason({ newest, editedSinceQuotation: edited });

  return (
    <div className="space-y-3">
      {newest && isAccepted(newest) && (
        <p className="text-sm text-slate-700">
          The client accepted version {newest.version} ({formatUsd(newest.totalUsd)}).
        </p>
      )}
      <div className="flex flex-wrap gap-2">
        <Button
          disabled={blocked !== null}
          aria-describedby={blocked ? 'confirm-blocked' : undefined}
          onClick={() => setAsking(true)}
        >
          Confirm
        </Button>
        <Button variant="secondary" aria-expanded={editing} onClick={() => setEditing((open) => !open)}>
          {editing ? 'Close editor' : 'Edit & resend'}
        </Button>
      </div>
      {blocked && (
        <p id="confirm-blocked" className="text-sm text-slate-600">
          {blocked}
        </p>
      )}
      {error && (
        <p role="alert" className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-700">
          {error}
        </p>
      )}
      {editing && (
        <EditAndSend
          trip={trip}
          workflow={workflow}
          newest={newest}
          repriceFrom={newest?.id ?? null}
          notice="The tourist will be asked to accept the updated quote."
        />
      )}

      <ConfirmDialog
        open={asking}
        title="Confirm trip"
        message="This books the guide, vehicle and rooms, saves the itinerary, issues the vouchers and emails the tourist — all in one step. If anything is no longer free, nothing is changed."
        confirmLabel="Confirm trip"
        isPending={confirmTrip.isPending}
        onCancel={() => setAsking(false)}
        onConfirm={() =>
          confirmTrip.mutate(undefined, {
            onSuccess: (result) => {
              setError(null);
              setAsking(false);
              toast.success(
                `Trip confirmed: ${result.holdsCreated} holds created, vouchers issued and the tourist emailed.`,
              );
            },
            onError: (failure) => {
              const message = getErrorMessage(failure);
              setError(message);
              setAsking(false);
              toast.error(message);
            },
          })
        }
      />
    </div>
  );
}
