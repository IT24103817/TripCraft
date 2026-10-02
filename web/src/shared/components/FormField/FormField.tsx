import { cn } from '../../utils/cn';
import type { FormFieldProps } from './FormField.types';

/**
 * A labelled input wired to react-hook-form (validation comes from the form's zod schema).
 * The error is announced and linked with aria-describedby; the .input class turns the border red via aria-invalid.
 */
export const FormField = ({
  label,
  registration,
  error,
  type = 'text',
  as = 'input',
  options = [],
  hint,
  className,
  ...rest
}: FormFieldProps) => {
  const id = `field-${registration.name}`;
  const describedBy =
    [error ? `${id}-error` : null, hint ? `${id}-hint` : null].filter(Boolean).join(' ') || undefined;
  const common = {
    id,
    'aria-invalid': error ? true : undefined,
    'aria-describedby': describedBy,
    className: 'input',
    ...registration,
  };

  return (
    <div className={cn('flex flex-col gap-1', className)}>
      <label htmlFor={id} className="text-sm font-medium text-slate-700">
        {label}
      </label>
      {as === 'textarea' ? (
        <textarea rows={3} placeholder={rest.placeholder} {...common} />
      ) : as === 'select' ? (
        <select {...common}>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      ) : (
        <input type={type} {...rest} {...common} />
      )}
      {hint && (
        <p id={`${id}-hint`} className="text-xs text-slate-500">
          {hint}
        </p>
      )}
      {error && (
        <p id={`${id}-error`} role="alert" className="text-xs font-medium text-red-700">
          {error}
        </p>
      )}
    </div>
  );
};
