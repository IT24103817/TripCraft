import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/auth/authStore';
import { useWorkflowCount, useWorkflows } from '@/features/quotations/api';
import { useRevenue } from '@/features/quotations/quotationsApi';
import { workflowColumns } from '@/features/quotations/workflowColumns';
import { GuideChangeRequestsPanel } from '@/features/resources/GuideChangeRequestsPanel';
import { useTripCount } from '@/features/trips/api';
import { DataTable } from '@/shared/components/DataTable';
import { KpiCard } from '@/shared/components/KpiCard';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { formatUsd, monthRange } from '@/shared/utils/format';
import { ActionTiles } from './ActionTiles';
import { RevenueChart } from './RevenueChart';
import { UpcomingTrips } from './UpcomingTrips';

/**
 * The manager's day in one page: what needs their action first, the trips running today and tomorrow, then the
 * figures. Admins see the figures they may read and the latest agent runs. The app composes features here;
 * features never import each other.
 */
export default function DashboardPage() {
  const isManager = useAuthStore((s) => s.user?.role === 'OperationsManager');

  return (
    <section className="space-y-6">
      <PageHeader title="Dashboard" />
      {isManager && <ActionTiles />}
      {isManager && <UpcomingTrips />}
      <Kpis isManager={isManager} />
      {isManager && <RevenueChart />}
      {/* Guide change requests are Operations Manager work (the API refuses Admins). */}
      {isManager && (
        <div id="guide-change-requests" tabIndex={-1} className="scroll-mt-4">
          <GuideChangeRequestsPanel />
        </div>
      )}
      {!isManager && <LatestAgentRuns />}
    </section>
  );
}

/** The four figures: proposals waiting, trips and revenue this month (managers only), agents still planning. */
function Kpis({ isManager }: { isManager: boolean }) {
  const month = monthRange();
  // A workflow is PendingApproval while its trip waits for the manager's review.
  const pending = useWorkflowCount('PendingApproval');
  const active = useWorkflowCount('Planning');
  const trips = useTripCount(month.from, month.to);
  // Reports are Operations Manager only; empty dates keep the query disabled for Admins.
  const revenue = useRevenue(isManager ? month.from : '', isManager ? month.to : '');
  const revenueUsd = revenue.data?.reduce((sum, m) => sum + m.totalUsd, 0);

  const show = (value: number | undefined, isError: boolean) => (isError ? 'n/a' : (value ?? '…'));

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <KpiCard
        label="Waiting for review"
        value={show(pending.data, pending.isError)}
        hint="Ready to send to the client"
      />
      {isManager && (
        <KpiCard label="Trips this month" value={show(trips.data, trips.isError)} hint="By start date" />
      )}
      {isManager && (
        <KpiCard
          label="Revenue this month"
          value={revenue.isError ? 'n/a' : revenueUsd === undefined ? '…' : formatUsd(revenueUsd)}
          hint="Quotations sent to clients"
        />
      )}
      <KpiCard
        label="Active workflows"
        value={show(active.data, active.isError)}
        hint="Agents still planning"
      />
    </div>
  );
}

/** Admins have no trips to act on; they see the newest agent runs instead. */
function LatestAgentRuns() {
  const navigate = useNavigate();
  const latest = useWorkflows({ page: 1, pageSize: 5 });
  return (
    <div>
      <h2 className="mb-2 font-semibold text-slate-900">Latest agent runs</h2>
      <PageState
        isLoading={latest.isLoading}
        isError={latest.isError}
        error={latest.error}
        onRetry={() => latest.refetch()}
        isEmpty={latest.data?.total === 0}
        emptyTitle="No agent runs yet"
      >
        {latest.data && (
          <DataTable
            caption="Latest agent runs"
            columns={workflowColumns}
            rows={latest.data.items}
            getRowId={(w) => w.id}
            rowLabel={(w) => `agent run ${w.id.slice(0, 8)}`}
            total={Math.min(latest.data.total, 5)}
            page={1}
            pageSize={5}
            onPageChange={() => navigate('/agent-runs')}
            onRowClick={(w) => navigate(`/agent-runs/${w.id}`)}
          />
        )}
      </PageState>
    </div>
  );
}
