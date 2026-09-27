import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import type { PagedResult } from '@/shared/api/types';
import type {
  AgentStepDto,
  QuotationDecisionResponse,
  TripSummary,
  WorkflowDto,
  WorkflowListQuery,
  WorkflowSummaryDto,
} from './types';

export const POLL_MS = 5000;

export const workflowKeys = {
  list: (query: WorkflowListQuery) => [queryRoots.workflows, 'list', query] as const,
  detail: (id: string) => [queryRoots.workflows, 'detail', id] as const,
  steps: (id: string) => [queryRoots.workflows, 'steps', id] as const,
};

/**
 * GET /api/workflows (staff): status filter, search on the trip objective, sort (startedAt, finishedAt,
 * status) and paging. Empty filters are left out, so the API uses its default sort ("-startedAt").
 */
export function useWorkflows(query: WorkflowListQuery) {
  return useQuery({
    queryKey: workflowKeys.list(query),
    queryFn: async () =>
      (
        await http.get<PagedResult<WorkflowSummaryDto>>('/api/workflows', {
          params: {
            status: query.status || undefined,
            search: query.search || undefined,
            sort: query.sort || undefined,
            page: query.page,
            pageSize: query.pageSize,
          },
        })
      ).data,
    placeholderData: keepPreviousData,
  });
}

/** Refreshes every 5 s while the agents are still planning. */
export function useWorkflow(id: string) {
  return useQuery({
    queryKey: workflowKeys.detail(id),
    queryFn: async () => (await http.get<WorkflowDto>(`/api/workflows/${id}`)).data,
    refetchInterval: (query) => (query.state.data?.status === 'Planning' ? POLL_MS : false),
  });
}

/** Only runs once the workflow itself has loaded (no point asking for steps of a missing workflow). */
export function useWorkflowSteps(id: string, options: { enabled: boolean; poll: boolean }) {
  return useQuery({
    queryKey: workflowKeys.steps(id),
    queryFn: async () => (await http.get<AgentStepDto[]>(`/api/workflows/${id}/steps`)).data,
    enabled: options.enabled,
    refetchInterval: options.poll ? POLL_MS : false,
  });
}

export function useWorkflowCount(status: string) {
  return useQuery({
    queryKey: [queryRoots.workflows, 'count', status],
    queryFn: async () =>
      (await http.get<PagedResult<WorkflowSummaryDto>>('/api/workflows', { params: { status, pageSize: 1 } }))
        .data.total,
  });
}

export function useTripSummary(tripId: string | undefined) {
  return useQuery({
    queryKey: [queryRoots.trips, 'detail', tripId],
    queryFn: async () => (await http.get<TripSummary>(`/api/trip-requests/${tripId}`)).data,
    enabled: Boolean(tripId),
  });
}

type Decision = 'approve' | 'reject' | 'request-revision';

/** Approve / reject / request revision. Refreshes workflows and trips afterwards (status changed on both). */
export function useQuotationDecision() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async ({
      quotationId,
      decision,
      comment,
    }: {
      quotationId: string;
      decision: Decision;
      comment?: string;
    }) =>
      (
        await http.post<QuotationDecisionResponse>(
          `/api/quotations/${quotationId}/${decision}`,
          comment ? { comment } : {},
        )
      ).data,
    onSuccess: () => {
      void client.invalidateQueries({ queryKey: [queryRoots.workflows] });
      void client.invalidateQueries({ queryKey: [queryRoots.trips] });
      void client.invalidateQueries({ queryKey: [queryRoots.reports] });
      void client.invalidateQueries({ queryKey: ['quotations'] });
    },
  });
}
