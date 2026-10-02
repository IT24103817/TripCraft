import { useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { Button } from '@/shared/components/Button';
import { useToast } from '@/shared/components/Toast';
import { EditAndSend } from './EditAndSend';
import { useRetryPlanning } from './tripActionsApi';
import type { QuotationDto, TripSummary, WorkflowDto } from './types';

interface Props {
  trip: TripSummary;
  workflow: WorkflowDto | null;
  newest: QuotationDto | undefined;
  onCancel: () => void;
}

/**
 * NeedsOperator: what went wrong (the workflow's error summary and failed checks), then Retry planning,
 * Edit & send manually (edit → Re-price → Send) or Cancel with reason.
 */
export function NeedsOperatorActions({ trip, workflow, newest, onCancel }: Props) {
  const toast = useToast();
  const retry = useRetryPlanning(trip.id);
  const [editing, setEditing] = useState(false);
  const violations = workflow?.validationResult?.violations ?? [];

  return (
    <div className="space-y-3">
      <section
        aria-label="What went wrong"
        className="space-y-1 rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800"
      >
        <h3 className="font-semibold">What went wrong</h3>
        <p>{workflow?.errorSummary ?? 'The agents did not record an error summary.'}</p>
        {violations.length > 0 && (
          <ul aria-label="Failed checks" className="list-inside list-disc">
            {violations.map((v) => (
              <li key={v.code}>
                {v.severity}: {v.message}
              </li>
            ))}
          </ul>
        )}
      </section>
      <div className="flex flex-wrap gap-2">
        <Button
          isLoading={retry.isPending}
          loadingText="Starting…"
          onClick={() =>
            retry.mutate(undefined, {
              onSuccess: () =>
                toast.success(
                  'Planning again. A proposal that passes the checks goes to the client automatically.',
                ),
              onError: (error) => toast.error(getErrorMessage(error)),
            })
          }
        >
          Retry planning
        </Button>
        <Button variant="secondary" aria-expanded={editing} onClick={() => setEditing((open) => !open)}>
          {editing ? 'Close editor' : 'Edit & send manually'}
        </Button>
        <Button variant="danger" onClick={onCancel}>
          Cancel with reason
        </Button>
      </div>
      {editing && (
        <EditAndSend
          trip={trip}
          workflow={workflow}
          newest={newest}
          repriceFrom={null}
          notice="When you send it, the tourist gets the quotation in the app and is asked to accept it."
        />
      )}
    </div>
  );
}
