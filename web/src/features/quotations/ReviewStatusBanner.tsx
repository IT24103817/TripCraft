import { Link } from 'react-router-dom';
import type { TripRequestStatus } from '@/shared/statuses';
import { ACTION_STATUSES, declineReason, NEXT_STEP_BANNERS } from './reviewRules';
import type { QuotationDto } from './types';

interface Props {
  tripId: string;
  tripStatus: TripRequestStatus;
  /** The newest quotation version, once loaded. */
  quotation: QuotationDto | undefined;
}

/**
 * The status banner at the top of the review page: what the trip's status means and what happens next. The
 * actions themselves (Confirm, Edit & resend, Replan…) are on the trip page, which the banner links to.
 */
export function ReviewStatusBanner({ tripId, tripStatus, quotation }: Props) {
  const banner = NEXT_STEP_BANNERS[tripStatus];
  const declined = tripStatus === 'ClientDeclined' ? declineReason(quotation) : null;
  if (!banner) return null;

  return (
    <section aria-label="Trip status" className="card space-y-2 border-brand-200 bg-brand-50">
      <h2 className="font-semibold text-slate-900">{banner.title}</h2>
      <p className="text-sm text-slate-700">{banner.body}</p>
      {declined && (
        <p className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
          The client declined version {quotation?.version}: {declined}
        </p>
      )}
      {ACTION_STATUSES.includes(tripStatus) && (
        <Link to={`/trips/${tripId}`} className="btn-primary">
          Open the trip to act
        </Link>
      )}
    </section>
  );
}
