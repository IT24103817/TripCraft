import { useQuery } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import type { AttentionItemDto, AttentionStatus } from './types';

/** The three kinds of trip that wait for the manager, in the order the tabs show them. */
export const ATTENTION_TABS: { id: AttentionStatus; label: string; empty: string }[] = [
  { id: 'ClientAccepted', label: 'Accepted', empty: 'No accepted quotations to confirm.' },
  { id: 'ClientDeclined', label: 'Declined', empty: 'No declined quotations waiting for a decision.' },
  {
    id: 'NeedsOperator',
    label: 'Needs operator',
    empty: 'No trips are stuck: the agents handled everything.',
  },
];

/** A tab id from the URL, or the first tab when the value is missing or old (e.g. "PendingApproval"). */
export function toAttentionStatus(value: string | null): AttentionStatus {
  return ATTENTION_TABS.find((t) => t.id === value)?.id ?? 'ClientAccepted';
}

/**
 * GET /api/dashboard/attention?status=…: the trips in one status that need the manager, oldest first.
 * Under the trips root, so any lifecycle step (which refreshes trips) refreshes these lists too.
 */
export function useAttention(status: AttentionStatus) {
  return useQuery({
    queryKey: [queryRoots.trips, 'attention', status],
    queryFn: async () =>
      (await http.get<AttentionItemDto[]>('/api/dashboard/attention', { params: { status } })).data,
  });
}
