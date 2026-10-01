import { formatDateTime } from '@/shared/utils/format';
import { DECISION_LABELS } from './reviewRules';
import type { QuotationDto } from './types';

/** The decisions on one quotation version: sent, accepted, the client's decline reason, the revision comment… */
export function DecisionList({ decisions }: { decisions: QuotationDto['decisions'] }) {
  if (decisions.length === 0) return null;
  return (
    <div>
      <h4 className="font-medium text-slate-800">Decisions</h4>
      <ul className="space-y-1 text-slate-700">
        {decisions.map((d, i) => (
          <li key={i}>
            {DECISION_LABELS[d.decision] ?? d.decision} ({formatDateTime(d.decidedAt)})
            {d.comment && <span className="block text-slate-600">“{d.comment}”</span>}
          </li>
        ))}
      </ul>
    </div>
  );
}
