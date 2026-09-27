import { Link } from 'react-router-dom';
import type { Column } from '@/shared/components/DataTable';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { formatDateTime, shortId } from '@/shared/utils/format';
import type { WorkflowSummaryDto } from './types';

/** Columns with a sortKey are sortable on pages that pass onSortChange (GET /api/workflows?sort=). */
export const workflowColumns: Column<WorkflowSummaryDto>[] = [
  { key: 'id', header: 'Workflow', render: (w) => <span className="font-mono">{shortId(w.id)}</span> },
  {
    key: 'trip',
    header: 'Trip',
    render: (w) => (
      <Link
        to={`/trips/${w.tripRequestId}`}
        className="font-mono text-brand-700 hover:underline"
        onClick={(e) => e.stopPropagation()}
      >
        {shortId(w.tripRequestId)}
      </Link>
    ),
  },
  {
    key: 'objective',
    header: 'Objective',
    // Long objectives are cut with "…"; the full text is in the tooltip.
    render: (w) => <span title={w.objective}>{w.objective}</span>,
    className: 'max-w-xs truncate px-4 py-3 text-slate-900',
  },
  { key: 'status', header: 'Status', sortKey: 'status', render: (w) => <StatusBadge status={w.status} /> },
  { key: 'step', header: 'Current step', render: (w) => w.currentStep ?? '—' },
  { key: 'started', header: 'Started', sortKey: 'startedAt', render: (w) => formatDateTime(w.startedAt) },
  { key: 'finished', header: 'Finished', sortKey: 'finishedAt', render: (w) => formatDateTime(w.finishedAt) },
];
