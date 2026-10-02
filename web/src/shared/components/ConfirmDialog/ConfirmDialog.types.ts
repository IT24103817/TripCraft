import type { ReactNode } from 'react';

export interface ConfirmDialogProps {
  open: boolean;
  title: string;
  message: ReactNode;
  confirmLabel: string;
  tone?: 'primary' | 'danger';
  isPending?: boolean;
  confirmDisabled?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
  /** Extra content, e.g. a comment box. */
  children?: ReactNode;
}
