import type { TripRequestStatus } from '@/shared/statuses';

/** What each status means for the Operations Manager, shown under the status timeline. */
export const STATUS_NOTES: Record<TripRequestStatus, string> = {
  Submitted: 'Waiting for the tourist to start planning in the mobile app.',
  Planning: 'The agents are planning this trip. It comes back for your review when they finish.',
  PendingReview:
    'The proposal is waiting for your review: send it to the client, edit it, or ask for a revision.',
  QuotationSent:
    'Waiting for the client to accept or decline the quotation in the app. Nothing is booked yet.',
  ClientAccepted: 'The client accepted the quotation. Open the review to confirm and book the trip.',
  Confirmed: 'Booked: the guide, vehicle and rooms are held and the vouchers were issued.',
  InProgress: 'The trip has started: the guide is checking in at the stops.',
  Completed: 'The trip is finished.',
  Cancelled: 'This trip was cancelled. The reason is in the history below.',
  RevisionRequested:
    'The Planner agent is re-planning with the comment; a new version comes back for review.',
  FailedSafely: 'Planning stopped safely without a usable proposal. The tourist can try again from the app.',
};

/** A manager may cancel in these statuses (TripStatusMachine: every status that can move to Cancelled). */
export const CANCELLABLE: TripRequestStatus[] = [
  'Submitted',
  'FailedSafely',
  'PendingReview',
  'RevisionRequested',
  'QuotationSent',
  'ClientAccepted',
  'Confirmed',
];

/** Statuses in which the review page has something to show or do. */
export const IN_REVIEW: TripRequestStatus[] = [
  'PendingReview',
  'RevisionRequested',
  'QuotationSent',
  'ClientAccepted',
];

/** Vouchers exist once the trip is confirmed. */
export const HAS_VOUCHERS: TripRequestStatus[] = ['Confirmed', 'InProgress', 'Completed'];

/** A quotation has been sent to the client, so the itinerary PDF exists (the API answers 409 before that). */
export const QUOTATION_SENT_OR_LATER: TripRequestStatus[] = [
  'QuotationSent',
  'ClientAccepted',
  'Confirmed',
  'InProgress',
  'Completed',
];
