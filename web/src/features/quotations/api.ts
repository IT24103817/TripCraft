import { keepPreviousData, useMutation, useQueries, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import type { PagedResult } from '@/shared/api/types';
import { TRIP_STATUSES } from '@/shared/statuses';
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

/** GET /api/workflows (staff). The API filters by status only and sorts newest first. */
export function useWorkflows(query: WorkflowListQuery) {
  return useQuery({
    queryKey: workflowKeys.list(query),
    queryFn: async () =>
      (
        await http.get<PagedResult<WorkflowSummaryDto>>('/api/workflows', {
          params: { status: query.status || undefined, page: query.page, pageSize: query.pageSize },
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
    },
  });
}

/** Requests by status for trips starting in [from, to]: one count query per status (the API returns totals). */
export function useTripStatusCounts(from: string, to: string) {
  return useQueries({
    queries: TRIP_STATUSES.map((status) => ({
      queryKey: [queryRoots.reports, 'trips-by-status', status, from, to],
      queryFn: async () =>
        (
          await http.get<PagedResult<unknown>>('/api/trip-requests', {
            params: { status, from: from || undefined, to: to || undefined, pageSize: 1 },
          })
        ).data.total,
    })),
    combine: (results) => ({
      data: results.every((r) => r.isSuccess)
        ? TRIP_STATUSES.map((status, i) => ({ status, count: results[i]?.data ?? 0 }))
        : undefined,
      isLoading: results.some((r) => r.isLoading),
      isError: results.some((r) => r.isError),
      error: results.find((r) => r.error)?.error,
      refetch: () => results.forEach((r) => void r.refetch()),
    }),
  });
}
