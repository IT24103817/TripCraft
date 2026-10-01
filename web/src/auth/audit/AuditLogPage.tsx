import { DataTable, type Column } from '@/shared/components/DataTable';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { SearchFilterBar } from '@/shared/components/SearchFilterBar';
import { useListParams } from '@/shared/hooks/useListParams';
import { formatDateTime, shortId } from '@/shared/utils/format';
import { ActorFilter } from './ActorFilter';
import { useAuditLogs, type AuditLogDto } from './auditApi';

const ENTITIES = [
  'TripRequest',
  'AgentWorkflow',
  'Quotation',
  'ResourceHold',
  'Attraction',
  'Tourist',
  'User',
];

const columns: Column<AuditLogDto>[] = [
  { key: 'at', header: 'When', sortKey: 'at', render: (a) => formatDateTime(a.at) },
  { key: 'actor', header: 'Who', render: (a) => a.actorEmail ?? 'System' },
  { key: 'action', header: 'Action', sortKey: 'action', render: (a) => a.action },
  { key: 'entity', header: 'Entity', sortKey: 'entity', render: (a) => `${a.entity} ${shortId(a.entityId)}` },
  {
    key: 'change',
    header: 'Change',
    render: (a) => (
      <code
        className="block max-w-md truncate text-xs text-slate-600"
        title={`${a.before ?? '—'} → ${a.after ?? '—'}`}
      >
        {a.before ?? '—'} → {a.after ?? '—'}
      </code>
    ),
  },
];

/** Admin: who changed what and when (audit_logs), with search, actor, entity and date filters, sorting and paging. */
export default function AuditLogPage() {
  const list = useListParams({ sort: '-at' });
  const query = {
    actor: list.get('actor'),
    entity: list.get('entity'),
    from: list.get('from'),
    to: list.get('to'),
    search: list.search,
    sort: list.sort,
    page: list.page,
    pageSize: list.pageSize,
  };
  const logs = useAuditLogs(query);

  return (
    <section className="space-y-4">
      <PageHeader title="Audit log" description="Every business change, with the user who made it." />
      <SearchFilterBar
        search={{
          value: list.search,
          placeholder: 'Search action, entity or user',
          onChange: (search) => list.set({ search }),
        }}
        filters={[
          {
            name: 'entity',
            label: 'Entity',
            value: query.entity,
            options: ENTITIES.map((e) => ({ value: e, label: e })),
            onChange: (entity) => list.set({ entity }),
          },
        ]}
        dateRange={{
          label: 'Date',
          from: query.from,
          to: query.to,
          onChange: (from, to) => list.set({ from, to }),
        }}
      >
        <ActorFilter value={query.actor} onChange={(actor) => list.set({ actor })} />
      </SearchFilterBar>
      <PageState
        isLoading={logs.isLoading}
        isError={logs.isError}
        error={logs.error}
        onRetry={() => logs.refetch()}
        isEmpty={logs.data?.total === 0}
        emptyTitle="No audit entries match these filters"
      >
        {logs.data && (
          <DataTable
            caption="Audit log"
            columns={columns}
            rows={logs.data.items}
            getRowId={(a) => a.id}
            total={logs.data.total}
            page={logs.data.page}
            pageSize={logs.data.pageSize}
            sort={list.sort}
            onSortChange={(sort) => list.set({ sort })}
            onPageChange={(page) => list.set({ page })}
            onPageSizeChange={(pageSize) => list.set({ pageSize })}
          />
        )}
      </PageState>
    </section>
  );
}
