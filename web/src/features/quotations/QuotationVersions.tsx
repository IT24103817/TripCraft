import { PageState } from '@/shared/components/PageState';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { formatLkr, formatUsd, shortId } from '@/shared/utils/format';
import { DecisionList } from './DecisionList';
import { useQuotation, useQuotationVersions } from './quotationsApi';
import { QUOTATION_STATUS_LABELS } from './reviewRules';
import type { QuotationDto } from './types';

interface Props {
  tripRequestId: string;
  /** id → display name for guides, vehicles and room types (from the workflow). */
  names: Record<string, string>;
}

/**
 * Versions side by side: when a trip has two or more quotation versions, the previous and the newest are shown
 * next to each other (totals, lines, day-by-day stops, guide and vehicle) with the decisions on each — the
 * manager's revision comment and the client's decline reason.
 */
export function QuotationVersions({ tripRequestId, names }: Props) {
  const versions = useQuotationVersions(tripRequestId);
  const list = versions.data ?? [];
  const previous = list.at(-2);
  const latest = list.at(-1);
  // Each version is loaded on its own: only GET /api/quotations/{id} has the proposal snapshot and decisions.
  const previousDetail = useQuotation(previous?.id);
  const latestDetail = useQuotation(latest?.id);

  if (list.length < 2) return null;

  return (
    <section aria-label="Versions side by side" className="card space-y-3 lg:col-span-2">
      <h2 className="font-semibold text-slate-900">
        Version {previous?.version} and version {latest?.version} side by side
      </h2>
      <div className="grid gap-4 md:grid-cols-2">
        {[previousDetail, latestDetail].map((detail, index) => (
          <PageState
            key={index}
            isLoading={detail.isLoading}
            isError={detail.isError}
            error={detail.error}
            onRetry={() => detail.refetch()}
          >
            {detail.data && <VersionColumn quotation={detail.data} names={names} />}
          </PageState>
        ))}
      </div>
    </section>
  );
}

function VersionColumn({ quotation, names }: { quotation: QuotationDto; names: Record<string, string> }) {
  const label = (id: string | null | undefined) => (id ? (names[id] ?? shortId(id)) : 'none');
  const snapshot = quotation.proposalSnapshot;
  return (
    <article
      aria-label={`Version ${quotation.version}`}
      className="space-y-3 rounded border border-slate-200 p-3 text-sm"
    >
      <h3 className="flex flex-wrap items-center gap-2 font-medium text-slate-900">
        Version {quotation.version}
        <StatusBadge status={quotation.status} label={QUOTATION_STATUS_LABELS[quotation.status]} />
      </h3>
      <p className="text-slate-900">
        Total {formatLkr(quotation.totalLkr)} ({formatUsd(quotation.totalUsd)})
      </p>
      <ul className="space-y-1 text-slate-700">
        {quotation.lines.map((line, i) => (
          <li key={i} className="flex justify-between gap-2">
            <span>{line.description}</span>
            <span>{formatLkr(line.amountLkr)}</span>
          </li>
        ))}
      </ul>
      {snapshot && (
        <div className="space-y-1">
          <p className="text-slate-700">
            Guide: {label(snapshot.resources?.guide_id)} · Vehicle: {label(snapshot.resources?.vehicle_id)}
          </p>
          <ol className="space-y-1 text-slate-700">
            {(snapshot.days ?? []).map((day) => (
              <li key={day.day}>
                <span className="font-medium">
                  Day {day.day} — {day.city}:
                </span>{' '}
                {(day.stops ?? []).map((s) => s.name).join(', ') || 'no stops'}
              </li>
            ))}
          </ol>
        </div>
      )}
      <DecisionList decisions={quotation.decisions} />
    </article>
  );
}
