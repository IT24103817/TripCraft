import { useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { ReasonDialog } from '@/shared/components/ReasonDialog';
import { useToast } from '@/shared/components/Toast';
import { useConfirmTrip, useReopenReview } from './reviewApi';

/**
 * The manager's two moves once the client accepted: Confirm (book everything in one transaction) or Reopen
 * review (back to PendingReview with a reason). A 409 (e.g. a room is no longer free) is shown here and in a toast.
 */
export function ClientAcceptedActions({ tripId }: { tripId: string }) {
  const toast = useToast();
  const confirmTrip = useConfirmTrip(tripId);
  const reopen = useReopenReview(tripId);
  const [dialog, setDialog] = useState<'confirm' | 'reopen' | null>(null);
  const [error, setError] = useState<string | null>(null);

  const fail = (failure: unknown) => {
    const message = getErrorMessage(failure);
    setError(message);
    toast.error(message);
    setDialog(null);
  };

  return (
    <div className="space-y-2">
      <div className="flex flex-wrap gap-2">
        <button type="button" className="btn-primary" onClick={() => setDialog('confirm')}>
          Confirm trip
        </button>
        <button type="button" className="btn-secondary" onClick={() => setDialog('reopen')}>
          Reopen review
        </button>
      </div>
      {error && (
        <p role="alert" className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-700">
          {error}
        </p>
      )}

      <ConfirmDialog
        open={dialog === 'confirm'}
        title="Confirm trip"
        message="This books the guide, vehicle and rooms, saves the itinerary, issues the vouchers and emails the tourist — all in one step. If anything is no longer free, nothing is changed."
        confirmLabel="Confirm trip"
        isPending={confirmTrip.isPending}
        onCancel={() => setDialog(null)}
        onConfirm={() =>
          confirmTrip.mutate(undefined, {
            onSuccess: (result) => {
              setError(null);
              setDialog(null);
              toast.success(
                `Trip confirmed: ${result.holdsCreated} holds created, vouchers issued and the tourist emailed.`,
              );
            },
            onError: fail,
          })
        }
      />
      {dialog === 'reopen' && (
        <ReasonDialog
          title="Reopen review"
          message="The trip goes back to Pending review so you can change it and send a new version. The client is told."
          fieldLabel="Reason"
          required
          maxLength={500}
          confirmLabel="Reopen review"
          isPending={reopen.isPending}
          onCancel={() => setDialog(null)}
          onConfirm={(reason) =>
            reopen.mutate(reason ?? '', {
              onSuccess: () => {
                setError(null);
                setDialog(null);
                toast.success('Review reopened. The trip is pending review again.');
              },
              onError: fail,
            })
          }
        />
      )}
    </div>
  );
}
