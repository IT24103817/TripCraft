import { useNavigate } from 'react-router-dom';
import { DataTable, type Column } from '@/shared/components/DataTable';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { SearchFilterBar } from '@/shared/components/SearchFilterBar';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { useListParams } from '@/shared/hooks/useListParams';
import { formatDate, formatLkr, formatUsd, shortId } from '@/shared/utils/format';
import { useQuotations } from './quotationsApi';
import type { QuotationDto } from './types';

const STATUSES = ['Pending', 'Approved', 'Rejected', 'RevisionRequested'];

const columns: Column<QuotationDto>[] = [
  { key: 'trip', header: 'Trip', render: (q) => shortId(q.tripRequestId) },
  { key: 'version', header: 'Version', sortKey: 'version', render: (q) => `v${q.version}` },
  { key: 'status', header: 'Status', sortKey: 'status', render: (q) => <StatusBadge status={q.status} /> },
  { key: 'totalLkr', header: 'Total (LKR)', sortKey: 'totalLkr', render: (q) => formatLkr(q.totalLkr) },
  { key: 'totalUsd', header: 'Total (USD)', sortKey: 'totalUsd', render: (q) => formatUsd(q.totalUsd) },
  { key: 'fx', header: 'FX', render: (q) => `${q.fxRate}${q.fxStale ? ' (stale)' : ''}` },
  { key: 'accepted', header: 'Tourist', render: (q) => (q.acceptedAt ? 'Accepted' : '—') },
  { key: 'createdAt', header: 'Created', sortKey: 'createdAt', render: (q) => formatDate(q.createdAt) },
];

/**
 * Component C: every quotation version, filterable by status, created date and minimum total (USD),
 * and searchable by the trip's objective.
 */
export default function QuotationsPage() {
  const navigate = useNavigate();
  const list = useListParams({ sort: '-createdAt' });
  const query = {
    status: list.get('status'),
    from: list.get('from'),
    to: list.get('to'),
    minTotalUsd: list.get('minTotalUsd'),
    search: list.search,
    sort: list.sort,
    page: list.page,
    pageSize: list.pageSize,
  };
  const quotations = useQuotations(query);
  const filtered = Boolean(query.status || query.from || query.to || query.minTotalUsd || query.search);
  const clearFilters = () => list.set({ status: '', from: '', to: '', minTotalUsd: '', search: '' });

  return (
    <section className="space-y-4">
      <PageHeader
        title="Quotations"
        description="Every priced version the agents proposed, and its decision."
      />
      <SearchFilterBar
        search={{
          value: list.search,
          placeholder: 'Search the trip objective',
          onChange: (search) => list.set({ search }),
        }}
        filters={[
          {
            name: 'status',
            label: 'Status',
            value: query.status,
            options: STATUSES.map((s) => ({ value: s, label: s })),
            onChange: (status) => list.set({ status }),
          },
        ]}
        dateRange={{
          label: 'Created',
          from: query.from,
          to: query.to,
          onChange: (from, to) => list.set({ from, to }),
        }}
      >
        <label className="flex flex-col gap-1 text-sm text-slate-700">
          Min total (USD)
          <input
            type="number"
            className="input"
            min={0}
            step="0.01"
            inputMode="decimal"
            value={query.minTotalUsd}
            onChange={(e) => {
              // The API rejects a negative minimum, so only empty or 0+ values are sent.
              const value = e.target.value;
              if (value === '' || Number(value) >= 0) list.set({ minTotalUsd: value });
            }}
          />
        </label>
      </SearchFilterBar>
      <PageState
        isLoading={quotations.isLoading}
        isError={quotations.isError}
        error={quotations.error}
        onRetry={() => quotations.refetch()}
        isEmpty={quotations.data?.total === 0}
        emptyTitle={filtered ? 'No quotations match these filters' : 'No quotations yet'}
        emptyDescription={
          filtered ? undefined : 'A quotation appears when the agents finish planning a trip.'
        }
        emptyAction={
          filtered && (
            <button type="button" className="btn-secondary" onClick={clearFilters}>
              Clear filters
            </button>
          )
        }
      >
        {quotations.data && (
          <DataTable
            caption="Quotations"
            columns={columns}
            rows={quotations.data.items}
            getRowId={(q) => q.id}
            rowLabel={(q) => `quotation v${q.version} of trip ${shortId(q.tripRequestId)}`}
            total={quotations.data.total}
            page={quotations.data.page}
            pageSize={quotations.data.pageSize}
            sort={list.sort}
            onSortChange={(sort) => list.set({ sort })}
            onPageChange={(page) => list.set({ page })}
            onPageSizeChange={(pageSize) => list.set({ pageSize })}
            onRowClick={(q) => q.workflowId && navigate(`/approvals/${q.workflowId}`)}
          />
        )}
      </PageState>
    </section>
  );
}
