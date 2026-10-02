import { statusLabel, TONE_CLASSES, toneFor } from '../../statuses';
import { cn } from '../../utils/cn';
import type { StatusBadgeProps } from './StatusBadge.types';

/** Coloured pill for any status in the PLAN.md status workflows. The dot repeats the tone; the text carries the meaning. */
export const StatusBadge = ({ status, label, className }: StatusBadgeProps) => (
  <span
    className={cn(
      'inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset',
      TONE_CLASSES[toneFor(status)],
      className,
    )}
  >
    <span className="h-1.5 w-1.5 rounded-full bg-current" aria-hidden="true" />
    {label ?? statusLabel(status)}
  </span>
);
