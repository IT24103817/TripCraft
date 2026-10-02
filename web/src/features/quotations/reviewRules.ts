import type { TripRequestStatus } from '@/shared/statuses';
import { formatUsd } from '@/shared/utils/format';
import type { QuotationDto, QuotationStatus } from './types';

/** What a quotation status means to the manager (an "Approved" quotation was sent to the client). */
export const QUOTATION_STATUS_LABELS: Record<QuotationStatus, string> = {
  Pending: 'Not sent yet',
  Approved: 'Sent to client',
  Declined: 'Declined by client',
  Superseded: 'Replaced by a newer version',
  Rejected: 'Rejected',
};

/** Readable names for the decisions stored on a quotation version. */
export const DECISION_LABELS: Record<string, string> = {
  Approved: 'Sent to the client',
  Accepted: 'Client accepted',
  Declined: 'Client declined',
  Confirmed: 'Confirmed by the operator',
  RevisionRequested: 'Manager asked for a revision (before v1.1)',
  Rejected: 'Rejected by the operator',
  Superseded: 'Replaced by a re-priced version',
};

/** A short "what happens next" banner per trip status (the trip page's action panel and the review page). */
export const NEXT_STEP_BANNERS: Partial<Record<TripRequestStatus, { title: string; body: string }>> = {
  Planning: {
    title: 'The agents are planning',
    body: 'A proposal that passes the checks is sent to the client automatically. If it cannot be, the trip comes back to you as "Needs operator".',
  },
  QuotationSent: {
    title: 'Waiting for the client',
    body: 'The tourist has the quotation in the app and can accept or decline it. Nothing is booked yet.',
  },
  ClientAccepted: {
    title: 'Accepted — confirm to book',
    body: 'Confirm holds the guide, vehicle and rooms, issues the vouchers and emails the tourist, all in one step. If something must change first, use Edit & resend: the tourist will be asked to accept the updated quote.',
  },
  ClientDeclined: {
    title: 'Declined — needs a decision',
    body: 'Replan with a note for the Planner agent (the new version goes to the client automatically), or cancel the trip with a reason.',
  },
  NeedsOperator: {
    title: 'Needs operator',
    body: 'The agents could not produce a quotation that passes the checks, so nothing was sent. Retry planning, edit the proposal and send it yourself, or cancel the trip.',
  },
  Confirmed: {
    title: 'Confirmed',
    body: 'The trip is booked: the guide, vehicle and rooms are held and the vouchers were issued.',
  },
  InProgress: { title: 'In progress', body: 'The trip has started.' },
  Completed: { title: 'Completed', body: 'The trip is finished.' },
  Cancelled: { title: 'Cancelled', body: 'This trip was cancelled. Nothing is held for it.' },
};

/** The trip statuses in which the manager has something to do on the trip page. */
export const ACTION_STATUSES: TripRequestStatus[] = ['ClientAccepted', 'ClientDeclined', 'NeedsOperator'];

/** True when the client accepted this version (the API sets acceptedAt). */
export function isAccepted(quotation: QuotationDto | undefined): boolean {
  return Boolean(quotation?.acceptedAt);
}

/**
 * Why Confirm is not possible right now, or null when it is. Mirrors the API's 409 rules for
 * POST /api/trip-requests/{id}/confirm: the newest version must be the accepted one, and nothing may have been
 * edited since it was priced.
 */
export function confirmBlockedReason(check: {
  newest: QuotationDto | undefined;
  editedSinceQuotation: boolean;
}): string | null {
  if (!check.newest) return 'There is no quotation to confirm yet.';
  if (check.editedSinceQuotation)
    return 'The proposal was edited after it was priced. Re-price it and send it to the client; they must accept it before you can confirm.';
  if (!isAccepted(check.newest))
    return `Version ${check.newest.version} has not been accepted by the client. Send it to the client; they must accept it before you can confirm.`;
  return null;
}

/**
 * Why "Send to client" (POST /api/quotations/{id}/send) is not possible right now, or null when it is: only a
 * re-priced version that was not sent yet can be sent, and only when nothing was edited after that price.
 */
export function sendBlockedReason(check: {
  newest: QuotationDto | undefined;
  editedSinceQuotation: boolean;
}): string | null {
  if (check.editedSinceQuotation)
    return 'The proposal was edited after it was priced. Re-price it first, so the client gets the right total.';
  if (!check.newest) return 'There is no priced version yet. Re-price the proposal first.';
  if (check.newest.status !== 'Pending')
    return `Version ${check.newest.version} is already ${QUOTATION_STATUS_LABELS[check.newest.status].toLowerCase()}. Edit or re-price to make a new version to send.`;
  return null;
}

/** The client's reason when the version was declined, or null. */
export function declineReason(quotation: QuotationDto | undefined): string | null {
  if (quotation?.status !== 'Declined') return null;
  const declined = quotation.decisions.filter((d) => d.decision === 'Declined').at(-1);
  return declined?.comment ?? 'No reason was given.';
}

/**
 * The best-price sentence of a version that is still over the budget after the agents' lowest-cost re-plans,
 * or null when the version is within budget. The API's own sentence is used when it sends one.
 */
export function budgetNoteText(quotation: QuotationDto): string | null {
  if (!quotation.bestAvailablePrice) return null;
  if (quotation.budgetNote) return quotation.budgetNote;
  return quotation.overBudgetUsd
    ? `Best price we can offer — ${formatUsd(quotation.overBudgetUsd)} above your budget`
    : 'Best price we can offer';
}

/** The API accepts a deposit payment change only once the client has accepted the newest version. */
export const PAYMENT_STATUSES: TripRequestStatus[] = [
  'ClientAccepted',
  'Confirmed',
  'InProgress',
  'Completed',
];
