import type { ReactNode } from 'react';

export interface DialogProps {
  open: boolean;
  title: string;
  onClose: () => void;
  children: ReactNode;
  /** "wide" for forms with a table (e.g. a hotel and its room types). */
  size?: 'default' | 'wide';
  /** "side" slides in from the right as a panel (e.g. one availability cell). */
  placement?: 'center' | 'side';
  className?: string;
}
