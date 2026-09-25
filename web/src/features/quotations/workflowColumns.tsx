import { Link } from 'react-router-dom';
import type { Column } from '@/shared/components/DataTable';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { formatDateTime, shortId } from '@/shared/utils/format';
import type { WorkflowSummaryDto } from './types';

export const workflowColumns: Column<WorkflowSummaryDto>[] = [
  { key: 'id', header: 'Workflow', render: (w) => <span className="font-mono">{shortId(w.id)}</span> },
  {
    key: 'trip',
    header: 'Trip',
    render: (w) => (
      <Link
        to={`/trips/${w.tripRequestId}`}
        className="font-mono text-indigo-700 hover:underline"
        onClick={(e) => e.stopPropagation()}
      >
        {shortId(w.tripRequestId)}
      </Link>
    ),
  },
  { key: 'status', header: 'Status', render: (w) => <StatusBadge status={w.status} /> },
  { key: 'step', header: 'Current step', render: (w) => w.currentStep ?? '—' },
  { key: 'started', header: 'Started', render: (w) => formatDateTime(w.startedAt) },
];
