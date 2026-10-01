import { statusLabel, TONE_CLASSES, toneFor } from '../statuses';
import { cn } from '../utils/cn';

/**
 * Coloured pill for any status in the PLAN.md status workflows. `label` replaces the generated text when one
 * status name means something else on a page (e.g. a quotation that is "Approved" was sent to the client).
 */
export function StatusBadge({
  status,
  label,
  className,
}: {
  status: string;
  label?: string;
  className?: string;
}) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset',
        TONE_CLASSES[toneFor(status)],
        className,
      )}
    >
      {label ?? statusLabel(status)}
    </span>
  );
}
