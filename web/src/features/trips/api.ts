import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import type { PagedResult } from '@/shared/api/types';
import type {
  AttractionDto,
  AttractionListQuery,
  ItineraryDto,
  SaveAttractionRequest,
  TripHistoryEntryDto,
  TripRequestDto,
  TripRequestListQuery,
  UpdateItineraryDayRequest,
} from './types';

/** Drops empty values so the API only sees filters that are set. */
function clean<T extends object>(query: T): Partial<T> {
  return Object.fromEntries(
    Object.entries(query).filter(([, v]) => v !== '' && v !== undefined),
  ) as Partial<T>;
}

export const tripKeys = {
  list: (query: TripRequestListQuery) => [queryRoots.trips, 'list', query] as const,
  detail: (id: string) => [queryRoots.trips, 'detail', id] as const,
  itinerary: (id: string) => [queryRoots.trips, 'itinerary', id] as const,
  history: (id: string) => [queryRoots.trips, 'history', id] as const,
};

export function useTrips(query: TripRequestListQuery) {
  return useQuery({
    queryKey: tripKeys.list(query),
    queryFn: async () =>
      (await http.get<PagedResult<TripRequestDto>>('/api/trip-requests', { params: clean(query) })).data,
    placeholderData: keepPreviousData,
  });
}

export function useTrip(id: string) {
  return useQuery({
    queryKey: tripKeys.detail(id),
    queryFn: async () => (await http.get<TripRequestDto>(`/api/trip-requests/${id}`)).data,
  });
}

/** 404 means "no itinerary yet" — returned as null instead of an error. */
export function useItinerary(id: string) {
  return useQuery({
    queryKey: tripKeys.itinerary(id),
    queryFn: async () => {
      const response = await http.get<ItineraryDto>(`/api/trip-requests/${id}/itinerary`, {
        validateStatus: (status) => status === 200 || status === 404,
      });
      return response.status === 404 ? null : response.data;
    },
  });
}

/**
 * Replaces one day's stops and notes (the itinerary editor). The API answers with the whole itinerary
 * (version + 1, generatedBy "Manual"), which is shown at once; the trip's queries are then refreshed
 * because the change is also in the trip history.
 */
export function useUpdateItineraryDay(tripId: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async ({ dayNumber, body }: { dayNumber: number; body: UpdateItineraryDayRequest }) =>
      (await http.put<ItineraryDto>(`/api/trip-requests/${tripId}/itinerary/days/${dayNumber}`, body)).data,
    onSuccess: (itinerary) => {
      client.setQueryData(tripKeys.itinerary(tripId), itinerary);
      void client.invalidateQueries({ queryKey: [queryRoots.trips] });
    },
  });
}

/** Audit events of the trip and its agent workflows, oldest first. */
export function useTripHistory(id: string) {
  return useQuery({
    queryKey: tripKeys.history(id),
    queryFn: async () => (await http.get<TripHistoryEntryDto[]>(`/api/trip-requests/${id}/history`)).data,
  });
}

/** Submitted → Cancelled (409 in any other status). Refreshes the trip, its history and the lists. */
export function useCancelTrip(id: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async () => (await http.post<TripRequestDto>(`/api/trip-requests/${id}/cancel`)).data,
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.trips] }),
  });
}

/** Trips starting in [from, to]; only the total is used (dashboard KPI). */
export function useTripCount(from: string, to: string) {
  return useQuery({
    queryKey: [queryRoots.trips, 'count', from, to],
    queryFn: async () =>
      (
        await http.get<PagedResult<TripRequestDto>>('/api/trip-requests', {
          params: { from, to, pageSize: 1 },
        })
      ).data.total,
  });
}

export const attractionKeys = {
  list: (query: AttractionListQuery) => [queryRoots.attractions, 'list', query] as const,
  all: [queryRoots.attractions, 'all'] as const,
};

export function useAttractions(query: AttractionListQuery) {
  return useQuery({
    queryKey: attractionKeys.list(query),
    queryFn: async () =>
      (await http.get<PagedResult<AttractionDto>>('/api/attractions', { params: clean(query) })).data,
    placeholderData: keepPreviousData,
  });
}

/** Up to 100 attractions, used to build the city and category filter options from real data. */
export function useAttractionFacets() {
  return useQuery({
    queryKey: attractionKeys.all,
    queryFn: async () => {
      const { items } = (
        await http.get<PagedResult<AttractionDto>>('/api/attractions', { params: { pageSize: 100 } })
      ).data;
      return {
        cities: [...new Set(items.map((a) => a.city))].sort(),
        categories: [...new Set(items.map((a) => a.category))].sort(),
      };
    },
    staleTime: 5 * 60_000,
  });
}

export function useSaveAttraction() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id?: string; body: SaveAttractionRequest }) =>
      id
        ? (await http.put<AttractionDto>(`/api/attractions/${id}`, body)).data
        : (await http.post<AttractionDto>('/api/attractions', body)).data,
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.attractions] }),
  });
}

export function useDeleteAttraction() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await http.delete(`/api/attractions/${id}`);
    },
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.attractions] }),
  });
}
