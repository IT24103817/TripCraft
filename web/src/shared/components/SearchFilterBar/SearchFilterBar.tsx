import { useEffect, useState } from 'react';
import { useDebouncedValue } from '../../hooks/useDebouncedValue';
import { cn } from '../../utils/cn';
import { Card } from '../Card';
import type { SearchFilterBarProps } from './SearchFilterBar.types';

/** Debounced search box, filter selects and a date range. Each change goes straight to the list's URL params. */
export const SearchFilterBar = ({
  search,
  filters = [],
  dateRange,
  children,
  className,
}: SearchFilterBarProps) => {
  const [text, setText] = useState(search?.value ?? '');
  const debounced = useDebouncedValue(text, 300);
  const onSearchChange = search?.onChange;
  const currentSearch = search?.value;

  // The search was changed outside the box (e.g. a "Clear filters" button): show the new value.
  // Done while rendering (React's "adjust state when a prop changes" pattern) so the effect below
  // never sends the old text back. A value this box sent itself equals `debounced` and is skipped.
  const [lastSearch, setLastSearch] = useState(currentSearch);
  if (currentSearch !== lastSearch) {
    setLastSearch(currentSearch);
    if (currentSearch !== debounced) setText(currentSearch ?? '');
  }

  // Send the typed text once the user has paused (debounced has caught up with the box).
  useEffect(() => {
    if (onSearchChange && debounced === text && debounced !== currentSearch) onSearchChange(debounced);
  }, [debounced, text, currentSearch, onSearchChange]);

  return (
    <Card
      role="search"
      className={cn('flex flex-col gap-3 md:flex-row md:flex-wrap md:items-end', className)}
    >
      {search && (
        <label className="flex min-w-0 flex-1 flex-col gap-1 text-sm font-medium text-slate-700">
          Search
          <input
            type="search"
            className="input font-normal"
            value={text}
            placeholder={search.placeholder ?? 'Search…'}
            onChange={(e) => setText(e.target.value)}
          />
        </label>
      )}
      {filters.map((filter) => (
        <label key={filter.name} className="flex flex-col gap-1 text-sm font-medium text-slate-700">
          {filter.label}
          <select
            className="input font-normal"
            value={filter.value}
            onChange={(e) => filter.onChange(e.target.value)}
          >
            <option value="">All</option>
            {filter.options.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
      ))}
      {dateRange && (
        <fieldset className="flex flex-col gap-1 text-sm text-slate-700">
          <legend className="mb-1 font-medium">{dateRange.label}</legend>
          <div className="flex flex-wrap gap-2">
            <label className="flex items-center gap-1">
              <span className="sr-only">{dateRange.label} from</span>
              <input
                type="date"
                className="input"
                aria-label={`${dateRange.label} from`}
                value={dateRange.from}
                onChange={(e) => dateRange.onChange(e.target.value, dateRange.to)}
              />
            </label>
            <label className="flex items-center gap-1">
              <span className="sr-only">{dateRange.label} to</span>
              <input
                type="date"
                className="input"
                aria-label={`${dateRange.label} to`}
                value={dateRange.to}
                min={dateRange.from || undefined}
                onChange={(e) => dateRange.onChange(dateRange.from, e.target.value)}
              />
            </label>
          </div>
        </fieldset>
      )}
      {children}
    </Card>
  );
};
