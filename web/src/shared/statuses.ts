/**
 * Trip request lifecycle v1.1 (TripRequestStatus in C#, docs/API-V11.md "Trip statuses"). Main path first, then
 * the side states. Quotations go to the client automatically; the manager's gate is Confirm.
 */
export const TRIP_STATUSES = [
  'Submitted',
  'Planning',
  'QuotationSent',
  'ClientAccepted',
  'Confirmed',
  'InProgress',
  'Completed',
  'ClientDeclined',
  'NeedsOperator',
  'Cancelled',
] as const;
export type TripRequestStatus = (typeof TRIP_STATUSES)[number];

/** AgentWorkflowStatus in C#. RevisionRequested was retired in v1.1: a re-plan is simply Planning again. */
export const WORKFLOW_STATUSES = [
  'Planning',
  'PendingApproval',
  'Approved',
  'Rejected',
  'Completed',
  'FailedSafely',
] as const;
export type WorkflowStatus = (typeof WORKFLOW_STATUSES)[number];

type Tone = 'grey' | 'blue' | 'amber' | 'green' | 'red' | 'purple';

const TONE_BY_STATUS: Record<string, Tone> = {
  // Trip statuses
  Submitted: 'grey',
  Planning: 'blue',
  QuotationSent: 'blue',
  ClientAccepted: 'purple',
  Confirmed: 'green',
  InProgress: 'blue',
  Completed: 'green',
  ClientDeclined: 'amber',
  NeedsOperator: 'red',
  Cancelled: 'grey',
  // Workflow and quotation statuses
  PendingApproval: 'amber',
  FailedSafely: 'red',
  Approved: 'green',
  Rejected: 'red',
  Pending: 'amber',
  Declined: 'red',
  Superseded: 'grey',
  // Agent steps and resources
  Succeeded: 'green',
  Failed: 'red',
  Active: 'green',
  Inactive: 'grey',
  // Deposit payment
  Paid: 'green',
  Unpaid: 'amber',
  // Availability grid cells
  Free: 'grey',
  Held: 'amber',
  Blocked: 'red',
};

export const TONE_CLASSES: Record<Tone, string> = {
  grey: 'bg-slate-100 text-slate-700 ring-slate-300',
  blue: 'bg-blue-50 text-blue-700 ring-blue-300',
  amber: 'bg-amber-50 text-amber-800 ring-amber-300',
  green: 'bg-green-50 text-green-700 ring-green-300',
  red: 'bg-red-50 text-red-700 ring-red-300',
  purple: 'bg-purple-50 text-purple-700 ring-purple-300',
};

export function toneFor(status: string): Tone {
  return TONE_BY_STATUS[status] ?? 'grey';
}

/** Trip statuses whose name reads badly when only spaced out ("Client accepted" is just "Accepted" to staff). */
const LABELS: Record<string, string> = {
  ClientAccepted: 'Accepted',
  ClientDeclined: 'Declined',
};

/** "QuotationSent" -> "Quotation sent", "NeedsOperator" -> "Needs operator", "ClientAccepted" -> "Accepted". */
export function statusLabel(status: string): string {
  const known = LABELS[status];
  if (known) return known;
  const spaced = status.replace(/([a-z])([A-Z])/g, '$1 $2').toLowerCase();
  return spaced.charAt(0).toUpperCase() + spaced.slice(1);
}
