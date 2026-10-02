import type { ButtonHTMLAttributes } from 'react';

/** primary = the one main action in a region; danger = destructive, always behind a confirmation. */
export type ButtonVariant = 'primary' | 'secondary' | 'danger';

/** sm is for tight rows such as table pagination. */
export type ButtonSize = 'md' | 'sm';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  /** Shows a spinner, disables the button and sets aria-busy while a request runs. */
  isLoading?: boolean;
  /** Label shown while loading, e.g. "Saving…". Without it the normal label stays. */
  loadingText?: string;
}
