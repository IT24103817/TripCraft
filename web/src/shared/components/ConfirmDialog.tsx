import type { ReactNode } from 'react';
import { Dialog } from './Dialog';

interface ConfirmDialogProps {
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

export function ConfirmDialog(props: ConfirmDialogProps) {
  return (
    <Dialog open={props.open} title={props.title} onClose={props.onCancel}>
      <div className="space-y-4 text-sm text-slate-700">
        <div>{props.message}</div>
        {props.children}
        <div className="flex justify-end gap-2">
          <button type="button" className="btn-secondary" onClick={props.onCancel}>
            Cancel
          </button>
          <button
            type="button"
            className={props.tone === 'danger' ? 'btn-danger' : 'btn-primary'}
            disabled={props.isPending || props.confirmDisabled}
            onClick={props.onConfirm}
          >
            {props.isPending ? 'Working…' : props.confirmLabel}
          </button>
        </div>
      </div>
    </Dialog>
  );
}
