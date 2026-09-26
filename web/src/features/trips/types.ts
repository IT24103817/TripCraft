import type { TripRequestStatus } from '@/shared/statuses';

// Taken from backend/src/TripCraft.Application/Trips/Dtos. DateOnly -> "yyyy-MM-dd", Guid -> string.

export interface TripRequestDto {
  id: string;
  touristId: string;
  objective: string;
  startDate: string;
  endDate: string;
  pax: number;
  budgetUsd: number;
  preferences: Record<string, unknown>;
  status: TripRequestStatus;
  createdAt: string;
  updatedAt: string;
}

/** Query of GET /api/trip-requests. Sortable: createdAt, startDate, budgetUsd, pax, status. Search matches the objective. */
export interface TripRequestListQuery {
  status?: string;
  from?: string;
  to?: string;
  search?: string;
  sort?: string;
  page: number;
  pageSize: number;
}

export interface ItineraryStopDto {
  sequence: number;
  arrivalTime: string | null;
  attractionId: string;
  attractionName: string;
  durationMinutes: number;
}

export interface ItineraryDayDto {
  dayNumber: number;
  city: string;
  hotelId: string | null;
  notes: string | null;
  stops: ItineraryStopDto[];
}

export interface ItineraryDto {
  id: string;
  tripRequestId: string;
  version: number;
  generatedBy: string;
  days: ItineraryDayDto[];
}

export interface AttractionDto {
  id: string;
  name: string;
  city: string;
  category: string;
  durationMinutes: number;
  entryFeeLkr: number;
  latitude: number;
  longitude: number;
}

/** Query of GET /api/attractions. Sortable: name, city, category, durationMinutes, entryFeeLkr. Search matches the name. */
export interface AttractionListQuery {
  city?: string;
  category?: string;
  search?: string;
  sort?: string;
  page: number;
  pageSize: number;
}

/** Body of POST and PUT /api/attractions. */
export type SaveAttractionRequest = Omit<AttractionDto, 'id'>;

/** One event from GET /api/trip-requests/{id}/history. Actor is a role or "System". */
export interface TripHistoryEntryDto {
  at: string;
  action: string;
  entity: string;
  actor: string;
  fromStatus: string | null;
  toStatus: string | null;
}
