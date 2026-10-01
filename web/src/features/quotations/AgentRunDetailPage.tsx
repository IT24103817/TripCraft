import { Link, useParams } from 'react-router-dom';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { formatDateTime, formatDuration, shortId } from '@/shared/utils/format';
import { useWorkflow, useWorkflowSteps } from './api';
import { StepTimeline } from './StepTimeline';
import { ValidationChecklist } from './ValidationChecklist';

/** One agent run: its status, timings, validation result and every agent step. */
export default function AgentRunDetailPage() {
  const { id = '' } = useParams();
  const workflow = useWorkflow(id);
  const planning = workflow.data?.status === 'Planning';
  const steps = useWorkflowSteps(id, { enabled: workflow.isSuccess, poll: planning });

  return (
    <PageState
      isLoading={workflow.isLoading}
      isError={workflow.isError}
      error={workflow.error}
      onRetry={() => workflow.refetch()}
    >
      {workflow.data && (
        <section className="space-y-4">
          <PageHeader
            title={`Agent run ${shortId(workflow.data.id)}`}
            description={`Trip ${shortId(workflow.data.tripRequestId)}`}
            actions={
              <>
                <Link to="/agent-runs" className="btn-secondary">
                  All agent runs
                </Link>
                {/* Review, send, confirm: possible once the agents produced a proposal. */}
                {(workflow.data.status === 'PendingApproval' ||
                  workflow.data.status === 'RevisionRequested' ||
                  workflow.data.status === 'Approved') && (
                  <Link to={`/approvals/${id}`} className="btn-primary">
                    Review proposal
                  </Link>
                )}
              </>
            }
          />
          <div className="card grid grid-cols-2 gap-3 text-sm md:grid-cols-4">
            <Item label="Status" value={<StatusBadge status={workflow.data.status} />} />
            <Item label="Current step" value={workflow.data.currentStep ?? '—'} />
            <Item label="Started" value={formatDateTime(workflow.data.startedAt)} />
            <Item label="Finished" value={formatDateTime(workflow.data.finishedAt)} />
            <Item label="Elapsed" value={formatDuration(workflow.data.elapsedMs)} />
            <Item label="Agent time" value={formatDuration(workflow.data.totalStepDurationMs)} />
            <Item label="Steps" value={workflow.data.stepCount} />
            {planning && (
              <p role="status" className="col-span-2 text-xs text-blue-700 md:col-span-1">
                Planning — refreshing every 5 s
              </p>
            )}
          </div>
          {workflow.data.errorSummary && (
            <p role="alert" className="card border-red-200 bg-red-50 text-sm text-red-800">
              {workflow.data.errorSummary}
            </p>
          )}
          {workflow.data.validationResult && (
            <div className="card">
              <h2 className="mb-3 font-semibold text-slate-900">Validation result</h2>
              <ValidationChecklist result={workflow.data.validationResult} />
            </div>
          )}
          <div>
            <h2 className="mb-3 font-semibold text-slate-900">Agent steps</h2>
            <PageState
              isLoading={steps.isLoading}
              isError={steps.isError}
              error={steps.error}
              onRetry={() => steps.refetch()}
              isEmpty={steps.data?.length === 0}
              emptyTitle="No steps reported yet"
            >
              <StepTimeline steps={steps.data ?? []} />
            </PageState>
          </div>
        </section>
      )}
    </PageState>
  );
}

function Item({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div>
      <p className="text-slate-500">{label}</p>
      <p className="font-medium text-slate-900">{value}</p>
    </div>
  );
}
