import { getErrorMessage } from '@/shared/api/errors';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { useToast } from '@/shared/components/Toast';
import type { TripRequestStatus } from '@/shared/statuses';
import { formatDateTime, formatLkr, formatUsd } from '@/shared/utils/format';
import { useDepositPayment } from './quotationsApi';
import { PAYMENT_STATUSES } from './reviewRules';
import type { QuotationDto } from './types';

interface Props {
  quotation: QuotationDto;
  tripStatus: TripRequestStatus;
  /** Only the newest version can be marked paid. */
  isNewest: boolean;
}

/** Deposit of one quotation version (percentage, LKR and USD), its Paid/Unpaid badge and the manager's switch. */
export function DepositPanel({ quotation, tripStatus, isNewest }: Props) {
  const toast = useToast();
  const payment = useDepositPayment();
  const canChange = isNewest && PAYMENT_STATUSES.includes(tripStatus);
  const paid = quotation.depositPaid;

  const change = () =>
    payment.mutate(
      { quotationId: quotation.id, paid: !paid },
      {
        onSuccess: () => toast.success(paid ? 'Deposit marked as unpaid.' : 'Deposit marked as paid.'),
        onError: (error) => toast.error(getErrorMessage(error)),
      },
    );

  return (
    <section aria-label="Deposit" className="space-y-2 rounded-md border border-slate-200 p-3 text-sm">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h4 className="font-medium text-slate-900">Deposit ({quotation.depositPct}%)</h4>
        <StatusBadge status={paid ? 'Paid' : 'Unpaid'} />
      </div>
      <p className="tabular-nums text-slate-700">
        {formatLkr(quotation.depositLkr)} ({formatUsd(quotation.depositUsd)})
      </p>
      {paid && quotation.depositPaidAt && (
        <p className="text-slate-600">Paid on {formatDateTime(quotation.depositPaidAt)}</p>
      )}
      {canChange ? (
        <button type="button" className="btn-secondary" disabled={payment.isPending} onClick={change}>
          {paid ? 'Mark unpaid' : 'Mark deposit paid'}
        </button>
      ) : (
        <p className="text-slate-600">
          {isNewest
            ? 'The deposit can be marked paid once the client has accepted this quotation.'
            : 'Only the newest version can be marked paid.'}
        </p>
      )}
    </section>
  );
}
