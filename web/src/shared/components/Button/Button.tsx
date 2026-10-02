import { forwardRef } from 'react';
import { cn } from '../../utils/cn';
import type { ButtonProps, ButtonSize, ButtonVariant } from './Button.types';

// The look lives in index.css (.btn-*), so <Button> and a <Link className="btn-primary"> always match.
const VARIANT_CLASSES: Record<ButtonVariant, string> = {
  primary: 'btn-primary',
  secondary: 'btn-secondary',
  danger: 'btn-danger',
};

const SIZE_CLASSES: Record<ButtonSize, string> = {
  md: '',
  sm: 'px-2 py-1',
};

const Spinner = () => (
  <svg viewBox="0 0 24 24" className="h-4 w-4 animate-spin" aria-hidden="true">
    <circle cx="12" cy="12" r="9" fill="none" stroke="currentColor" strokeWidth="3" className="opacity-25" />
    <path d="M21 12a9 9 0 0 0-9-9" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" />
  </svg>
);

/** The Hallmark button. type defaults to "button" so it never submits a form by accident. */
export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      variant = 'primary',
      size = 'md',
      isLoading = false,
      loadingText,
      type = 'button',
      disabled,
      className,
      children,
      ...rest
    },
    ref,
  ) => (
    <button
      ref={ref}
      type={type}
      disabled={disabled || isLoading}
      aria-busy={isLoading || undefined}
      className={cn(VARIANT_CLASSES[variant], SIZE_CLASSES[size], className)}
      {...rest}
    >
      {isLoading && <Spinner />}
      {isLoading && loadingText ? loadingText : children}
    </button>
  ),
);

Button.displayName = 'Button';
