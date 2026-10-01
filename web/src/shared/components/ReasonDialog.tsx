import { useId, useState, type ReactNode } from 'react';
import { ConfirmDialog } from './ConfirmDialog';

interface ReasonDialogProps {
  title: string;
  message: ReactNode;
  confirmLabel: string;
  /** Label of the text box, e.g. "Reason" or "Comment". */
  fieldLabel: string;
  /** When true an empty text is refused with `requiredMessage`. */
  required: boolean;
  requiredMessage?: string;
  /** The API's maximum length (FluentValidation MaximumLength). */
  maxLength: number;
  tone?: 'primary' | 'danger';
  isPending?: boolean;
  /** Called with the trimmed text, or undefined when an optional text was left empty. */
  onConfirm: (text: string | undefined) => void;
  onCancel: () => void;
}

/**
 * A confirmation that asks for a reason or a comment (cancel a trip, request a revision, reopen a review…).
 * Rendered only while open, so the text box starts empty every time.
 */
export function ReasonDialog(props: ReasonDialogProps) {
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
      <div className="flex flex-col gap-1">
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
          <p id={`${id}-error`} role="alert" className="text-xs text-red-700">
            {error}
          </p>
        )}
      </div>
    </ConfirmDialog>
  );
}
