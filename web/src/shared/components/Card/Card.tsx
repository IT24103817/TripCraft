import { forwardRef } from 'react';
import { cn } from '../../utils/cn';
import type { CardProps } from './Card.types';

/** Surface panel (white, or deep slate in dark mode) with the Hallmark border and shadow. With a title it gets the title row: title left, actions right. */
export const Card = forwardRef<HTMLDivElement, CardProps>(
  ({ title, actions, className, children, ...rest }, ref) => (
    <div ref={ref} className={cn('card', className)} {...rest}>
      {(title || actions) && (
        <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
          {title && <h2 className="font-semibold text-slate-900">{title}</h2>}
          {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
        </div>
      )}
      {children}
    </div>
  ),
);

Card.displayName = 'Card';
