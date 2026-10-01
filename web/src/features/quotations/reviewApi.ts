import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import type { PagedResult } from '@/shared/api/types';
import { invalidateAfterDecision } from './api';
import type {
  AvailableOption,
  CityAttraction,
  EditableProposalDto,
  QuotationDecisionResponse,
  SwapResourcesRequest,
} from './types';

// The v1.1 review calls that are not a quotation decision: Confirm, Reopen review and "Edit directly".

/** POST /api/trip-requests/{id}/confirm (ClientAccepted): holds, itinerary, vouchers and email in one step. */
export function useConfirmTrip(tripId: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async () =>
      (await http.post<QuotationDecisionResponse>(`/api/trip-requests/${tripId}/confirm`)).data,
    onSuccess: () => invalidateAfterDecision(client),
  });
}

/** POST /api/trip-requests/{id}/reopen-review {reason} (ClientAccepted → PendingReview). */
export function useReopenReview(tripId: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (reason: string) =>
      (
        await http.post<QuotationDecisionResponse>(`/api/trip-requests/${tripId}/reopen-review`, {
          reason,
        })
      ).data,
    onSuccess: () => invalidateAfterDecision(client),
  });
}

/** PUT /api/trip-requests/{id}/proposal/days/{n}: 1–3 attractions of that day's city, in visiting order. */
export function useEditProposalDay(tripId: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async ({ dayNumber, attractionIds }: { dayNumber: number; attractionIds: string[] }) =>
      (
        await http.put<EditableProposalDto>(`/api/trip-requests/${tripId}/proposal/days/${dayNumber}`, {
          attractionIds,
        })
      ).data,
    // The workflow holds the edited proposal (and editedSinceQuotation), so it is loaded again.
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.workflows] }),
  });
}

/** PUT /api/trip-requests/{id}/proposal/resources: swap the guide, the vehicle or a city's room type. */
export function useSwapResources(tripId: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (body: SwapResourcesRequest) =>
      (await http.put<EditableProposalDto>(`/api/trip-requests/${tripId}/proposal/resources`, body)).data,
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.workflows] }),
  });
}

/** The active attractions of one city (for the proposal day editor). */
export function useCityAttractions(city: string) {
  return useQuery({
    queryKey: [queryRoots.attractions, 'city', city],
    queryFn: async () =>
      (
        await http.get<PagedResult<CityAttraction>>('/api/attractions', {
          params: { city, sort: 'name', page: 1, pageSize: 100 },
        })
      ).data.items,
  });
}

/**
 * GET /api/availability: the free guides (language, pax), vehicles (seats) or room types (city, rooms on every
 * night) for the trip's dates. Rendered only inside "Edit directly", so it is asked for only when needed.
 */
export function useAvailableOptions(params: Record<string, string | number>) {
  return useQuery({
    queryKey: [queryRoots.resources, 'availability', params],
    queryFn: async () => (await http.get<AvailableOption[]>('/api/availability', { params })).data,
  });
}
