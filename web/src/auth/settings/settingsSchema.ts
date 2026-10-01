import { z } from 'zod';

const percent = z.coerce
  .number({ invalid_type_error: 'Enter a number.' })
  .min(0, 'Must be 0–100.')
  .max(100, 'Must be 0–100.');

/** Mirrors the API's settings validator (docs/API-V11-WEB.md, "Settings"). */
export const settingsSchema = z.object({
  llmProvider: z.enum(['ollama', 'groq'], { message: 'Choose Ollama or Groq.' }),
  cancellationCutoffDays: z.coerce
    .number({ invalid_type_error: 'Enter a number.' })
    .int('Whole days only.')
    .min(0, 'Must be 0–30 days.')
    .max(30, 'Must be 0–30 days.'),
  marginPct: percent,
  depositPct: percent,
  operatorContact: z
    .string()
    .trim()
    .min(1, 'The operator contact is required.')
    .max(200, 'At most 200 characters.'),
});

export type SettingsForm = z.infer<typeof settingsSchema>;
