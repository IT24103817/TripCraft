import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import type { PagedResult } from '@/shared/api/types';
import type {
  AvailabilityGridDto,
  CreateGuideRequest,
  CreateHoldRequest,
  GuideAccountDto,
  GuideChangeRequestDto,
  GuideDto,
  HoldDto,
  HotelDto,
  ListQuery,
  SaveGuideRequest,
  SaveHotelRequest,
  SaveVehicleRequest,
  UpdateHoldRequest,
  VehicleDto,
} from './types';

const clean = (query: ListQuery) => Object.fromEntries(Object.entries(query).filter(([, v]) => v !== ''));

/** Paged list of one resource kind (guides, vehicles or hotels) with search, filters, sort and paging. */
function useList<T>(path: string, query: ListQuery) {
  return useQuery({
    queryKey: [queryRoots.resources, path, clean(query)],
    queryFn: async () => (await http.get<PagedResult<T>>(path, { params: clean(query) })).data,
    placeholderData: keepPreviousData,
  });
}

/** Create (no id) or update (with id); refreshes every resource query afterwards. */
function useSave<TBody, TResult>(path: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id?: string; body: TBody }) =>
      id
        ? (await http.put<TResult>(`${path}/${id}`, body)).data
        : (await http.post<TResult>(path, body)).data,
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.resources] }),
  });
}

function useDelete(path: string) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await http.delete(`${path}/${id}`);
    },
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.resources] }),
  });
}

export const useGuides = (query: ListQuery) => useList<GuideDto>('/api/guides', query);
export const useDeleteGuide = () => useDelete('/api/guides');

/** POST /api/guides: creates the guide and their login. The answer holds the one-time temporary password. */
export function useCreateGuide() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateGuideRequest) =>
      (await http.post<GuideAccountDto>('/api/guides', body)).data,
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.resources] }),
  });
}

/** PUT /api/guides/{id}: the guide's details (the login is not changed here). */
export function useUpdateGuide() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id: string; body: SaveGuideRequest }) =>
      (await http.put<GuideDto>(`/api/guides/${id}`, body)).data,
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.resources] }),
  });
}

/** POST /api/guides/{id}/reset-password: a new one-time temporary password; the old one stops working. */
export function useResetGuidePassword() {
  return useMutation({
    mutationFn: async (id: string) =>
      (await http.post<GuideAccountDto>(`/api/guides/${id}/reset-password`)).data,
  });
}

/** GET /api/guide-change-requests: the open requests, each with the guides who could take over. */
export function useGuideChangeRequests() {
  return useQuery({
    queryKey: [queryRoots.resources, 'guide-change-requests'],
    queryFn: async () => (await http.get<GuideChangeRequestDto[]>('/api/guide-change-requests')).data,
  });
}

/** POST /api/guide-change-requests/{id}/resolve: swaps the guide's holds to the replacement. */
export function useResolveGuideChange() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, replacementGuideId }: { id: string; replacementGuideId: string }) =>
      (
        await http.post<GuideChangeRequestDto>(`/api/guide-change-requests/${id}/resolve`, {
          replacementGuideId,
        })
      ).data,
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.resources] }),
  });
}

export const useVehicles = (query: ListQuery) => useList<VehicleDto>('/api/vehicles', query);
export const useSaveVehicle = () => useSave<SaveVehicleRequest, VehicleDto>('/api/vehicles');
export const useDeleteVehicle = () => useDelete('/api/vehicles');

export const useHotels = (query: ListQuery) => useList<HotelDto>('/api/hotels', query);
export const useSaveHotel = () => useSave<SaveHotelRequest, HotelDto>('/api/hotels');
export const useDeleteHotel = () => useDelete('/api/hotels');

export function useCreateHold() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateHoldRequest) =>
      (await http.post<HoldDto>('/api/resource-holds', body)).data,
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.resources] }),
  });
}

export function useReleaseHold() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => (await http.post<HoldDto>(`/api/resource-holds/${id}/release`)).data,
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.resources] }),
  });
}

/**
 * GET /api/availability/grid: every guide, vehicle and room type with its state on each day of [from, to]
 * (at most 62 days). Optional filters: type, language (guides), seats (vehicles, minimum) and city (rooms).
 */
export function useAvailabilityGrid(query: ListQuery) {
  return useQuery({
    queryKey: [queryRoots.resources, 'availability-grid', clean(query)],
    queryFn: async () =>
      (await http.get<AvailabilityGridDto>('/api/availability/grid', { params: clean(query) })).data,
    placeholderData: keepPreviousData,
  });
}

/** GET /api/resource-holds/{id}: one hold, e.g. a manual block to edit. */
export function useHold(id: string | null | undefined) {
  return useQuery({
    queryKey: [queryRoots.resources, 'hold', id],
    queryFn: async () => (await http.get<HoldDto>(`/api/resource-holds/${id}`)).data,
    enabled: Boolean(id),
  });
}

/** PUT /api/resource-holds/{id}: change a manual block's dates, quantity or note. */
export function useUpdateHold() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: async ({ id, body }: { id: string; body: UpdateHoldRequest }) =>
      (await http.put<HoldDto>(`/api/resource-holds/${id}`, body)).data,
    onSuccess: () => client.invalidateQueries({ queryKey: [queryRoots.resources] }),
  });
}

/** GET /api/attractions/cities: the cities TripCraft covers (the grid's city filter for room types). */
export function useCoveredCities() {
  return useQuery({
    queryKey: [queryRoots.attractions, 'cities'],
    queryFn: async () => (await http.get<string[]>('/api/attractions/cities')).data,
    staleTime: 5 * 60_000,
  });
}
