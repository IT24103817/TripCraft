import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import type { PagedResult } from '@/shared/api/types';
import type {
  AttractionDto,
  AttractionListQuery,
  CancelTripRequest,
  ItineraryDto,
  SaveAttractionRequest,
  TripHistoryEntryDto,
  TripRequestDto,
  TripRequestListQuery,
  UpdateItineraryDayRequest,
} from './types';

/** Drops empty values (and empty lists) so the API only sees filters that are set. */
function clean<T extends object>(query: T): Partial<T> {
  return Object.fromEntries(
    Object.entries(query).filter(
      ([, v]) => v !== '' && v !== undefined && !(Array.isArray(v) && v.length === 0),
    ),
  ) as Partial<T>;
}

export const tripKeys = {
  list: (query: TripRequestListQuery) => [queryRoots.trips, 'list', query] as const,
  detail: (id: string) => [queryRoots.trips, 'detail', id] as const,
  itinerary: (id: string) => [queryRoots.trips, 'itinerary', id] as const,
  history: (id: string) => [queryRoots.trips, 'history', id] as const,
  workflow: (id: string) => [queryRoots.trips, 'workflow', id] as const,
};

/**
 * GET /api/trip-requests. `indexes: null` makes Axios send a list as cities=Kandy&cities=Ella (what ASP.NET
 * Core binds to a List), instead of its default cities[]=Kandy.
 */
export function useTrips(query: TripRequestListQuery) {
  return useQuery({
    queryKey: tripKeys.list(query),
    queryFn: async () =>
      (
        await http.get<PagedResult<TripRequestDto>>('/api/trip-requests', {
          params: clean(query),
          paramsSerializer: { indexes: null },
        })
      ).data,
    placeholderData: keepPreviousData,
  });
}

/** GET /api/attractions/cities: the cities TripCraft covers (for the trips list's city filter). */
export function useCities() {
  return useQuery({
    queryKey: [queryRoots.attractions, 'cities'],
    queryFn: async () => (await http.get<string[]>('/api/attractions/cities')).data,
    staleTime: 5 * 60_000,
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

/**
 * Cancel with a reason (a manager may cancel at any time before the trip starts; 409 otherwise). The holds are
 * released by the API in the same transaction. Refreshes the trip, its history and the lists.
 */
export function useCancelTrip(id: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (body: CancelTripRequest) =>
      (await http.post<TripRequestDto>(`/api/trip-requests/${id}/cancel`, body)).data,
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.trips] }),
  });
}

/**
 * The id of the trip's newest agent workflow (GET /api/trip-requests/{id}/workflow), so the trip page can link
 * to the review page. Only asked for when `enabled`; a 404 (no workflow yet) is returned as null.
 */
export function useTripWorkflowId(id: string, enabled: boolean) {
  return useQuery({
    queryKey: tripKeys.workflow(id),
    queryFn: async () => {
      const response = await http.get<{ id: string }>(`/api/trip-requests/${id}/workflow`, {
        validateStatus: (status) => status === 200 || status === 404,
      });
      return response.status === 404 ? null : response.data.id;
    },
    enabled,
  });
}

/**
 * The trip PDFs (GET /api/trips/{id}/vouchers.pdf and /itinerary.pdf) need the bearer token, so a plain link cannot
 * open them. The PDF is fetched through the API client as a blob and handed to the browser as a download.
 */
export async function downloadPdf(path: string, fileName: string): Promise<void> {
  const response = await http.get<Blob>(path, { responseType: 'blob' });
  const url = URL.createObjectURL(response.data);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
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

/** How many trips are in one status; only the total is used (dashboard KPI). */
export function useTripStatusCount(status: string, enabled = true) {
  return useQuery({
    queryKey: [queryRoots.trips, 'count', status],
    queryFn: async () =>
      (
        await http.get<PagedResult<TripRequestDto>>('/api/trip-requests', {
          params: { status, pageSize: 1 },
        })
      ).data.total,
    enabled,
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
