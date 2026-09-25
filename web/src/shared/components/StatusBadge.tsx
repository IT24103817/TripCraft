import { statusLabel, TONE_CLASSES, toneFor } from '../statuses';
import { cn } from '../utils/cn';

/** Coloured pill for any status in the PLAN.md status workflows. */
export function StatusBadge({ status, className }: { status: string; className?: string }) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset',
        TONE_CLASSES[toneFor(status)],
        className,
      )}
    >
      {statusLabel(status)}
    </span>
  );
}
