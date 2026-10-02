import { Link, useParams } from 'react-router-dom';
import { PageHeader } from '@/shared/components/PageHeader';
import { PageState } from '@/shared/components/PageState';
import { StatusBadge } from '@/shared/components/StatusBadge';
import { formatDate, formatUsd } from '@/shared/utils/format';
import { useTripSummary, useWorkflow } from './api';
import { BudgetNote } from './BudgetNote';
import { ProposedItinerary, ProposedResources } from './ProposalDetails';
import { QuotationPanel } from './QuotationPanel';
import { QuotationVersions } from './QuotationVersions';
import { toPanelQuotation, useQuotation } from './quotationsApi';
import { QUOTATION_STATUS_LABELS } from './reviewRules';
import { ReviewStatusBanner } from './ReviewStatusBanner';
import { ValidationChecklist } from './ValidationChecklist';
import { WhyThisPlan } from './WhyThisPlan';

/**
 * The review of one trip's proposal (v1.1 lifecycle). :id is the workflow id. It explains the plan: why this
 * plan, the deterministic checks, the itinerary, the resources, the quotation and the versions side by side.
 * Quotations go to the client automatically, so there is no decision here: the banner links to the trip page,
 * where the manager confirms, edits and resends, replans or cancels.
 */
export default function ApprovalReviewPage() {
  const { id = '' } = useParams();
  const workflow = useWorkflow(id);
  const trip = useTripSummary(workflow.data?.tripRequestId);
  const proposal = workflow.data?.finalOutcome?.proposal;
  // The stored quotation (the newest version) once it exists; the agent's proposal before that.
  const stored = useQuotation(proposal?.quotationId);
  const names = workflow.data?.resourceNames ?? {};

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
                  Back to review queue
                </Link>
                <Link to={`/trips/${workflow.data.tripRequestId}`} className="btn-secondary">
                  Trip details
                </Link>
                <Link to={`/agent-runs/${id}`} className="btn-secondary">
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
                <span>{trip.data.cities.join(', ')}</span>
                <span>{trip.data.pax} travellers</span>
                <span>Budget {formatUsd(trip.data.budgetUsd)}</span>
              </>
            )}
            {proposal && proposal.replans > 0 && <span>Re-planned {proposal.replans}×</span>}
          </div>

          {trip.data && (
            <ReviewStatusBanner tripId={trip.data.id} tripStatus={trip.data.status} quotation={stored.data} />
          )}

          {proposal && <WhyThisPlan tripId={workflow.data.tripRequestId} />}

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
              <div className="card">
                <h2 className="mb-3 font-semibold text-slate-900">Itinerary</h2>
                <ProposedItinerary days={proposal?.days ?? []} />
              </div>
              <div className="card lg:col-span-2">
                <h2 className="mb-3 font-semibold text-slate-900">Proposed guide, vehicle and rooms</h2>
                {proposal?.resources ? (
                  <ProposedResources resources={proposal.resources} names={names} />
                ) : (
                  <p className="text-sm">None.</p>
                )}
              </div>
              <div className="card lg:col-span-2">
                <h2 className="mb-3 font-semibold text-slate-900">
                  Quotation
                  {stored.data
                    ? ` v${stored.data.version} (${QUOTATION_STATUS_LABELS[stored.data.status]})`
                    : ''}
                </h2>
                {stored.data ? (
                  <div className="space-y-3">
                    <BudgetNote quotation={stored.data} />
                    <QuotationPanel quotation={toPanelQuotation(stored.data)} />
                  </div>
                ) : proposal?.quotation ? (
                  <QuotationPanel quotation={proposal.quotation} />
                ) : (
                  <p className="text-sm">None.</p>
                )}
              </div>
              <QuotationVersions tripRequestId={workflow.data.tripRequestId} names={names} />
            </div>
          </PageState>
        </section>
      )}
    </PageState>
  );
}
