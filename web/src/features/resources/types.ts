/** Shapes of the Resource Management API (backend/src/TripCraft.Application/Resources/Dtos). */

export type ResourceType = 'Guide' | 'Vehicle' | 'Room';

export interface GuideDto {
  id: string;
  name: string;
  phone: string;
  languages: string[];
  dayRateLkr: number;
  maxPax: number;
  isActive: boolean;
}

/** Body of PUT /api/guides/{id}. */
export interface SaveGuideRequest {
  name: string;
  phone: string;
  languages: string[];
  dayRateLkr: number;
  maxPax: number;
  isActive: boolean;
}

/** Body of POST /api/guides (v1.1): the guide and their Guide login are made together; email is the login. */
export interface CreateGuideRequest extends SaveGuideRequest {
  email: string;
}

/**
 * Returned once by create and by reset-password. The temporary password is never stored or shown again; the
 * guide must change it at the first login.
 */
export interface GuideAccountDto {
  guide: GuideDto;
  email: string;
  temporaryPassword: string;
}

/** A guide who could replace another (free for the whole trip, speaks the language, takes the party). */
export interface GuideOption {
  id: string;
  name: string;
  languages: string[];
  maxPax: number;
}

/** One open request from GET /api/guide-change-requests (a guide asked to be replaced on a confirmed trip). */
export interface GuideChangeRequestDto {
  id: string;
  tripRequestId: string;
  tripObjective: string;
  startDate: string;
  endDate: string;
  pax: number;
  language: string;
  guideId: string;
  guideName: string;
  reason: string;
  status: string;
  /** Set once resolved (the answer of POST .../resolve). */
  replacementGuideName?: string | null;
  candidates: GuideOption[];
}

export interface VehicleDto {
  id: string;
  registrationNo: string;
  type: string;
  seats: number;
  ratePerKmLkr: number;
  isActive: boolean;
}

export type SaveVehicleRequest = Omit<VehicleDto, 'id'>;

export interface RoomTypeDto {
  id: string;
  hotelId: string;
  name: string;
  capacity: number;
  ratePerNightLkr: number;
  totalRooms: number;
}

export interface HotelDto {
  id: string;
  name: string;
  city: string;
  starRating: number;
  latitude: number;
  longitude: number;
  isActive: boolean;
  roomTypes: RoomTypeDto[];
}

/**
 * One room type in the hotel body (docs/API-V11-WEB.md): with an id it updates that room type, without one it adds
 * a new one; a room type left out is deleted (409 if it has holds).
 */
export interface SaveHotelRoomType {
  id?: string;
  name: string;
  capacity: number;
  ratePerNightLkr: number;
  totalRooms: number;
}

/** Body of POST and PUT /api/hotels: the hotel and all its room types (at least one). */
export type SaveHotelRequest = Omit<HotelDto, 'id' | 'roomTypes'> & { roomTypes: SaveHotelRoomType[] };

export interface HoldDto {
  id: string;
  resourceType: ResourceType;
  resourceId: string;
  resourceName: string;
  tripRequestId: string | null;
  fromDate: string;
  toDate: string;
  quantity: number;
  status: 'Held' | 'Released';
  note: string | null;
}

export interface CreateHoldRequest {
  resourceType: ResourceType;
  resourceId: string;
  fromDate: string;
  toDate: string;
  quantity: number;
  note: string | null;
}

/** Query-string values; empty strings are dropped before calling the API. */
export type ListQuery = Record<string, string | number>;

/** Body of PUT /api/resource-holds/{id}: manual blocks only (a trip's hold answers 409). */
export interface UpdateHoldRequest {
  fromDate: string;
  toDate: string;
  quantity: number;
  note: string;
}

/**
 * One resource on one day in GET /api/availability/grid. Blocked = a manual hold (leave, maintenance);
 * Confirmed = held for a Confirmed, InProgress or Completed trip; Held = held for a trip in any other status.
 */
export type CellState = 'Free' | 'Held' | 'Confirmed' | 'Blocked';

export interface AvailabilityCellDto {
  date: string;
  state: CellState;
  holdId?: string | null;
  tripRequestId?: string | null;
  touristName?: string | null;
  tripStatus?: string | null;
  note?: string | null;
  heldQuantity: number;
  freeQuantity: number;
}

/** A guide, a vehicle or a hotel room type, with one cell per day. Room types: capacity = total rooms. */
export interface AvailabilityRowDto {
  resourceType: ResourceType;
  resourceId: string;
  name: string;
  detail: string;
  capacity: number;
  cells: AvailabilityCellDto[];
}

export interface AvailabilityGridDto {
  days: string[];
  rows: AvailabilityRowDto[];
}
