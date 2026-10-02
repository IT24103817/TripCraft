import { cn } from '../../utils/cn';
import type { PageHeaderProps } from './PageHeader.types';

/** The page's single h1, its description, and the page actions on the right. */
export const PageHeader = ({ title, description, actions, className }: PageHeaderProps) => (
  <div
    className={cn(
      'mb-6 flex flex-col gap-3 border-b border-slate-200 pb-5 sm:flex-row sm:items-end sm:justify-between',
      className,
    )}
  >
    <div>
      <h1 className="text-2xl font-semibold text-slate-900">{title}</h1>
      {description && <p className="mt-1 max-w-2xl text-sm text-slate-600">{description}</p>}
    </div>
    {actions && <div className="flex flex-wrap gap-2">{actions}</div>}
  </div>
);
