import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { http } from '@/shared/api/http';
import { queryRoots } from '@/shared/api/queryKeys';
import type { PagedResult } from '@/shared/api/types';
import type {
  AvailableResourceDto,
  CreateGuideRequest,
  CreateHoldRequest,
  GuideAccountDto,
  GuideChangeRequestDto,
  GuideDto,
  HoldDto,
  HotelDto,
  ListQuery,
  RoomTypeDto,
  SaveGuideRequest,
  SaveHotelRequest,
  SaveRoomTypeRequest,
  SaveVehicleRequest,
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

export function useSaveRoomType(hotelId: string) {
  return useSave<SaveRoomTypeRequest, RoomTypeDto>(`/api/hotels/${hotelId}/room-types`);
}

export function useDeleteRoomType(hotelId: string) {
  return useDelete(`/api/hotels/${hotelId}/room-types`);
}

/** GET /api/availability — only runs once the search form has been submitted (query not null). */
export function useAvailability(query: ListQuery | null) {
  return useQuery({
    queryKey: [queryRoots.resources, 'availability', query],
    queryFn: async () =>
      (await http.get<AvailableResourceDto[]>('/api/availability', { params: clean(query ?? {}) })).data,
    enabled: query !== null,
  });
}

/** The most holds the calendar asks for in one window (the API's largest page size). */
export const HOLDS_PAGE_SIZE = 100;

/**
 * Held holds overlapping [from, to], first page only. The result keeps the API's `total`, so the
 * calendar can say when there are more holds than it shows instead of dropping them silently.
 */
export function useHolds(from: string, to: string) {
  return useQuery({
    queryKey: [queryRoots.resources, 'holds', from, to],
    queryFn: async () =>
      (
        await http.get<PagedResult<HoldDto>>('/api/resource-holds', {
          params: { from, to, status: 'Held', pageSize: HOLDS_PAGE_SIZE },
        })
      ).data,
  });
}

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
