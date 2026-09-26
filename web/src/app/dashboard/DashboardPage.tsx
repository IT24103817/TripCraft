import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/auth/authStore';
import { useWorkflowCount, useWorkflows } from '@/features/quotations/api';
import { useRevenue } from '@/features/quotations/quotationsApi';
import { workflowColumns } from '@/features/quotations/workflowColumns';
import { useTripCount } from '@/features/trips/api';
import { DataTable } from '@/shared/components/DataTable';
import { KpiCard } from '@/shared/components/KpiCard';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { formatUsd, monthRange } from '@/shared/utils/format';

/** The app composes features here; features never import each other. */
export default function DashboardPage() {
  const navigate = useNavigate();
  const isManager = useAuthStore((s) => s.user?.role === 'OperationsManager');
  const month = monthRange();
  const pending = useWorkflowCount('PendingApproval');
  const active = useWorkflowCount('Planning');
  const trips = useTripCount(month.from, month.to);
  const latest = useWorkflows({ page: 1, pageSize: 5 });
  // Reports are Operations Manager only; empty dates keep the query disabled for Admins.
  const revenue = useRevenue(isManager ? month.from : '', isManager ? month.to : '');
  const revenueUsd = revenue.data?.reduce((sum, m) => sum + m.totalUsd, 0);

  const show = (value: number | undefined, isError: boolean) => (isError ? 'n/a' : (value ?? '…'));

  return (
    <section className="space-y-6">
      <PageHeader title="Dashboard" />
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <KpiCard label="Pending approvals" value={show(pending.data, pending.isError)} />
        {isManager && (
          <KpiCard label="Trips this month" value={show(trips.data, trips.isError)} hint="By start date" />
        )}
        {isManager && (
          <KpiCard
            label="Revenue this month"
            value={revenue.isError ? 'n/a' : revenueUsd === undefined ? '…' : formatUsd(revenueUsd)}
            hint="Approved quotations"
          />
        )}
        <KpiCard
          label="Active workflows"
          value={show(active.data, active.isError)}
          hint="Agents still planning"
        />
      </div>
      <div>
        <h2 className="mb-2 font-semibold text-slate-900">Latest workflows</h2>
        <PageState
          isLoading={latest.isLoading}
          isError={latest.isError}
          error={latest.error}
          onRetry={() => latest.refetch()}
          isEmpty={latest.data?.total === 0}
          emptyTitle="No workflows yet"
        >
          {latest.data && (
            <DataTable
              caption="Latest workflows"
              columns={workflowColumns}
              rows={latest.data.items}
              getRowId={(w) => w.id}
              total={Math.min(latest.data.total, 5)}
              page={1}
              pageSize={5}
              onPageChange={() => navigate('/workflows')}
              onRowClick={(w) => navigate(`/workflows/${w.id}`)}
            />
          )}
        </PageState>
      </div>
    </section>
  );
}
