import type { TripRequestStatus } from '@/shared/statuses';

/** What each status means for the Operations Manager, shown under the status timeline. */
export const STATUS_NOTES: Record<TripRequestStatus, string> = {
  Submitted: 'Waiting for the tourist to start planning in the mobile app.',
  Planning:
    'The agents are planning this trip. A proposal that passes the checks is sent to the client automatically.',
  QuotationSent:
    'Waiting for the client to accept or decline the quotation in the app. Nothing is booked yet.',
  ClientAccepted: 'The client accepted the quotation. Confirm it to book the guide, vehicle and rooms.',
  ClientDeclined: 'The client declined the quotation. Replan it with a note, or cancel the trip.',
  NeedsOperator:
    'The agents could not produce a quotation that passes the checks, so nothing was sent. It needs you.',
  Confirmed: 'Booked: the guide, vehicle and rooms are held and the vouchers were issued.',
  InProgress: 'The trip has started: the guide is checking in at the stops.',
  Completed: 'The trip is finished.',
  Cancelled: 'This trip was cancelled. The reason is in the history below.',
};

/** A manager may cancel in these statuses (TripStatusMachine: every status that can move to Cancelled). */
export const CANCELLABLE: TripRequestStatus[] = [
  'Submitted',
  'NeedsOperator',
  'QuotationSent',
  'ClientAccepted',
  'ClientDeclined',
  'Confirmed',
];

/**
 * In these statuses the trip's action panel offers "Cancel with reason" next to the other choices, so the page
 * header does not repeat a Cancel button.
 */
export const CANCEL_IN_ACTION_PANEL: TripRequestStatus[] = ['ClientDeclined', 'NeedsOperator'];

/** Statuses in which the agents' proposal exists, so the review page (why this plan, checks, versions) is useful. */
export const HAS_PROPOSAL: TripRequestStatus[] = [
  'QuotationSent',
  'ClientAccepted',
  'ClientDeclined',
  'NeedsOperator',
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
