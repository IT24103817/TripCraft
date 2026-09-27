import { useNavigate } from 'react-router-dom';
import { DataTable } from '@/shared/components/DataTable';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { SearchFilterBar } from '@/shared/components/SearchFilterBar';
import { useListParams } from '@/shared/hooks/useListParams';
import { statusLabel, WORKFLOW_STATUSES } from '@/shared/statuses';
import { useWorkflows } from './api';
import { workflowColumns } from './workflowColumns';

/** Agent workflow monitor: status filter, search on the trip objective, sortable columns and paging. */
export default function WorkflowsPage() {
  const navigate = useNavigate();
  const list = useListParams({ sort: '-startedAt' });
  const status = list.get('status');
  const workflows = useWorkflows({
    status,
    search: list.search,
    sort: list.sort,
    page: list.page,
    pageSize: list.pageSize,
  });
  const filtered = Boolean(status || list.search);

  return (
    <section className="space-y-4">
      <PageHeader title="Agent workflows" description="Every planning run by the four agents." />
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
            value: status,
            options: WORKFLOW_STATUSES.map((s) => ({ value: s, label: statusLabel(s) })),
            onChange: (value) => list.set({ status: value }),
          },
        ]}
      />
      <PageState
        isLoading={workflows.isLoading}
        isError={workflows.isError}
        error={workflows.error}
        onRetry={() => workflows.refetch()}
        isEmpty={workflows.data?.total === 0}
        emptyTitle={filtered ? 'No workflows match these filters' : 'No workflows yet'}
        emptyDescription={
          filtered
            ? 'Try another search or status.'
            : 'A workflow starts when a trip request is sent for planning.'
        }
      >
        {workflows.data && (
          <DataTable
            caption="Agent workflows"
            columns={workflowColumns}
            rows={workflows.data.items}
            getRowId={(w) => w.id}
            rowLabel={(w) => `workflow ${w.id.slice(0, 8)}`}
            total={workflows.data.total}
            page={workflows.data.page}
            pageSize={workflows.data.pageSize}
            sort={list.sort}
            onSortChange={(sort) => list.set({ sort })}
            onPageChange={(page) => list.set({ page })}
            onPageSizeChange={(pageSize) => list.set({ pageSize })}
            onRowClick={(w) => navigate(`/workflows/${w.id}`)}
          />
        )}
      </PageState>
    </section>
  );
}
