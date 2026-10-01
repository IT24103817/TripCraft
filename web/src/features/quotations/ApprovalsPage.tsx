import { useNavigate } from 'react-router-dom';
import { DataTable } from '@/shared/components/DataTable';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { SearchFilterBar } from '@/shared/components/SearchFilterBar';
import { useListParams } from '@/shared/hooks/useListParams';
import { cn } from '@/shared/utils/cn';
import { useWorkflows } from './api';
import { workflowColumns } from './workflowColumns';

/**
 * Tabs by agent workflow status: PendingApproval = ready for review, RevisionRequested = a rule warning or the
 * Planner is re-planning, Approved = the quotation was sent (the client may have accepted it: confirm it here).
 */
const TABS = [
  { status: 'PendingApproval', label: 'Pending review' },
  { status: 'RevisionRequested', label: 'Revision requested' },
  { status: 'Approved', label: 'Sent to client' },
] as const;

/** Approval inbox: workflows waiting for the Operations Manager (PLAN.md section 6, step 9; v1.1 lifecycle). */
export default function ApprovalsPage() {
  const navigate = useNavigate();
  const list = useListParams({ sort: '-startedAt' });
  const tab = list.get('tab') || 'PendingApproval';
  const workflows = useWorkflows({
    status: tab,
    search: list.search,
    sort: list.sort,
    page: list.page,
    pageSize: list.pageSize,
  });

  return (
    <section className="space-y-4">
      <PageHeader
        title="Approvals"
        description="AI-drafted trips to review, and quotations sent to clients that may need confirming."
      />
      <SearchFilterBar
        search={{
          value: list.search,
          placeholder: 'Search the trip objective',
          onChange: (search) => list.set({ search }),
        }}
      />
      <div role="tablist" aria-label="Approval status" className="flex gap-2 border-b border-slate-200">
        {TABS.map((t) => (
          <button
            key={t.status}
            type="button"
            role="tab"
            aria-selected={tab === t.status}
            className={cn(
              '-mb-px border-b-2 px-3 py-2 text-sm font-medium',
              tab === t.status
                ? 'border-brand-700 text-brand-700'
                : 'border-transparent text-slate-600 hover:text-slate-900',
            )}
            onClick={() => list.set({ tab: t.status })}
          >
            {t.label}
          </button>
        ))}
      </div>
      <div role="tabpanel" aria-label={TABS.find((t) => t.status === tab)?.label}>
        <PageState
          isLoading={workflows.isLoading}
          isError={workflows.isError}
          error={workflows.error}
          onRetry={() => workflows.refetch()}
          isEmpty={workflows.data?.total === 0}
          emptyTitle={list.search ? 'No proposals match your search' : 'Nothing is waiting here'}
          emptyDescription={
            list.search
              ? 'Try another search, or clear the search box.'
              : 'New proposals appear as soon as the agents finish planning.'
          }
        >
          {workflows.data && (
            <DataTable
              caption="Workflows waiting for a decision"
              columns={workflowColumns}
              rows={workflows.data.items}
              getRowId={(w) => w.id}
              rowLabel={(w) => `proposal ${w.id.slice(0, 8)}`}
              total={workflows.data.total}
              page={workflows.data.page}
              pageSize={workflows.data.pageSize}
              sort={list.sort}
              onSortChange={(sort) => list.set({ sort })}
              onPageChange={(page) => list.set({ page })}
              onPageSizeChange={(pageSize) => list.set({ pageSize })}
              onRowClick={(w) => navigate(`/approvals/${w.id}`)}
            />
          )}
        </PageState>
      </div>
    </section>
  );
}
