import { Link, useParams } from 'react-router-dom';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { formatDate, formatUsd } from '@/shared/utils/format';
import { useTripSummary, useWorkflow } from './api';
import { DecisionActions } from './DecisionActions';
import { ProposedItinerary, ProposedResources } from './ProposalDetails';
import { QuotationPanel } from './QuotationPanel';
import { ValidationChecklist } from './ValidationChecklist';

/** Everything the manager needs for the decision (PLAN.md section 6, step 9). :id is the workflow id. */
export default function ApprovalReviewPage() {
  const { id = '' } = useParams();
  const workflow = useWorkflow(id);
  const trip = useTripSummary(workflow.data?.tripRequestId);
  const proposal = workflow.data?.finalOutcome?.proposal;

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
            title="Review proposal"
            description={trip.data?.objective}
            actions={
              <>
                <Link to="/approvals" className="btn-secondary">
                  Back to approvals
                </Link>
                <Link to={`/workflows/${id}`} className="btn-secondary">
                  Agent timeline
                </Link>
              </>
            }
          />

          <div className="card flex flex-wrap items-center gap-x-6 gap-y-2 text-sm">
            <span>
              Workflow <StatusBadge status={workflow.data.status} />
            </span>
            {trip.data && (
              <>
                <span>
                  Trip <StatusBadge status={trip.data.status} />
                </span>
                <span>
                  {formatDate(trip.data.startDate)} – {formatDate(trip.data.endDate)}
                </span>
                <span>{trip.data.pax} travellers</span>
                <span>Budget {formatUsd(trip.data.budgetUsd)}</span>
              </>
            )}
            {proposal && proposal.replans > 0 && <span>Re-planned {proposal.replans}×</span>}
          </div>

          <PageState
            isLoading={false}
            isError={false}
            isEmpty={!proposal}
            emptyTitle="The agents have not sent a proposal yet"
          >
            <div className="grid gap-4 lg:grid-cols-2">
              <div className="card">
                <h2 className="mb-3 font-semibold text-slate-900">Deterministic validation</h2>
                {workflow.data.validationResult ? (
                  <ValidationChecklist result={workflow.data.validationResult} />
                ) : (
                  <p className="text-sm text-slate-600">Not validated yet.</p>
                )}
              </div>
              <div className="card space-y-3">
                <h2 className="font-semibold text-slate-900">Decision</h2>
                <DecisionActions workflow={workflow.data} />
              </div>
              <div className="card">
                <h2 className="mb-3 font-semibold text-slate-900">Itinerary</h2>
                <ProposedItinerary days={proposal?.days ?? []} />
              </div>
              <div className="card">
                <h2 className="mb-3 font-semibold text-slate-900">Proposed guide, vehicle and rooms</h2>
                {proposal?.resources ? (
                  <ProposedResources resources={proposal.resources} />
                ) : (
                  <p className="text-sm">None.</p>
                )}
              </div>
              <div className="card lg:col-span-2">
                <h2 className="mb-3 font-semibold text-slate-900">Quotation</h2>
                {proposal?.quotation ? (
                  <QuotationPanel quotation={proposal.quotation} />
                ) : (
                  <p className="text-sm">None.</p>
                )}
              </div>
            </div>
          </PageState>
        </section>
      )}
    </PageState>
  );
}
