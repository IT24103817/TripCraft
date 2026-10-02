import { Link } from 'react-router-dom';
import { Button } from '@/shared/components/Button';
import { DataTable, type Column } from '@/shared/components/DataTable';
import { PageState } from '@/shared/components/PageState';
import { SearchFilterBar } from '@/shared/components/SearchFilterBar';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { useListParams } from '@/shared/hooks/useListParams';
import { formatDate, formatLkr, formatUsd, shortId } from '@/shared/utils/format';
import { useLatestQuotations } from './quotationsApi';
import { QUOTATION_STATUS_LABELS } from './reviewRules';
import type { QuotationDto, QuotationStatus } from './types';

/** The newest version of a trip is never Superseded, so that status is not offered as a filter. */
const STATUSES: QuotationStatus[] = ['Pending', 'Approved', 'RevisionRequested', 'Declined', 'Rejected'];

const tripName = (q: QuotationDto) => q.tripObjective || `Trip ${shortId(q.tripRequestId)}`;

const columns: Column<QuotationDto>[] = [
  {
    key: 'trip',
    header: 'Trip',
    render: (q) => (
      <Link
        to={`/trips/${q.tripRequestId}?tab=quotation`}
        className="font-medium text-brand-700 hover:underline focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500"
      >
        {tripName(q)}
      </Link>
    ),
    className: 'max-w-xs truncate px-4 py-3',
  },
  { key: 'version', header: 'Version', sortKey: 'version', render: (q) => `v${q.version}` },
  {
    key: 'status',
    header: 'Quotation',
    sortKey: 'status',
    render: (q) => <StatusBadge status={q.status} label={QUOTATION_STATUS_LABELS[q.status]} />,
  },
  {
    key: 'tripStatus',
    header: 'Trip status',
    render: (q) => (q.tripStatus ? <StatusBadge status={q.tripStatus} /> : '—'),
  },
  {
    key: 'totalUsd',
    header: 'Total (USD)',
    sortKey: 'totalUsd',
    render: (q) => formatUsd(q.totalUsd),
    className: 'whitespace-nowrap px-4 py-3 font-medium tabular-nums text-slate-900',
  },
  {
    key: 'totalLkr',
    header: 'Total (LKR)',
    sortKey: 'totalLkr',
    render: (q) => formatLkr(q.totalLkr),
    className: 'whitespace-nowrap px-4 py-3 tabular-nums text-slate-700',
  },
  {
    key: 'deposit',
    header: 'Deposit',
    render: (q) => (q.depositPaid ? 'Paid' : 'Unpaid'),
  },
  { key: 'createdAt', header: 'Created', sortKey: 'createdAt', render: (q) => formatDate(q.createdAt) },
];

/**
 * The Quotations tab of the Trips page: every trip that has a quotation, with its newest version. Filters,
 * sorting and paging live in the URL and are done by the API. The trip opens on its Quotation tab.
 */
export function TripQuotationsList() {
  const list = useListParams({ sort: '-createdAt' });
  const query = {
    status: list.get('status'),
    minTotalUsd: list.get('minTotalUsd'),
    search: list.search,
    sort: list.sort,
    page: list.page,
    pageSize: list.pageSize,
  };
  const quotations = useLatestQuotations(query);
  const filtered = Boolean(query.status || query.minTotalUsd || query.search);
  const clearFilters = () => list.set({ status: '', minTotalUsd: '', search: '' });

  return (
    <div className="space-y-4">
      <SearchFilterBar
        search={{
          value: list.search,
          placeholder: 'Search the trip objective',
          onChange: (search) => list.set({ search }),
        }}
        filters={[
          {
            name: 'status',
            label: 'Quotation status',
            value: query.status,
            options: STATUSES.map((s) => ({ value: s, label: QUOTATION_STATUS_LABELS[s] })),
            onChange: (status) => list.set({ status }),
          },
        ]}
      >
        <label className="flex flex-col gap-1 text-sm font-medium text-slate-700">
          Minimum total (USD)
          <input
            type="number"
            className="input font-normal"
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
          filtered ? undefined : 'A trip appears here once the agents have priced its first quotation.'
        }
        emptyAction={
          filtered && (
            <Button variant="secondary" onClick={clearFilters}>
              Clear filters
            </Button>
          )
        }
      >
        {quotations.data && (
          <DataTable
            caption="Quotations (newest version of each trip)"
            columns={columns}
            rows={quotations.data.items}
            getRowId={(q) => q.id}
            total={quotations.data.total}
            page={quotations.data.page}
            pageSize={quotations.data.pageSize}
            sort={list.sort}
            onSortChange={(sort) => list.set({ sort })}
            onPageChange={(page) => list.set({ page })}
            onPageSizeChange={(pageSize) => list.set({ pageSize })}
          />
        )}
      </PageState>
    </div>
  );
}
