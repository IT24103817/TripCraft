import { Button } from '../Button';
import { Dialog } from '../Dialog';
import type { ConfirmDialogProps } from './ConfirmDialog.types';

export const ConfirmDialog = (props: ConfirmDialogProps) => (
  <Dialog open={props.open} title={props.title} onClose={props.onCancel}>
    <div className="space-y-4 text-sm text-slate-700">
      <div>{props.message}</div>
      {props.children}
      <div className="flex justify-end gap-2 border-t border-slate-100 pt-4">
        <Button variant="secondary" onClick={props.onCancel}>
          Cancel
        </Button>
        <Button
          variant={props.tone === 'danger' ? 'danger' : 'primary'}
          isLoading={props.isPending}
          loadingText="Working…"
          disabled={props.confirmDisabled}
          onClick={props.onConfirm}
        >
          {props.confirmLabel}
        </Button>
      </div>
    </div>
  </Dialog>
);
