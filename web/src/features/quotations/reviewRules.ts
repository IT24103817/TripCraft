import type { TripRequestStatus, WorkflowStatus } from '@/shared/statuses';
import type { QuotationDto, QuotationStatus } from './types';

/** What a quotation status means to the manager (an "Approved" quotation was sent to the client). */
export const QUOTATION_STATUS_LABELS: Record<QuotationStatus, string> = {
  Pending: 'Waiting for review',
  Approved: 'Sent to client',
  Declined: 'Declined by client',
  RevisionRequested: 'Revision requested',
  Superseded: 'Replaced by a newer version',
  Rejected: 'Rejected',
};

/** Readable names for the decisions stored on a quotation version. */
export const DECISION_LABELS: Record<string, string> = {
  Approved: 'Sent to the client',
  Accepted: 'Client accepted',
  Declined: 'Client declined',
  RevisionRequested: 'Manager asked for a revision',
  Rejected: 'Rejected by the operator',
  Superseded: 'Replaced by a re-priced version',
};

/** The banner at the top of the review page: what the trip's status means and what happens next. */
export const TRIP_STATUS_BANNERS: Partial<Record<TripRequestStatus, { title: string; body: string }>> = {
  PendingReview: {
    title: 'Pending review',
    body: 'The proposal is waiting for you. Send it to the client, edit it directly, ask the Planner agent for a revision, or reject it.',
  },
  RevisionRequested: {
    title: 'Re-planning',
    body: 'The Planner agent is re-planning with your comment. A new version comes back here for review.',
  },
  QuotationSent: {
    title: 'Waiting for the client',
    body: 'The tourist has the quotation in the app and can accept or decline it. Nothing is booked yet.',
  },
  ClientAccepted: {
    title: 'Client accepted',
    body: 'Confirm books the guide, vehicle and rooms, issues vouchers and emails the tourist — all in one step. Reopen review sends the trip back to you for changes.',
  },
  Confirmed: {
    title: 'Confirmed',
    body: 'The trip is booked: the guide, vehicle and rooms are held and the vouchers were issued.',
  },
  InProgress: { title: 'In progress', body: 'The trip has started.' },
  Completed: { title: 'Completed', body: 'The trip is finished.' },
  Cancelled: { title: 'Cancelled', body: 'This trip was cancelled. Nothing is held for it.' },
  FailedSafely: {
    title: 'Failed safely',
    body: 'Planning stopped without a usable proposal. The tourist can try again from the app.',
  },
};

/** "What happens next" under the review buttons (trip PendingReview). */
export const NEXT_STEPS: { action: string; text: string }[] = [
  {
    action: 'Send to client',
    text: 'the tourist gets the quotation in the app and can accept or decline. Nothing is booked yet.',
  },
  {
    action: 'Request revision',
    text: 'the Planner agent re-plans with your comment and a new version comes back for review.',
  },
  { action: 'Reject', text: 'the trip is cancelled and the tourist is told.' },
  {
    action: 'Edit directly',
    text: "change a day's stops or swap the guide, vehicle or hotel, then re-price.",
  },
  { action: 'Re-price', text: "makes a new version with today's rates." },
];

/**
 * Why "Send to client" is not possible right now, or null when it is. Mirrors the API's 409 rules for
 * POST /api/quotations/{id}/approve, so the manager sees the reason before clicking.
 */
export function sendBlockedReason(check: {
  workflowStatus: WorkflowStatus;
  editedSinceQuotation: boolean;
  quotationId: string | null | undefined;
  quotationStatus: QuotationStatus | undefined;
}): string | null {
  if (!check.quotationId) return 'There is no priced quotation to send yet.';
  if (check.workflowStatus === 'RevisionRequested')
    return 'A rule warning is open (for example the total is over the budget). Request a revision, or edit and re-price, before sending.';
  if (check.editedSinceQuotation)
    return 'The proposal was edited after it was priced. Re-price it first, so the client gets the right total.';
  if (check.quotationStatus === 'Declined')
    return 'The client declined this version. Edit and re-price it, or request a revision, to make a new version.';
  if (check.quotationStatus !== undefined && check.quotationStatus !== 'Pending')
    return `This version is already decided (${QUOTATION_STATUS_LABELS[check.quotationStatus].toLowerCase()}).`;
  return null;
}

/** The client's reason when the newest version was declined, or null. */
export function declineReason(quotation: QuotationDto | undefined): string | null {
  if (quotation?.status !== 'Declined') return null;
  const declined = quotation.decisions.filter((d) => d.decision === 'Declined').at(-1);
  return declined?.comment ?? 'No reason was given.';
}
