import { Link, useNavigate } from 'react-router-dom';
import { DataTable, type Column } from '@/shared/components/DataTable';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { SearchFilterBar } from '@/shared/components/SearchFilterBar';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { useListParams } from '@/shared/hooks/useListParams';
import { statusLabel, TRIP_STATUSES } from '@/shared/statuses';
import { formatDate, formatUsd } from '@/shared/utils/format';
import { useTrips } from './api';
import { CityFilter } from './CityFilter';
import type { TripRequestDto } from './types';

const columns: Column<TripRequestDto>[] = [
  {
    key: 'objective',
    header: 'Objective',
    render: (t) => t.objective,
    className: 'max-w-xs truncate px-4 py-2 text-slate-900',
  },
  { key: 'cities', header: 'Cities', render: (t) => t.cities.join(', ') || '—' },
  { key: 'startDate', header: 'Start', sortKey: 'startDate', render: (t) => formatDate(t.startDate) },
  { key: 'endDate', header: 'End', render: (t) => formatDate(t.endDate) },
  { key: 'pax', header: 'Pax', sortKey: 'pax', render: (t) => t.pax },
  { key: 'budgetUsd', header: 'Budget', sortKey: 'budgetUsd', render: (t) => formatUsd(t.budgetUsd) },
  { key: 'status', header: 'Status', sortKey: 'status', render: (t) => <StatusBadge status={t.status} /> },
  { key: 'createdAt', header: 'Submitted', sortKey: 'createdAt', render: (t) => formatDate(t.createdAt) },
];

export default function TripsListPage() {
  const navigate = useNavigate();
  const list = useListParams({ sort: '-createdAt' });
  // The ticked cities live in the URL as "Kandy,Ella", like every other filter.
  const cities = list.get('cities') ? list.get('cities').split(',') : [];
  const query = {
    status: list.get('status'),
    cities,
    from: list.get('from'),
    to: list.get('to'),
    search: list.search,
    sort: list.sort,
    page: list.page,
    pageSize: list.pageSize,
  };
  const trips = useTrips(query);

  return (
    <section className="space-y-4">
      <PageHeader
        title="Trips"
        description="Every trip request submitted from the mobile app, through review, booking and the trip itself."
        actions={
          <Link to="/attractions" className="btn-secondary">
            Attractions
          </Link>
        }
      />
      <SearchFilterBar
        search={{
          value: list.search,
          placeholder: 'Search the objective',
          onChange: (search) => list.set({ search }),
        }}
        filters={[
          {
            name: 'status',
            label: 'Status',
            value: query.status,
            options: TRIP_STATUSES.map((s) => ({ value: s, label: statusLabel(s) })),
            onChange: (status) => list.set({ status }),
          },
        ]}
        dateRange={{
          label: 'Start date',
          from: query.from,
          to: query.to,
          onChange: (from, to) => list.set({ from, to }),
        }}
      >
        <CityFilter value={cities} onChange={(next) => list.set({ cities: next.join(',') })} />
      </SearchFilterBar>
      <PageState
        isLoading={trips.isLoading}
        isError={trips.isError}
        error={trips.error}
        onRetry={() => trips.refetch()}
        isEmpty={trips.data?.total === 0}
        emptyTitle="No trip requests match these filters"
        emptyAction={
          <button
            type="button"
            className="btn-secondary"
            onClick={() => list.set({ status: '', cities: '', from: '', to: '', search: '' })}
          >
            Clear filters
          </button>
        }
      >
        {trips.data && (
          <DataTable
            caption="Trip requests"
            columns={columns}
            rows={trips.data.items}
            getRowId={(t) => t.id}
            rowLabel={(t) => `trip ${t.objective}`}
            total={trips.data.total}
            page={trips.data.page}
            pageSize={trips.data.pageSize}
            sort={list.sort}
            onSortChange={(sort) => list.set({ sort })}
            onPageChange={(page) => list.set({ page })}
            onPageSizeChange={(pageSize) => list.set({ pageSize })}
            onRowClick={(t) => navigate(`/trips/${t.id}`)}
          />
        )}
      </PageState>
    </section>
  );
}
