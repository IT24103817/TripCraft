import type { TripRequestStatus } from '@/shared/statuses';
import { statusLabel } from '@/shared/statuses';
import { cn } from '@/shared/utils/cn';

/** Main path of the status workflow in PLAN.md section 3. */
const MAIN_PATH: TripRequestStatus[] = [
  'Submitted',
  'Planning',
  'PendingApproval',
  'Confirmed',
  'InProgress',
  'Completed',
];

/** Side statuses and the main-path step they follow. */
const SIDE_STEP: Partial<Record<TripRequestStatus, TripRequestStatus>> = {
  Approved: 'PendingApproval',
  RevisionRequested: 'PendingApproval',
  Rejected: 'PendingApproval',
  Cancelled: 'Submitted',
};

export function StatusTimeline({ status }: { status: TripRequestStatus }) {
  const reachedStep = SIDE_STEP[status] ?? status;
  const reachedIndex = MAIN_PATH.indexOf(reachedStep);
  const isSide = status in SIDE_STEP;
  const steps = isSide ? [...MAIN_PATH.slice(0, reachedIndex + 1), status] : MAIN_PATH;

  return (
    <ol aria-label="Status timeline" className="flex flex-wrap gap-x-2 gap-y-3">
      {steps.map((step, index) => {
        const current = step === status;
        const done = index < steps.indexOf(status);
        return (
          <li
            key={step}
            aria-current={current ? 'step' : undefined}
            className="flex items-center gap-2 text-sm"
          >
            <span
              className={cn(
                'flex h-6 w-6 items-center justify-center rounded-full text-xs font-semibold',
                current
                  ? 'bg-indigo-600 text-white'
                  : done
                    ? 'bg-green-600 text-white'
                    : 'bg-slate-200 text-slate-600',
              )}
              aria-hidden="true"
            >
              {done ? '✓' : index + 1}
            </span>
            <span className={cn(current ? 'font-semibold text-slate-900' : 'text-slate-600')}>
              {statusLabel(step)}
            </span>
            {index < steps.length - 1 && (
              <span aria-hidden="true" className="hidden h-px w-6 bg-slate-300 sm:block" />
            )}
          </li>
        );
      })}
    </ol>
  );
}
