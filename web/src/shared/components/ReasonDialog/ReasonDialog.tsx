import { useId, useState } from 'react';
import { cn } from '../../utils/cn';
import { ConfirmDialog } from '../ConfirmDialog';
import type { ReasonDialogProps } from './ReasonDialog.types';

/**
 * A confirmation that asks for a reason or a comment (cancel a trip, replan with a note, send with a comment…).
 * Rendered only while open, so the text box starts empty every time.
 */
export const ReasonDialog = (props: ReasonDialogProps) => {
  const id = useId();
  const [text, setText] = useState('');
  const [error, setError] = useState<string | null>(null);

  const confirm = () => {
    const trimmed = text.trim();
    if (props.required && trimmed === '') {
      setError(props.requiredMessage ?? 'A reason is required.');
      return;
    }
    props.onConfirm(trimmed === '' ? undefined : trimmed);
  };

  return (
    <ConfirmDialog
      open
      title={props.title}
      message={props.message}
      confirmLabel={props.confirmLabel}
      tone={props.tone}
      isPending={props.isPending}
      onCancel={props.onCancel}
      onConfirm={confirm}
    >
      <div className={cn('flex flex-col gap-1', props.className)}>
        <label htmlFor={id} className="font-medium">
          {props.fieldLabel} {props.required ? '(required)' : '(optional)'}
        </label>
        <textarea
          id={id}
          rows={3}
          className="input"
          maxLength={props.maxLength}
          value={text}
          aria-invalid={error ? true : undefined}
          aria-describedby={error ? `${id}-error` : undefined}
          onChange={(e) => {
            setText(e.target.value);
            setError(null);
          }}
        />
        {error && (
          <p id={`${id}-error`} role="alert" className="text-xs font-medium text-red-700">
            {error}
          </p>
        )}
      </div>
    </ConfirmDialog>
  );
};
