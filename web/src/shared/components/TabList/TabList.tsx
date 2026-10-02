import { cn } from '../../utils/cn';
import type { TabListProps } from './TabList.types';

/**
 * A row of tabs. Each tab button has the id `tab-{id}` and controls the panel `panel-{id}`, which the page
 * renders itself (role="tabpanel", aria-labelledby="tab-{id}").
 */
export const TabList = ({ label, tabs, selected, onSelect, className }: TabListProps) => (
  <div role="tablist" aria-label={label} className={cn('flex gap-2 border-b border-slate-200', className)}>
    {tabs.map((tab) => (
      <button
        key={tab.id}
        id={`tab-${tab.id}`}
        type="button"
        role="tab"
        aria-selected={selected === tab.id}
        aria-controls={`panel-${tab.id}`}
        className={cn(
          '-mb-px border-b-2 px-3 py-2 text-sm font-medium transition-colors duration-150 focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500',
          selected === tab.id
            ? 'border-brand-700 text-brand-700'
            : 'border-transparent text-slate-600 hover:text-slate-900',
        )}
        onClick={() => onSelect(tab.id)}
      >
        {tab.label}
      </button>
    ))}
  </div>
);
