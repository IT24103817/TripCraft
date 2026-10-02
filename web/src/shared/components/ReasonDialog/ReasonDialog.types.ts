import type { ReactNode } from 'react';

export interface ReasonDialogProps {
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
  className?: string;
}
