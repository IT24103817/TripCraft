import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import { invalidateTripLifecycle } from './api';
import type { QuotationDecisionResponse, RepriceResponse, WorkflowDto } from './types';

// The Operations Manager's lifecycle steps on one trip (docs/API-V11.md "Operations Manager actions").

/**
 * The trip's newest agent workflow (GET /api/trip-requests/{id}/workflow): its proposal, the error summary,
 * the validation result and whether the proposal was edited since it was priced. 404 (none yet) is null.
 * Kept under the workflows root, so editing the proposal refreshes it.
 */
export function useTripWorkflow(tripId: string, enabled: boolean) {
  return useQuery({
    queryKey: [queryRoots.workflows, 'trip', tripId],
    queryFn: async () => {
      const response = await http.get<WorkflowDto>(`/api/trip-requests/${tripId}/workflow`, {
        validateStatus: (status) => status === 200 || status === 404,
      });
      return response.status === 404 ? null : response.data;
    },
    enabled,
  });
}

/** POST /api/trip-requests/{id}/confirm (ClientAccepted): holds, itinerary, vouchers and email in one step. */
export function useConfirmTrip(tripId: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async () =>
      (await http.post<QuotationDecisionResponse>(`/api/trip-requests/${tripId}/confirm`)).data,
    onSuccess: () => invalidateTripLifecycle(client),
  });
}

/**
 * POST /api/quotations/{id}/send {comment?}: sends a re-priced version to the client (Edit & resend, Edit & send
 * manually). The trip becomes QuotationSent and the tourist is asked to accept it.
 */
export function useSendQuotation() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async ({ quotationId, comment }: { quotationId: string; comment?: string }) =>
      (
        await http.post<QuotationDecisionResponse>(
          `/api/quotations/${quotationId}/send`,
          comment ? { comment } : {},
        )
      ).data,
    onSuccess: () => invalidateTripLifecycle(client),
  });
}

/** POST /api/trip-requests/{id}/replan {note} (ClientDeclined → Planning). The note goes to the Planner agent. */
export function useReplanTrip(tripId: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (note: string) =>
      (await http.post<QuotationDecisionResponse>(`/api/trip-requests/${tripId}/replan`, { note })).data,
    onSuccess: () => invalidateTripLifecycle(client),
  });
}

/** POST /api/trip-requests/{id}/start-planning (NeedsOperator → Planning): "Retry planning". */
export function useRetryPlanning(tripId: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async () => {
      await http.post(`/api/trip-requests/${tripId}/start-planning`);
    },
    onSuccess: () => invalidateTripLifecycle(client),
  });
}

/**
 * POST /api/trip-requests/{id}/proposal/reprice: prices the trip's (edited) proposal as a new version that is
 * not sent yet. Used when the trip has no quotation to re-price (a Hard rule stopped the agents' one).
 */
export function useRepriceTrip(tripId: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async () =>
      (await http.post<RepriceResponse>(`/api/trip-requests/${tripId}/proposal/reprice`)).data,
    onSuccess: () => {
      void client.invalidateQueries({ queryKey: [queryRoots.quotations] });
      void client.invalidateQueries({ queryKey: [queryRoots.workflows] });
    },
  });
}
