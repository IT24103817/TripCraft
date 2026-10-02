import { getErrorMessage } from '@/shared/api/errors';
import { Button } from '@/shared/components/Button';
import { useToast } from '@/shared/components/Toast';
import { formatUsd } from '@/shared/utils/format';
import { useRecalculate } from './quotationsApi';
import { useRepriceTrip } from './tripActionsApi';
import type { RepriceResponse } from './types';

interface Props {
  tripId: string;
  /** The newest version to re-price; null when the trip has no quotation yet (then the trip is priced). */
  quotationId: string | null | undefined;
}

/**
 * Re-price makes a new version with today's rates and exchange rate, not sent yet: POST /api/quotations/{id}/calculate
 * when a version exists, otherwise POST /api/trip-requests/{id}/proposal/reprice. A Hard rule is a 409 (toast).
 */
export function RepriceButton({ tripId, quotationId }: Props) {
  const toast = useToast();
  const recalculate = useRecalculate();
  const repriceTrip = useRepriceTrip(tripId);
  const isPending = recalculate.isPending || repriceTrip.isPending;

  const handlers = {
    onSuccess: (r: RepriceResponse) =>
      toast.success(
        r.previousTotalUsd
          ? `Re-priced as version ${r.version}: ${formatUsd(r.previousTotalUsd)} → ${formatUsd(r.totalUsd)}.`
          : `Priced as version ${r.version}: ${formatUsd(r.totalUsd)}.`,
      ),
    onError: (error: unknown) => toast.error(getErrorMessage(error)),
  };

  return (
    <Button
      variant="secondary"
      isLoading={isPending}
      loadingText="Re-pricing…"
      onClick={() =>
        quotationId ? recalculate.mutate(quotationId, handlers) : repriceTrip.mutate(undefined, handlers)
      }
    >
      Re-price
    </Button>
  );
}
