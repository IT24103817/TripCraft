import { z } from 'zod';

const money = (max: number) =>
  z.coerce.number({ invalid_type_error: 'Enter a number.' }).positive('Must be more than 0.').max(max);
const whole = (min: number, max: number) =>
  z.coerce.number({ invalid_type_error: 'Enter a number.' }).int().min(min).max(max);

/** Mirrors SaveGuideRequestValidator. Languages are typed as "en, de". */
export const guideSchema = z.object({
  name: z.string().trim().min(2, 'At least 2 characters.').max(100),
  phone: z
    .string()
    .trim()
    .regex(/^\+?[0-9 ]{7,20}$/, 'Phone must be 7–20 digits, optionally starting with +.'),
  languages: z
    .string()
    .trim()
    .min(1, 'At least one language.')
    .refine(
      (v) =>
        v
          .split(',')
          .map((s) => s.trim())
          .every((code) => /^[a-zA-Z]{2}$/.test(code)),
      'Use two-letter codes separated by commas, e.g. en, de.',
    ),
  dayRateLkr: money(1_000_000),
  maxPax: whole(1, 50),
  isActive: z.boolean(),
});
export type GuideForm = z.infer<typeof guideSchema>;

export const VEHICLE_TYPES = ['Car', 'Van', 'Coach'] as const;

/** Mirrors SaveVehicleRequestValidator. */
export const vehicleSchema = z.object({
  registrationNo: z
    .string()
    .trim()
    .min(3, 'At least 3 characters.')
    .max(20)
    .regex(/^[A-Za-z0-9-]+$/, "Letters, digits and '-' only."),
  type: z.enum(VEHICLE_TYPES),
  seats: whole(1, 60),
  ratePerKmLkr: money(10_000),
  isActive: z.boolean(),
});
export type VehicleForm = z.infer<typeof vehicleSchema>;

/** Mirrors SaveHotelRequestValidator (coordinates must be in Sri Lanka). */
export const hotelSchema = z.object({
  name: z.string().trim().min(2).max(150),
  city: z.string().trim().min(2).max(100),
  starRating: whole(1, 5),
  latitude: z.coerce
    .number()
    .min(5.5, 'Latitude must be in Sri Lanka (5.5–10.0).')
    .max(10, 'Latitude must be in Sri Lanka (5.5–10.0).'),
  longitude: z.coerce
    .number()
    .min(79.4, 'Longitude must be in Sri Lanka (79.4–82.1).')
    .max(82.1, 'Longitude must be in Sri Lanka (79.4–82.1).'),
  isActive: z.boolean(),
});
export type HotelForm = z.infer<typeof hotelSchema>;

/** Mirrors SaveRoomTypeRequestValidator. */
export const roomTypeSchema = z.object({
  name: z.string().trim().min(2).max(60),
  capacity: whole(1, 8),
  ratePerNightLkr: money(1_000_000),
  totalRooms: whole(1, 500),
});
export type RoomTypeForm = z.infer<typeof roomTypeSchema>;
