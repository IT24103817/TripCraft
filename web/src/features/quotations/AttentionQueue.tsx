import { Link } from 'react-router-dom';
import { PageState } from '@/shared/components/PageState';
import { TabList } from '@/shared/components/TabList';
import { formatDate, formatDateTime, formatUsd } from '@/shared/utils/format';
import { ATTENTION_TABS, useAttention } from './attentionApi';
import type { AttentionItemDto, AttentionStatus } from './types';

interface Props {
  selected: AttentionStatus;
  onSelect: (status: AttentionStatus) => void;
  /** Accessible name of the tab row. */
  label: string;
}

/**
 * Trips that need the Operations Manager, one tab per status (Accepted, Declined, Needs operator), from
 * GET /api/dashboard/attention. Each row links to the trip page, where the manager acts.
 */
export function AttentionQueue({ selected, onSelect, label }: Props) {
  const tab = ATTENTION_TABS.find((t) => t.id === selected) ?? ATTENTION_TABS[0]!;
  const items = useAttention(tab.id);

  return (
    <div className="space-y-3">
      <TabList
        label={label}
        tabs={ATTENTION_TABS}
        selected={tab.id}
        onSelect={(id) => onSelect(id as AttentionStatus)}
      />
      <div role="tabpanel" id={`panel-${tab.id}`} aria-labelledby={`tab-${tab.id}`}>
        <PageState
          isLoading={items.isLoading}
          isError={items.isError}
          error={items.error}
          onRetry={() => items.refetch()}
          isEmpty={items.data?.length === 0}
          emptyTitle="Nothing is waiting here"
          emptyDescription={tab.empty}
        >
          <ul aria-label={`${tab.label} trips`} className="card divide-y divide-slate-200 p-0">
            {items.data?.map((item) => (
              <AttentionRow key={item.tripRequestId} item={item} />
            ))}
          </ul>
        </PageState>
      </div>
    </div>
  );
}

function AttentionRow({ item }: { item: AttentionItemDto }) {
  return (
    <li className="space-y-1 px-5 py-3 text-sm">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <Link
          to={`/trips/${item.tripRequestId}`}
          className="font-medium text-brand-700 hover:underline focus:outline-none focus-visible:ring-2 focus-visible:ring-brand-500"
        >
          {item.objective}
        </Link>
        <span className="font-medium tabular-nums text-slate-900">
          {item.totalUsd === null ? 'Not priced' : formatUsd(item.totalUsd)}
        </span>
      </div>
      <p className="text-slate-600">
        {item.touristName} · {formatDate(item.startDate)} – {formatDate(item.endDate)} · {item.pax} travellers
      </p>
      {item.detail && <p className="text-slate-800">{item.detail}</p>}
      <p className="text-xs text-slate-500">Waiting since {formatDateTime(item.since)}</p>
    </li>
  );
}
