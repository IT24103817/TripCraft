/** Trip request status workflow (PLAN.md section 3, TripRequestStatus in C#). */
export const TRIP_STATUSES = [
  'Submitted',
  'Planning',
  'PendingApproval',
  'Approved',
  'Rejected',
  'RevisionRequested',
  'Confirmed',
  'InProgress',
  'Completed',
  'Cancelled',
] as const;
export type TripRequestStatus = (typeof TRIP_STATUSES)[number];

/** AgentWorkflowStatus in C#. */
export const WORKFLOW_STATUSES = [
  'Planning',
  'PendingApproval',
  'RevisionRequested',
  'Approved',
  'Rejected',
  'Completed',
  'FailedSafely',
] as const;
export type WorkflowStatus = (typeof WORKFLOW_STATUSES)[number];

type Tone = 'grey' | 'blue' | 'amber' | 'green' | 'red' | 'purple';

const TONE_BY_STATUS: Record<string, Tone> = {
  Submitted: 'grey',
  Planning: 'blue',
  PendingApproval: 'amber',
  RevisionRequested: 'purple',
  Approved: 'green',
  Confirmed: 'green',
  InProgress: 'blue',
  Completed: 'green',
  Rejected: 'red',
  Cancelled: 'grey',
  FailedSafely: 'red',
  Succeeded: 'green',
  Failed: 'red',
  Active: 'green',
  Inactive: 'grey',
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

/** "PendingApproval" -> "Pending approval". */
export function statusLabel(status: string): string {
  const spaced = status.replace(/([a-z])([A-Z])/g, '$1 $2').toLowerCase();
  return spaced.charAt(0).toUpperCase() + spaced.slice(1);
}
