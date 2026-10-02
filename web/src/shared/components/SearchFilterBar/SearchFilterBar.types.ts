import type { ReactNode } from 'react';

export interface FilterOption {
  value: string;
  label: string;
}

export interface FilterSelect {
  name: string;
  label: string;
  value: string;
  options: FilterOption[];
  onChange: (value: string) => void;
}

export interface SearchFilterBarProps {
  /** Omit when the API has no search for this list. */
  search?: { value: string; placeholder?: string; onChange: (value: string) => void };
  filters?: FilterSelect[];
  dateRange?: { label: string; from: string; to: string; onChange: (from: string, to: string) => void };
  /** Extra labelled controls a list needs (e.g. a number filter), shown at the end of the same bar. */
  children?: ReactNode;
  className?: string;
}
