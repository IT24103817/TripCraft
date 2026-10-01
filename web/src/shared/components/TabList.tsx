import { cn } from '../utils/cn';

export interface TabItem {
  id: string;
  label: string;
}

interface TabListProps {
  label: string;
  tabs: TabItem[];
  selected: string;
  onSelect: (id: string) => void;
}

/**
 * A row of tabs. Each tab button has the id `tab-{id}` and controls the panel `panel-{id}`, which the page
 * renders itself (role="tabpanel", aria-labelledby="tab-{id}").
 */
export function TabList({ label, tabs, selected, onSelect }: TabListProps) {
  return (
    <div role="tablist" aria-label={label} className="flex gap-2 border-b border-slate-200">
      {tabs.map((tab) => (
        <button
          key={tab.id}
          id={`tab-${tab.id}`}
          type="button"
          role="tab"
          aria-selected={selected === tab.id}
          aria-controls={`panel-${tab.id}`}
          className={cn(
            '-mb-px border-b-2 px-3 py-2 text-sm font-medium focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500',
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
}
