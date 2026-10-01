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
  /** The cities the trip visits, in order (v1.1). */
  cities: string[];
}

/**
 * Query of GET /api/trip-requests. Sortable: createdAt, startDate, budgetUsd, pax, status. Search matches the
 * objective. `cities` keeps only trips that visit every listed city (sent as cities=A&cities=B).
 */
export interface TripRequestListQuery {
  status?: string;
  cities?: string[];
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

/**
 * Body of PUT /api/trip-requests/{id}/itinerary/days/{dayNumber} (Operations Manager, Confirmed trips only):
 * 1–3 distinct active attractions in the day's city, in visiting order, and notes of at most 500 characters.
 */
export interface UpdateItineraryDayRequest {
  attractionIds: string[];
  notes: string | null;
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
  /** Why the status changed (v1.1), e.g. "Declined by the client: too expensive". */
  reason?: string | null;
}

/** Body of POST /api/trip-requests/{id}/cancel: the reason is required (at most 500 characters). */
export interface CancelTripRequest {
  reason: string;
}
