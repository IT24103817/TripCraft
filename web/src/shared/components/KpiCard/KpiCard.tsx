import { cn } from '../../utils/cn';
import { Card } from '../Card';
import type { KpiCardProps } from './KpiCard.types';

/** One headline number for a report. A brand stripe on the left marks it as a KPI. */
export const KpiCard = ({ label, value, hint, className }: KpiCardProps) => (
  <Card className={cn('border-l-4 border-l-brand-700', className)}>
    <p className="text-xs font-medium uppercase tracking-wide text-slate-500">{label}</p>
    <p className="mt-1 text-2xl font-semibold tabular-nums text-slate-900">{value}</p>
    {hint && <p className="mt-1 text-xs text-slate-500">{hint}</p>}
  </Card>
);
