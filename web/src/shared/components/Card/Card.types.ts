import type { HTMLAttributes, ReactNode } from 'react';

export interface CardProps extends Omit<HTMLAttributes<HTMLDivElement>, 'title'> {
  /** Card title (an h2), shown top left. */
  title?: string;
  /** Shown top right of the title row, e.g. a status badge or a button. */
  actions?: ReactNode;
}
