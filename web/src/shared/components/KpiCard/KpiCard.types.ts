import type { ReactNode } from 'react';

export interface KpiCardProps {
  label: string;
  value: ReactNode;
  hint?: string;
  className?: string;
}
