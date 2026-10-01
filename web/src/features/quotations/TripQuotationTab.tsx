import { useState } from 'react';
import { PageState } from '@/shared/components/PageState';
import { StatusBadge } from '@/shared/components/StatusBadge';
import type { TripRequestStatus } from '@/shared/statuses';
import { formatDate, formatLkr, formatUsd } from '@/shared/utils/format';
import { DecisionList } from './DecisionList';
import { DepositPanel } from './DepositPanel';
import { QuotationPanel } from './QuotationPanel';
import { toPanelQuotation, useQuotation, useQuotationVersions } from './quotationsApi';
import { QUOTATION_STATUS_LABELS } from './reviewRules';

interface Props {
  tripId: string;
  tripStatus: TripRequestStatus;
}

/**
 * The Quotation tab of a trip: every priced version (newest last), and for the chosen one its lines, totals in
 * LKR and USD, the deposit with its paid state, and the decisions (sent, accepted, declined with the reason…).
 */
export function TripQuotationTab({ tripId, tripStatus }: Props) {
  const versions = useQuotationVersions(tripId);
  const list = versions.data ?? [];
  const newest = list.at(-1);
  // null = follow the newest version (also after a re-price adds one).
  const [chosenId, setChosenId] = useState<string | null>(null);
  const selectedId = chosenId ?? newest?.id;
  const detail = useQuotation(selectedId);

  return (
    <PageState
      isLoading={versions.isLoading}
      isError={versions.isError}
      error={versions.error}
      onRetry={() => versions.refetch()}
      isEmpty={list.length === 0}
      emptyTitle="No quotation yet"
      emptyDescription="A quotation appears here when the agents have priced this trip."
    >
      <div className="space-y-4">
        <div className="card overflow-x-auto p-0">
          <table className="min-w-full divide-y divide-slate-200 whitespace-nowrap text-sm tabular-nums">
            <caption className="sr-only">Quotation versions</caption>
            <thead className="bg-slate-50 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
              <tr>
                <th scope="col" className="px-4 py-3">
                  Version
                </th>
                <th scope="col" className="px-4 py-3">
                  Status
                </th>
                <th scope="col" className="px-4 py-3 text-right">
                  Total (LKR)
                </th>
                <th scope="col" className="px-4 py-3 text-right">
                  Total (USD)
                </th>
                <th scope="col" className="px-4 py-3">
                  Deposit
                </th>
                <th scope="col" className="px-4 py-3">
                  Created
                </th>
                <th scope="col" className="px-4 py-3">
                  <span className="sr-only">Show</span>
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {list.map((q) => (
                <tr key={q.id} className={q.id === selectedId ? 'bg-brand-50' : undefined}>
                  <th scope="row" className="px-4 py-3 text-left font-medium text-slate-900">
                    v{q.version}
                  </th>
                  <td className="px-4 py-3">
                    <StatusBadge status={q.status} label={QUOTATION_STATUS_LABELS[q.status]} />
                  </td>
                  <td className="px-4 py-3 text-right">{formatLkr(q.totalLkr)}</td>
                  <td className="px-4 py-3 text-right">{formatUsd(q.totalUsd)}</td>
                  <td className="px-4 py-3">
                    <StatusBadge status={q.depositPaid ? 'Paid' : 'Unpaid'} />
                  </td>
                  <td className="px-4 py-3">{formatDate(q.createdAt)}</td>
                  <td className="px-4 py-3 text-right">
                    <button
                      type="button"
                      className="font-semibold text-brand-700 hover:underline"
                      aria-pressed={q.id === selectedId}
                      onClick={() => setChosenId(q.id)}
                    >
                      Show v{q.version}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <PageState
          isLoading={detail.isLoading}
          isError={detail.isError}
          error={detail.error}
          onRetry={() => detail.refetch()}
        >
          {detail.data && (
            <article aria-label={`Quotation version ${detail.data.version}`} className="card space-y-4">
              <h3 className="flex flex-wrap items-center gap-2 font-semibold text-slate-900">
                Version {detail.data.version}
                <StatusBadge
                  status={detail.data.status}
                  label={QUOTATION_STATUS_LABELS[detail.data.status]}
                />
              </h3>
              <QuotationPanel quotation={toPanelQuotation(detail.data)} />
              <DepositPanel
                quotation={detail.data}
                tripStatus={tripStatus}
                isNewest={detail.data.id === newest?.id}
              />
              <div className="text-sm">
                <DecisionList decisions={detail.data.decisions} />
              </div>
            </article>
          )}
        </PageState>
      </div>
    </PageState>
  );
}
