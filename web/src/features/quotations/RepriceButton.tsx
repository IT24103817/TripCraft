import { getErrorMessage } from '@/shared/api/errors';
import { useToast } from '@/shared/components/Toast';
import { formatUsd } from '@/shared/utils/format';
import { useRecalculate } from './quotationsApi';

/**
 * POST /api/quotations/{id}/calculate — Re-price makes a new version with today's rates and exchange rate.
 * A Hard rule in the edited proposal is a 409, shown as a toast.
 */
export function RepriceButton({ quotationId }: { quotationId: string }) {
  const toast = useToast();
  const recalculate = useRecalculate();
  return (
    <button
      type="button"
      className="btn-secondary"
      disabled={recalculate.isPending}
      onClick={() =>
        recalculate.mutate(quotationId, {
          onSuccess: (r) =>
            toast.success(
              `Re-priced as version ${r.version}: ${formatUsd(r.previousTotalUsd)} → ${formatUsd(r.totalUsd)}.` +
                (r.workflowStatus === 'RevisionRequested'
                  ? ' A rule warning is still open (for example over budget).'
                  : ''),
            ),
          onError: (error) => toast.error(getErrorMessage(error)),
        })
      }
    >
      {recalculate.isPending ? 'Re-pricing…' : 'Re-price'}
    </button>
  );
}
