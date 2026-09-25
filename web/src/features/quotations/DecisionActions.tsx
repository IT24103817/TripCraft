import { useState } from 'react';
import { getErrorMessage } from '@/shared/api/errors';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { useToast } from '@/shared/components/Toast';
import { statusLabel } from '@/shared/statuses';
import { useQuotationDecision } from './api';
import { revisionSchema } from './revisionSchema';
import type { WorkflowDto } from './types';

type Action = 'approve' | 'reject' | 'request-revision';

const COPY: Record<Action, { title: string; confirm: string; message: string; tone: 'primary' | 'danger' }> =
  {
    approve: {
      title: 'Approve quotation',
      confirm: 'Approve',
      message: 'This holds the guide, vehicle and rooms and confirms the trip, in one transaction.',
      tone: 'primary',
    },
    reject: {
      title: 'Reject quotation',
      confirm: 'Reject',
      message: 'The trip request will be rejected. Nothing is held.',
      tone: 'danger',
    },
    'request-revision': {
      title: 'Request a revision',
      confirm: 'Send to the planner',
      message: 'The Planner agent re-plans the trip using your comment.',
      tone: 'primary',
    },
  };

/** Approve / Reject / Request revision, each behind a confirmation. Only an Operations Manager reaches this page. */
export function DecisionActions({ workflow }: { workflow: WorkflowDto }) {
  const toast = useToast();
  const decide = useQuotationDecision();
  const [action, setAction] = useState<Action | null>(null);
  const [comment, setComment] = useState('');
  const [commentError, setCommentError] = useState<string | null>(null);

  const quotationId = workflow.finalOutcome?.proposal.quotationId;
  const decidable = workflow.status === 'PendingApproval' || workflow.status === 'RevisionRequested';
  const canApprove = workflow.status === 'PendingApproval' && Boolean(quotationId);

  const close = () => {
    setAction(null);
    setComment('');
    setCommentError(null);
  };

  const confirm = () => {
    if (!action || !quotationId) return;
    let text: string | undefined = comment.trim() || undefined;
    if (action === 'request-revision') {
      const parsed = revisionSchema.safeParse({ comment });
      if (!parsed.success) {
        setCommentError(parsed.error.issues[0]?.message ?? 'Invalid comment.');
        return;
      }
      text = parsed.data.comment;
    }
    decide.mutate(
      { quotationId, decision: action, comment: text },
      {
        onSuccess: (result) => {
          toast.success(
            `${statusLabel(result.decision)}. Trip is now ${statusLabel(result.tripStatus).toLowerCase()}` +
              (result.holdsCreated > 0 ? `; ${result.holdsCreated} holds created.` : '.'),
          );
          close();
        },
        onError: (error) => {
          toast.error(getErrorMessage(error));
          close();
        },
      },
    );
  };

  if (!decidable || !quotationId) {
    return (
      <p className="text-sm text-slate-600">
        No decision is possible: the workflow is {statusLabel(workflow.status).toLowerCase()}.
      </p>
    );
  }

  return (
    <div className="flex flex-wrap gap-2">
      <button
        type="button"
        className="btn-primary"
        disabled={!canApprove}
        onClick={() => setAction('approve')}
      >
        Approve
      </button>
      <button type="button" className="btn-danger" onClick={() => setAction('reject')}>
        Reject
      </button>
      <button type="button" className="btn-secondary" onClick={() => setAction('request-revision')}>
        Request revision
      </button>
      {!canApprove && (
        <p className="w-full text-xs text-slate-500">Approve is only possible when every check passed.</p>
      )}

      {action && (
        <ConfirmDialog
          open
          title={COPY[action].title}
          message={COPY[action].message}
          confirmLabel={COPY[action].confirm}
          tone={COPY[action].tone}
          isPending={decide.isPending}
          onCancel={close}
          onConfirm={confirm}
        >
          {action !== 'approve' && (
            <div className="flex flex-col gap-1">
              <label htmlFor="decision-comment" className="font-medium">
                Comment{action === 'request-revision' ? ' (required)' : ' (optional)'}
              </label>
              <textarea
                id="decision-comment"
                rows={3}
                className="input"
                value={comment}
                aria-invalid={commentError ? true : undefined}
                aria-describedby={commentError ? 'decision-comment-error' : undefined}
                onChange={(e) => {
                  setComment(e.target.value);
                  setCommentError(null);
                }}
              />
              {commentError && (
                <p id="decision-comment-error" role="alert" className="text-xs text-red-700">
                  {commentError}
                </p>
              )}
            </div>
          )}
        </ConfirmDialog>
      )}
    </div>
  );
}
