import { formatDateTime, formatLkr, formatUsd } from '@/shared/utils/format';
import type { ProposalQuotation } from './types';

/** Quotation lines in LKR with the USD equivalent, the FX rate and when it was fetched. */
export function QuotationPanel({ quotation }: { quotation: ProposalQuotation }) {
  const toUsd = (lkr: number) => lkr / quotation.fx_rate;
  return (
    <div className="space-y-3">
      <div className="overflow-x-auto">
        <table className="min-w-full text-sm">
          <caption className="sr-only">Quotation lines</caption>
          <thead className="bg-slate-50 text-left">
            <tr>
              <th scope="col" className="px-3 py-2">
                Item
              </th>
              <th scope="col" className="px-3 py-2 text-right">
                Qty
              </th>
              <th scope="col" className="px-3 py-2 text-right">
                Unit (LKR)
              </th>
              <th scope="col" className="px-3 py-2 text-right">
                Amount (LKR)
              </th>
              <th scope="col" className="px-3 py-2 text-right">
                Amount (USD)
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {(quotation.lines ?? []).map((line, i) => (
              <tr key={i}>
                <td className="px-3 py-2">
                  <span className="mr-2 rounded bg-slate-100 px-1 text-xs uppercase text-slate-600">
                    {line.line_type}
                  </span>
                  {line.description}
                </td>
                <td className="px-3 py-2 text-right">{line.qty}</td>
                <td className="px-3 py-2 text-right">{formatLkr(line.unit_lkr)}</td>
                <td className="px-3 py-2 text-right">{formatLkr(line.amount_lkr)}</td>
                <td className="px-3 py-2 text-right">{formatUsd(toUsd(line.amount_lkr))}</td>
              </tr>
            ))}
          </tbody>
          <tfoot className="font-medium">
            <tr>
              <th scope="row" colSpan={3} className="px-3 py-1 text-right">
                Subtotal
              </th>
              <td className="px-3 py-1 text-right">{formatLkr(quotation.subtotal_lkr)}</td>
              <td className="px-3 py-1 text-right">{formatUsd(toUsd(quotation.subtotal_lkr))}</td>
            </tr>
            <tr>
              <th scope="row" colSpan={3} className="px-3 py-1 text-right">
                Margin ({quotation.margin_pct}%)
              </th>
              <td className="px-3 py-1 text-right">{formatLkr(quotation.margin_lkr)}</td>
              <td className="px-3 py-1 text-right">{formatUsd(toUsd(quotation.margin_lkr))}</td>
            </tr>
            <tr className="text-slate-900">
              <th scope="row" colSpan={3} className="px-3 py-1 text-right">
                Total
              </th>
              <td className="px-3 py-1 text-right">{formatLkr(quotation.total_lkr)}</td>
              <td className="px-3 py-1 text-right">{formatUsd(quotation.total_usd)}</td>
            </tr>
          </tfoot>
        </table>
      </div>
      <p className="text-sm text-slate-600">
        1 USD = {quotation.fx_rate} LKR, as of {formatDateTime(quotation.fx_as_of)}
        {quotation.fx_stale && (
          <span className="ml-2 rounded bg-amber-100 px-1 text-amber-800">
            stale rate — the FX provider was unavailable
          </span>
        )}
      </p>
    </div>
  );
}
