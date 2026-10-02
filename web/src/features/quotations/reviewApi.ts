import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import type { PagedResult } from '@/shared/api/types';
import type {
  AvailableOption,
  CityAttraction,
  EditableProposalDto,
  PlanExplanationDto,
  SwapResourcesRequest,
} from './types';

// The review and editing calls that are not a lifecycle step: "Edit" a day or the resources, the options to
// swap to, and "Why this plan". (Confirm, Send, Replan and Retry planning are in tripActionsApi.ts.)

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

/**
 * GET /api/trip-requests/{id}/plan-explanation: "Why this plan" in plain language. A 404 (no proposal yet) is
 * returned as null, so the review page simply hides the panel.
 */
export function usePlanExplanation(tripId: string | undefined) {
  return useQuery({
    queryKey: [queryRoots.trips, 'plan-explanation', tripId],
    queryFn: async () => {
      const response = await http.get<PlanExplanationDto>(`/api/trip-requests/${tripId}/plan-explanation`, {
        validateStatus: (status) => status === 200 || status === 404,
      });
      return response.status === 404 ? null : response.data;
    },
    enabled: Boolean(tripId),
  });
}
