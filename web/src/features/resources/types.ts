/** Shapes of the Resource Management API (backend/src/TripCraft.Application/Resources/Dtos). */

export type ResourceType = 'Guide' | 'Vehicle' | 'Room';

export interface GuideDto {
  id: string;
  userId: string | null;
  name: string;
  phone: string;
  languages: string[];
  dayRateLkr: number;
  maxPax: number;
  isActive: boolean;
}

export interface SaveGuideRequest {
  name: string;
  phone: string;
  languages: string[];
  dayRateLkr: number;
  maxPax: number;
  isActive: boolean;
  userId: string | null;
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
