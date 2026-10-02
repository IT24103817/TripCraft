import type { ReactNode } from 'react';

export interface PageHeaderProps {
  title: string;
  description?: string;
  /** Buttons shown on the right, e.g. "Add attraction". */
  actions?: ReactNode;
  className?: string;
}
