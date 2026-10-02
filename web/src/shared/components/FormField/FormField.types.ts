import type { UseFormRegisterReturn } from 'react-hook-form';
import type { FilterOption } from '../SearchFilterBar';

export interface FormFieldProps {
  label: string;
  registration: UseFormRegisterReturn;
  error?: string;
  type?: string;
  as?: 'input' | 'textarea' | 'select';
  options?: FilterOption[];
  placeholder?: string;
  step?: string;
  autoComplete?: string;
  hint?: string;
  className?: string;
}
