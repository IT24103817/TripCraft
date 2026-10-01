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

export type SaveRoomTypeRequest = Omit<RoomTypeDto, 'id' | 'hotelId'>;

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

export type SaveHotelRequest = Omit<HotelDto, 'id' | 'roomTypes'>;

export interface AvailableResourceDto {
  type: ResourceType;
  id: string;
  name: string;
  detail: string;
  rateLkr: number;
  freeRooms: number | null;
}

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
