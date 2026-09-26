import { getErrorMessage } from '@/shared/api/errors';
import { useToast } from '@/shared/components/Toast';
import { formatUsd } from '@/shared/utils/format';
import { useRecalculate } from './quotationsApi';

/** POST /api/quotations/{id}/calculate — re-price a Pending quotation with today's rates and exchange rate. */
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
              r.changed
                ? `Re-priced: ${formatUsd(r.previousTotalUsd)} → ${formatUsd(r.quotation.totalUsd)}.`
                : 'Re-priced: the total is unchanged.',
            ),
          onError: (error) => toast.error(getErrorMessage(error)),
        })
      }
    >
      {recalculate.isPending ? 'Re-pricing…' : "Re-price with today's rates"}
    </button>
  );
}
