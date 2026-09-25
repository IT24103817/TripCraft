import { z } from 'zod';

/** Mirrors SaveAttractionRequestValidator in the API. */
export const attractionSchema = z.object({
  name: z.string().trim().min(1, 'Name is required.').max(200),
  city: z.string().trim().min(1, 'City is required.').max(100),
  category: z.string().trim().min(1, 'Category is required.').max(50),
  durationMinutes: z.coerce
    .number({ invalid_type_error: 'Enter a number.' })
    .int()
    .min(15, 'At least 15 minutes.')
    .max(600, 'At most 600 minutes.'),
  entryFeeLkr: z.coerce.number({ invalid_type_error: 'Enter a number.' }).min(0, 'Cannot be negative.'),
  latitude: z.coerce
    .number({ invalid_type_error: 'Enter a number.' })
    .min(-90)
    .max(90, 'Between -90 and 90.'),
  longitude: z.coerce
    .number({ invalid_type_error: 'Enter a number.' })
    .min(-180)
    .max(180, 'Between -180 and 180.'),
});

export type AttractionForm = z.infer<typeof attractionSchema>;
